using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnManager : MonoBehaviour
{
    List<BaseNPC> _active_monsterlist = new List<BaseNPC>();
    [SerializeField] Transform[] _createpoint;
    [SerializeField] Transform[] _protectpoint;

    //죽은 몬스터 수와 상관없이 60초마다 몬스터 생성

    Vector2 MAXDOWNPOINT = new Vector2(-4.6f, -1.2f);

    Vector2 MAXUPPOINT = new Vector2(1.8f, 2.5f);

    const float NEXTSTAGETIME = 5f;
    int counts = 0;

    void OnEnable()
    {
        PlayManager._ongamestatechanged += CreateMonster;
        Monster_Base._monster_die_animation_exit += (diemon) => RemoveMonsterList(diemon);
        BaseSkill._skill_target_dictionary_list.Add(ETARGETKIND.MONSTER, _active_monsterlist);
    }

    void OnDisable()
    {
        for (int i = 0; i < _active_monsterlist.Count; i++)
        {
            if (_active_monsterlist[i] == null)
            {
                continue;
            }
            Destroy(_active_monsterlist[i].gameObject);
        }
        _active_monsterlist.Clear();

        PlayManager._ongamestatechanged -= CreateMonster;
        Monster_Base._monster_die_animation_exit -= (diemon) => RemoveMonsterList(diemon);
        BaseSkill._skill_target_dictionary_list.Remove(ETARGETKIND.MONSTER);
    }


    async void CreateMonster(GameStateData gamestate)
    {
        if (gamestate._state != EPLAYSTATE.PLAY)
        {
            return;
        }

        while (true)
        {
            // 게임 종료 체크
            if (!await TryValidateChapter())
                return;

            // 챕터 완료 체크
            if (!await TryValidateStage())
                continue;

            // 몬스터 스폰
            await SpawnCurrentStageMonsters();

            // 다음 스테이지 대기
            await WaitForNextStage();

            PlayManager.TriggerStageNext();
        }
    }

    /// <summary>
    /// 현재 챕터가 유효한지 검증하고, 유효하지 않으면 게임 종료 처리
    /// </summary>
    async UniTask<bool> TryValidateChapter()
    {
        var chapterid = PlayManager.instance.GetCurrentChapterID();
        var currentchapterdata = DataManager.instance.GetChapterData(chapterid);

        //더 이상 진행할 챕터 없을 경우 종료
        if (currentchapterdata._stagedata == null || currentchapterdata._stagedata.Count <= 0)
        {
            await UniTask.WaitUntil(() => _active_monsterlist.Count <= 0, cancellationToken: this.GetCancellationTokenOnDestroy());
            PlayManager.TriggerGameOver();
            return false;
        }

        return true;
    }

    /// <summary>
    /// 현재 스테이지가 유효한지 검증하고, 유효하지 않으면 다음 챕터로 진행
    /// </summary>
    async UniTask<bool> TryValidateStage()
    {
        var chapterid = PlayManager.instance.GetCurrentChapterID();
        var stageid = PlayManager.instance.GetCurrentStageID();
        var stagedata = DataManager.instance.GetStageData(chapterid, stageid);

        //더 이상 진행할 스테이지가 없을 경우
        if (stagedata == null || stagedata._monsterlist == null || stagedata._monsterlist.Count <= 0)
        {
            //모든 스테이지를 다 생성한 후 몬스터가 전부 처치 되었을때 클리어 처리
            await UniTask.WaitUntil(() => _active_monsterlist.Count <= 0, cancellationToken: this.GetCancellationTokenOnDestroy());
            PlayManager.TriggerChapterNext();
            return false;
        }
        return true;
    }

    /// <summary>
    /// 현재 스테이지의 모든 몬스터를 생성
    /// </summary>
    async UniTask SpawnCurrentStageMonsters()
    {
        var chapterid = PlayManager.instance.GetCurrentChapterID();
        var stageid = PlayManager.instance.GetCurrentStageID();
        var stagedata = DataManager.instance.GetStageData(chapterid, stageid);

        //몬스터 생성
        var maxcount = stagedata._monsterlist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            var monstercountlist = stagedata._monsterlist[i]._count;
            for (int j = 0; j < monstercountlist; j++)
            {
                if (stagedata._monsterlist[i]._delaytime > 0)
                {
                    //딜레이 후 몬스터 생성
                    await UniTask.WaitForSeconds(stagedata._monsterlist[i]._delaytime, cancellationToken: this.GetCancellationTokenOnDestroy());
                }
                var monid = stagedata._monsterlist[i].GetMonsterInfo()._npc._mid;
                var mon = PoolSystem_Monster.instance.GetMonsterObject(monid);
                mon.transform.position = MonsterCreatePoint();
                if (Application.isEditor)
                {
                    mon.gameObject.name = $"몬스터_{counts}";
                    counts++;
                }
                _active_monsterlist.Add(mon);

                mon.IDSetting(stagedata._monsterlist[i]._monsterid);
                mon.OnSpawn(MonsterMovePoint());
            }
        }
    }

    /// <summary>
    /// 다음 스테이지까지 대기
    /// </summary>
    async UniTask WaitForNextStage()
    {
        await UniTask.WaitForSeconds(NEXTSTAGETIME, cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    Vector3 MonsterCreatePoint()
    {
        Debug.Log($"{this.gameObject.name}");
        var randomindex = UnityEngine.Random.Range(0, _createpoint.Length);
        var createpoint = _createpoint[randomindex].position;
        createpoint.x += UnityEngine.Random.Range(-1f, 1f);
        createpoint.y += UnityEngine.Random.Range(-1f, 1f);
        return createpoint;
    }

    Vector3 MonsterMovePoint()
    {
        var randomindex = UnityEngine.Random.Range(0, _protectpoint.Length);
        var movepoint = _protectpoint[randomindex].localPosition;
        movepoint.x -= 0.15f;
        movepoint.y -= 0.15f;
        if (movepoint.x <= MAXDOWNPOINT.x)
        {
            movepoint.x = MAXDOWNPOINT.x;
        }
        else if (movepoint.x >= MAXUPPOINT.x)
        {
            movepoint.x = MAXUPPOINT.x;
        }

        if (movepoint.y <= MAXDOWNPOINT.y)
        {
            movepoint.y = MAXDOWNPOINT.y;
        }
        else if (movepoint.y >= MAXUPPOINT.x)
        {
            movepoint.y = MAXUPPOINT.y;
        }

        return movepoint;
    }

    void RemoveMonsterList(Monster_Base diemon)
    {
        if (_active_monsterlist.Contains(diemon) == false)
        {
            return;
        }

        _active_monsterlist.Remove(diemon);
        Debug.Log($"남은 몬스터 수 :{_active_monsterlist.Count}");
    }
}