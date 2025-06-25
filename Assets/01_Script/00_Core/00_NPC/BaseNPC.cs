using System;
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

    //기본 맴버변수 
    protected int _current_hp;
    protected bool _isdie;
    protected St_Status _status;


    //이벤트 변수들
    public event Action _die_event;
    public event Action _hit_event;

    //함수
    protected virtual void Start()
    {
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
    }

    public virtual void OnSpawn()
    {
        //생성 시 스테이터스를 기본 스테이터스로 복사하기
        _status = _so_npc._status;

        //생성 버프 발동 내부에 스테이터스 변화하는 게 존재할 수 있음
        _skillController?.ActiveBuffSkill(ESKILLTRIGGER.SPAWN);

        //생성 버프 발동 후 hp셋팅하기
        _current_hp = _status._hp;

        this.gameObject.SetActive(true);
    }

    public virtual void OnRelease()
    {
        //사망 애니메이션 끝나면 꺼버리기
        this.gameObject.SetActive(false);
    }

    public virtual void Target_To_Attack(BaseNPC target_npc)
    {
        var my_damage = TotalDamage();
        target_npc.Hp_Update(my_damage);
    }

    public void AddStatus(St_Status addstatus)
    {
        _status._armor += addstatus._armor;
        _status._damge += addstatus._damge;
        _status._critical += addstatus._critical;
        _status._critical_damage += addstatus._critical_damage;
        _status._hp += addstatus._hp;
    }

    public void RemoveStatus(St_Status removestatus)
    {
        _status._armor -= removestatus._armor;
        _status._damge -= removestatus._damge;
        _status._critical -= removestatus._critical;
        _status._critical_damage -= removestatus._critical_damage;
        _status._hp -= removestatus._hp;
    }

    int TotalDamage()
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
        _current_hp -= target_damage;
        _hit_event?.Invoke();

        if (_current_hp <= 0 && _isdie == false)
        {
            NPC_Die();
        }
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
}