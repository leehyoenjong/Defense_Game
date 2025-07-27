using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(AnimationController), typeof(SkillController), typeof(AttackAreaController))]
[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public abstract class BaseNPC : MonoBehaviour
{
    //NPC 별 데이터 
    public SO_NPC _so_npc;
    [SerializeField] protected AnimationController _animationController;
    [SerializeField] protected HpbarController _hpbarController;
    [SerializeField] protected SkillController _skillController;
    [SerializeField] protected DamageTextController _damagetextcontroller;

    //기본 맴버변수 
    protected int _myid;
    public int GetID() => _myid;
    [SerializeField] protected int _current_hp;
    [SerializeField] protected bool _isdie;
    protected St_Status _status;
    public St_Status GetStatus() => _status;

    //스킬
    protected List<SO_Skill_Attack> _active_attackskill = new List<SO_Skill_Attack>();
    public List<SO_Skill_Attack> GetActiveAttackSkill() => _active_attackskill;

    protected List<SO_Skill_Buff> _active_buffkskill = new List<SO_Skill_Buff>();
    public List<SO_Skill_Buff> GetActiveBuffSkill() => _active_buffkskill;


    //이벤트 변수들
    public event Action _die_event;
    public event Action _hit_event;

    //함수
    protected virtual void Start()
    {

    }


    public virtual void IDSetting(int id)
    {
        _myid = id;
    }

    public virtual void OnSpawn()
    {
        Setting_Status();

        //애니메이션 관련
        if (_animationController)
        {
            _die_event += () => _isdie = true;
            _die_event += () => PlayAnimation(EANIMATION.DEATH);
            _hit_event += () => PlayAnimation(EANIMATION.HIT);
            _animationController.AddExitAnimationAction(EANIMATION.DEATH, OnRelease);
        }

        //hp바 업데이트
        if (_hpbarController)
        {
            _hit_event += () => _hpbarController.Hpbar_Update(_status._hp, _current_hp);
        }

        //스킬 삽입
        AddActiveSkill(_so_npc._basic_attack_skill);

        //생성 버프 발동 내부에 스테이터스 변화하는 게 존재할 수 있음
        _skillController?.ActiveBuffSkill(ESKILLTRIGGER.SPAWN);

        //생성 버프 발동 후 hp셋팅하기
        _current_hp = _status._hp;

        this.gameObject.SetActive(true);
    }

    protected virtual void Setting_Status(int itemid = 0)
    {
        //기본 능력치 적용
        if (itemid > 0)
        {
            _status = _so_npc.GetStatus(itemid);
            return;
        }
        _status = _so_npc.GetStatus();
    }

    public virtual void OnRelease()
    {
        //사망 애니메이션 끝나면 꺼버리기
        this.gameObject.SetActive(false);
    }

    public virtual void Target_To_Attack(BaseNPC target_npc, int totaldamage)
    {
        target_npc.Hp_Update(totaldamage);
    }

    public virtual void AddStatus(St_Status addstatus)
    {
        _status._armor += addstatus._armor;
        _status._damge += addstatus._damge;
        _status._critical += addstatus._critical;
        _status._critical_damage += addstatus._critical_damage;
        _status._hp += addstatus._hp;
    }

    public virtual void RemoveStatus(St_Status removestatus)
    {
        _status._armor -= removestatus._armor;
        _status._damge -= removestatus._damge;
        _status._critical -= removestatus._critical;
        _status._critical_damage -= removestatus._critical_damage;
        _status._hp -= removestatus._hp;
    }

    public int TotalDamage()
    {
        float damage = _status._damge;

        var critical_random_value = UnityEngine.Random.Range(0f, 1f);
        if (critical_random_value <= _status._critical)
        {
            damage = damage * (_status._critical_damage + 1);
        }

        return Mathf.FloorToInt(damage);
    }

    protected virtual void Hp_Update(int target_damage)
    {
        if (_isdie)
        {
            return;
        }

        _current_hp -= target_damage;
        _hit_event?.Invoke();
        _damagetextcontroller?.CreateText(-target_damage);

        Debug.Log($"{this.gameObject.name}의 상태 전 :{_current_hp}/{_isdie}/{_current_hp <= 0 && _isdie == false}");
        if (_current_hp <= 0 && _isdie == false)
        {
            NPC_Die();
        }
        Debug.Log($"{this.gameObject.name}의 상태 후 :{_current_hp}/{_isdie}/{_current_hp <= 0 && _isdie == false}");
    }

    protected virtual void NPC_Die()
    {
        _die_event?.Invoke();
    }

    public bool CheckDie()
    {
        return _current_hp <= 0;
    }

    public virtual void PlayAnimation(EANIMATION eanimation)
    {
        _animationController.PlayAnimation(eanimation);
    }

    public virtual void PlayAnimation(EANIMATION eanimation, bool isaction)
    {
        _animationController.PlayAnimation(eanimation, isaction);
    }

    public void AddActiveSkill(BaseSkill addskill)
    {
        if (addskill == null)
        {
            return;
        }

        if (addskill._skillkind == ESKILLKIND.ATTACK)
        {
            var idx = _active_attackskill.FindIndex(x => x._skillInfo._mid == addskill._skillInfo._mid);
            if (idx != -1)
            {
                _active_attackskill.RemoveAt(idx);
            }
            _active_attackskill.Add(addskill as SO_Skill_Attack);
        }
        else
        {
            var idx = _active_buffkskill.FindIndex(x => x._skillInfo._mid == addskill._skillInfo._mid);
            if (idx != -1)
            {
                _active_buffkskill.RemoveAt(idx);
            }
            _active_buffkskill.Add(addskill as SO_Skill_Buff);
        }
    }

    public void AddActiveSkill(BaseSkill[] addskill)
    {
        var maxcount = addskill.Length;
        for (int i = 0; i < maxcount; i++)
        {
            AddActiveSkill(addskill[i]);
        }
    }

    /// <summary>
    /// 현재 체력 반환
    /// </summary>
    public int GetCurrentHP()
    {
        return _current_hp;
    }

    /// <summary>
    /// 최대 체력 반환
    /// </summary>
    public int GetMaxHP()
    {
        return _status._hp;
    }
}