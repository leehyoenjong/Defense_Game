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
    }

    public void OnRelease()
    {
        this.gameObject.SetActive(false);
        _moveController.ReSetting();
    }

    public void OnSpawn(Vector2 target)
    {
        this.gameObject.SetActive(true);
        _moveController.MoveToTarget(target);
    }
}