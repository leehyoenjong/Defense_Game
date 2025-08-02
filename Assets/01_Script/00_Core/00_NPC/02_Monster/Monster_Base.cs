using System;
using UnityEngine;

public class Monster_Base : BaseNPC
{
    [SerializeField] MoveController _moveController;
    public static event Action<Monster_Base> _monsterdie;
    public static Action<Monster_Base> _monster_die_animation_exit;
    St_MonsterTable _monsterinfo;
    public St_MonsterTable GetMonsterInfo() => _monsterinfo;

    protected override void Start()
    {
        _moveController._move_event += () => PlayAnimation(EANIMATION.MOVE, true);
        _moveController._move_end_check += () => PlayAnimation(EANIMATION.MOVE, false);
        _moveController._move_check += () => _animationController.CheckRunAnimation();
        base.Start();
    }

    void OnDestroy()
    {
        _moveController._move_event -= () => PlayAnimation(EANIMATION.MOVE, true);
        _moveController._move_end_check -= () => PlayAnimation(EANIMATION.MOVE, false);
        _moveController._move_check -= () => _animationController.CheckRunAnimation();
    }

    public override void IDSetting(int id)
    {
        base.IDSetting(id);
        _monsterinfo = DataManager.instance.GetMonsterInfo(id);
    }

    public virtual void OnSpawn(Vector2 target)
    {
        _moveController.ReSetting();
        _moveController.MoveToTarget(target);
        _status = _so_npc.GetStatus();
        base.OnSpawn();
    }

    protected override void NPC_Die()
    {
        base.NPC_Die();
        _moveController.ReSetting();
        _monsterdie?.Invoke(this);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        _monster_die_animation_exit?.Invoke(this);
    }
}