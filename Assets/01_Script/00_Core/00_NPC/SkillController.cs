using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    [SerializeField] BaseNPC _me;
    [SerializeField] AttackAreaController _attackAreaController;

    //일반 변수
    float _attackskillnextdelaytime;
    float _buffskillnextdelaytime;
    int _current_attack_skill_index = 0;
    int _current_buff_skill_index = 0;

    //쿨타임 및 지속시간 관련 변수
    Dictionary<int, bool> _skillcoolTime = new Dictionary<int, bool>();
    Dictionary<int, CancellationTokenSource> _skillcooltimetoken = new Dictionary<int, CancellationTokenSource>();
    Dictionary<int, CancellationTokenSource> _skilldurationtoken = new Dictionary<int, CancellationTokenSource>();

    //개별 이벤트
    public event Action _skill_attack_event;
    public event Action _skill_buff_event;

    private void Start()
    {
        if (_attackAreaController)
        {
            _attackAreaController._enter_active_skill_event += ActiveAttackSkill;
        }
    }

    private void Update()
    {
        _attackskillnextdelaytime -= Time.deltaTime;
        _buffskillnextdelaytime -= Time.deltaTime;
    }

    void ActiveAttackSkill(BaseNPC target, ESKILLTRIGGER eskilltrigger)
    {
        //다음 스킬 발동 딜레이 중일 경우 return, 이때는 index를 올릴 필요 없음 
        if (_attackskillnextdelaytime > 0)
        {
            return;
        }

        //SO에 스킬이 없다면 그냥 return
        var myattackskilllist = _me._so_npc._skill_Attack;
        if (myattackskilllist.Length <= 0)
        {
            return;
        }

        //해당 인덱스의 스킬이 trigger와 맞지 않다면 return 후 index값 높이기
        var soskillinfo = myattackskilllist[_current_attack_skill_index];
        if (soskillinfo._eskilltrigger != eskilltrigger)
        {
            AddAttackSkillIndex();
            return;
        }

        var skillinfo = soskillinfo._skillInfo;
        //현재 스킬이 쿨타임 진행중인지 체크
        if (CheckCoolTime(skillinfo._mid))
        {
            AddAttackSkillIndex();
            return;
        }

        //스킬 발동 
        _me._so_npc._skill_Attack[_current_attack_skill_index].ActiveSkill(_me, target);
        SkillCoolTime(skillinfo).Forget();
        _skill_attack_event?.Invoke();
        AddAttackSkillIndex();
    }

    public void ActiveBuffSkill(ESKILLTRIGGER ebuffskillactivetirrger)
    {
        //다음 스킬 발동 딜레이 중일 경우 return, 이때는 index를 올릴 필요 없음 
        if (_buffskillnextdelaytime > 0)
        {
            return;
        }

        var mybufflist = _me._so_npc._skill_buff;
        if (mybufflist.Length <= 0)
        {
            return;
        }

        var skillinfo = mybufflist[_current_attack_skill_index]._skillInfo;

        //다음 스킬 발동 쿨타임인지 체크
        if (CheckCoolTime(skillinfo._mid))
        {
            //쿨타임 중인 스킬이라면 다음 index스킬 체크
            AddBuffSkillIndex();
            return;
        }

        //스킬 발동
        _me._so_npc._skill_buff[_current_attack_skill_index].ActiveSkill(_me, ebuffskillactivetirrger);

        //스킬 발동 후 지속시간 후 종료되도록 처리
        BuffSkillDisable(skillinfo, _current_attack_skill_index).Forget();
        SkillCoolTime(skillinfo).Forget();
        _skill_buff_event?.Invoke();
        AddBuffSkillIndex();
    }

    void AddAttackSkillIndex()
    {
        _current_attack_skill_index++;
        if (_me._so_npc._skill_Attack.Length >= _current_attack_skill_index)
        {
            _current_attack_skill_index = 0;
        }
    }

    void AddBuffSkillIndex()
    {
        _current_buff_skill_index++;
        if (_me._so_npc._skill_buff.Length >= _current_buff_skill_index)
        {
            _current_buff_skill_index = 0;
        }
    }

    async UniTaskVoid BuffSkillDisable(St_SkillInfo skillinfo, int idx)
    {
        if (skillinfo._duration <= 0)
        {
            _me._so_npc._skill_buff[idx].DisableSkill(_me);
            return;
        }

        //강제 종료용 토큰 생성
        if (!_skilldurationtoken.ContainsKey(skillinfo._mid))
        {
            _skilldurationtoken.Add(skillinfo._mid, null);
        }

        if (_skilldurationtoken[skillinfo._mid] != null)
        {
            _skilldurationtoken[skillinfo._mid].Cancel();
            _skilldurationtoken[skillinfo._mid].Dispose();
        }
        _skilldurationtoken[skillinfo._mid] = new CancellationTokenSource();

        await UniTask.WaitForSeconds(skillinfo._duration, cancellationToken: _skilldurationtoken[skillinfo._mid].Token);
        _me._so_npc._skill_buff[idx].DisableSkill(_me);
    }

    async UniTaskVoid SkillCoolTime(St_SkillInfo skillinfo)
    {
        if (skillinfo._cooltime <= 0)
        {
            return;
        }

        if (!_skillcooltimetoken.ContainsKey(skillinfo._mid))
        {
            _skillcooltimetoken.Add(skillinfo._mid, null);
        }

        if (_skillcooltimetoken[skillinfo._mid] != null)
        {
            _skillcooltimetoken[skillinfo._mid].Cancel();
            _skillcooltimetoken[skillinfo._mid].Dispose();
        }
        _skillcooltimetoken[skillinfo._mid] = new CancellationTokenSource();

        _skillcoolTime[skillinfo._mid] = true;
        await UniTask.WaitForSeconds(skillinfo._cooltime, cancellationToken: _skillcooltimetoken[skillinfo._mid].Token);
        _skillcoolTime[skillinfo._mid] = false;
    }

    bool CheckCoolTime(int skillid)
    {
        if (_skillcoolTime.ContainsKey(skillid))
        {
            return _skillcoolTime[skillid];
        }

        _skillcoolTime.Add(skillid, false);
        return false;
    }


    void OnDisable()
    {
        foreach (var item in _skillcooltimetoken)
        {
            if (item.Value == null)
            {
                continue;
            }

            item.Value.Cancel();
            item.Value.Dispose();
        }

        foreach (var item in _skilldurationtoken)
        {
            if (item.Value == null)
            {
                continue;
            }

            item.Value.Cancel();
            item.Value.Dispose();
        }
    }
}