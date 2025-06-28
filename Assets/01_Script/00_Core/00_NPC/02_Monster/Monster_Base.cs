using System;
using UnityEngine;

public class Monster_Base : BaseNPC
{
    [SerializeField] MoveController _moveController;

    protected override void Start()
    {
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

    protected override void NPC_Die()
    {
        base.NPC_Die();
        _moveController.ReSetting();
        GoldManager._gold_add_event?.Invoke(_so_npc._diegold);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        MonsterSpawnManager._monster_die_animation_exit?.Invoke(this);
    }
}