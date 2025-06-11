using System;
using UnityEngine;

public class Monster_Base : BaseNPC
{
    [SerializeField] MoveController _moveController;

    protected override void Start()
    {
        PlayAnimation(EANIMATION.IDLE);
        _moveController._move_event += () => PlayAnimation(EANIMATION.MOVE);
    }

    public void OnRelease()
    {
        this.gameObject.SetActive(false);
        _moveController.ReSetting();
    }

    public void OnSpawn()
    {
        this.gameObject.SetActive(true);
    }
}