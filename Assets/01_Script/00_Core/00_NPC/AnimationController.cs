using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Animator _animator;

    public void PlayAnimation(EANIMATION eanimation)
    {
        _animator.SetTrigger(eanimation.ToString());
    }
}

public enum EANIMATION
{
    IDLE,
    MOVE,
    DEATH,
    ATTACK,
    HIT
}