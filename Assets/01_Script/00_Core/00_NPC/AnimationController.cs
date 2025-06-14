using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Animator _animator;
    EANIMATION _eanimation;

    public void PlayAnimation(EANIMATION eanimation)
    {
        _animator.SetTrigger(eanimation.ToString());
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
    IDLE,
    MOVE,
    DEATH,
    ATTACK,
    ATTACK1,
    ATTACK2,
    ABILITY,
    HIT
}