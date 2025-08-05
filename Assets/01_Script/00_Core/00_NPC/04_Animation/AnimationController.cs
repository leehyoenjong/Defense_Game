using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Animator _animator;
    EANIMATION _eanimation;
    public Dictionary<EANIMATION, Action> _animation_exit = new Dictionary<EANIMATION, Action>();

    public void PlayAnimation(EANIMATION eanimation)
    {
        // 모든 애니메이션을 즉시 재생 (처음부터 시작)
        _animator.Play(eanimation.ToString(), 0, 0f);
        _eanimation = eanimation;
    }

    public void PlayAnimation(EANIMATION eanimation, bool isaction)
    {
        _animator.SetBool(eanimation.ToString(), isaction);
    }

    public bool CheckRunAnimation()
    {
        return _eanimation == EANIMATION.MOVE;
    }

    public void AddExitAnimationAction(EANIMATION eanimation, Action action)
    {
        if (!_animation_exit.ContainsKey(eanimation))
        {
            _animation_exit.Add(eanimation, null);
        }
        _animation_exit[eanimation] += action;
    }

    public void RemoveAnimaitionActive(EANIMATION eanimation, Action action)
    {
        if (!_animation_exit.ContainsKey(eanimation))
        {
            return;
        }
        _animation_exit[eanimation] += action;
    }

    public void ActiveExitAnimation(EANIMATION eanimation)
    {
        if (!_animation_exit.ContainsKey(eanimation))
        {
            return;
        }
        _animation_exit[eanimation]?.Invoke();
    }

    /// <summary>
    /// 애니메이션 이벤트
    /// </summary>
    public void SetAnimatorEvent(EANIMATION eanimation)
    {
        _eanimation = eanimation;
    }


}

public enum EANIMATION
{
    NONE,
    IDLE,
    MOVE,
    DEATH,
    ATTACK,
    ATTACK1,
    ATTACK2,
    ABILITY,
    HIT
}