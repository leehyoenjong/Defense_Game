using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MonsterSpawnManager : MonoBehaviour
{
    [SerializeField] Transform[] _protectpoint;
    [SerializeField] Transform[] _createpoint;

    List<Monster_Base> _active_monsterlist = new List<Monster_Base>();
    const float NEXTSTAGETIME = 5f;//죽은 몬스터 수와 상관없이 60초마다 몬스터 생성

    Vector2 MAXDOWNPOINT = new Vector2(-4.6f, -1.2f);
    Vector2 MAXUPPOINT = new Vector2(1.8f, 2.5f);


    void OnEnable()
    {
        PlayManager._play_event += CreateMonster().Forget;
        Monster_Base._monster_die_animation_exit += (diemon) => RemoveMonsterList(diemon);
    }

    void OnDisable()
    {
        PlayManager._play_event -= CreateMonster().Forget;
        Monster_Base._monster_die_animation_exit -= (diemon) => RemoveMonsterList(diemon);
    }

    async UniTaskVoid CreateMonster()
    {
        while (true)
        {
            //현재 챕터 데이터 가져오기 
            var chapterid = PlayManager.instance.GetCurrentChapterID();
            var currentchapterdata = DataManager.instance.GetChapterData(chapterid);

            //더 이상 진행할 챕터 없을 경우 종료
            if (currentchapterdata._stagedata == null || currentchapterdata._stagedata.Count <= 0)
            {
                PlayManager._play_stage_allclear?.Invoke();
                return;
            }

            //스테이지 생성
            var stageid = PlayManager.instance.GetCurrentStageID();
            var stagedata = DataManager.instance.GetStageData(chapterid, stageid);

            //더 이상 진행할 스테이지가 없을 경우
            if (stagedata == null || stagedata._monsterlist == null || stagedata._monsterlist.Count <= 0)
            {
                //모든 스테이지를 다 생성한 후 몬스터가 전부 처치 되었을때 클리어 처리
                await UniTask.WaitUntil(() => _active_monsterlist.Count <= 0, cancellationToken: this.GetCancellationTokenOnDestroy());
                PlayManager._play_chapter_next?.Invoke();
                continue;
            }

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
                    var mon = Instantiate<GameObject>(stagedata._monsterlist[i].GetMonsterInfo()._npc._mybodyobject);
                    mon.transform.position = MonsterCreatePoint();
                    var monsterbase = mon.GetComponent<Monster_Base>();
                    _active_monsterlist.Add(monsterbase);

                    monsterbase.IDSetting(stagedata._monsterlist[i]._monsterid);
                    monsterbase.OnSpawn(MonsterMovePoint());
                }
            }

            //60초마다 몬스터 생성
            await UniTask.WaitForSeconds(NEXTSTAGETIME, cancellationToken: this.GetCancellationTokenOnDestroy());
            PlayManager._play_stage_next?.Invoke();
        }
    }

    void RemoveMonsterList(Monster_Base diemon)
    {
        if (_active_monsterlist.Contains(diemon) == false)
        {
            return;
        }

        _active_monsterlist.Remove(diemon);
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

    Vector3 MonsterCreatePoint()
    {
        var randomindex = UnityEngine.Random.Range(0, _createpoint.Length);
        var createpoint = _createpoint[randomindex].position;
        createpoint.x += UnityEngine.Random.Range(-1f, 1f);
        createpoint.y += UnityEngine.Random.Range(-1f, 1f);
        return createpoint;
    }
}
