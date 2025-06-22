using System;
using UnityEngine;

public class Monster_Base : BaseNPC
{
    [SerializeField] MoveController _moveController;

    protected override void Start()
    {
        PlayAnimation(EANIMATION.IDLE);
        _moveController._move_event += () => PlayAnimation(EANIMATION.MOVE, true);
        _moveController._move_end_check += () => PlayAnimation(EANIMATION.MOVE, false);
        _moveController._move_check += () => _animationController.CheckRunAnimation();
        base.Start();
    }

    public virtual void OnSpawn(Vector2 target)
    {
        _moveController.ReSetting();
        _moveController.MoveToTarget(target);
        base.OnSpawn();
    }

    public override void OnRelease()
    {
        base.OnRelease();
        MonsterSpawnManager._monster_die_animation_exit?.Invoke(this);
    }
}