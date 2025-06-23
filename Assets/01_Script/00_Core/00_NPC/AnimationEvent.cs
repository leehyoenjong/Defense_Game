using System;
using UnityEngine;

public class AnimationEvent : StateMachineBehaviour
{
    // AnimationController 캐싱용
    private AnimationController _controller;
    AnimationController GetController(GameObject obj)
    {
        if (_controller == null)
        {
            _controller = obj.GetComponent<AnimationController>();
        }
        return _controller;
    }
    [SerializeField] EANIMATION _eanimation;

    // 상태 진입 시 호출
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 첫 번째 호출 시에만 AnimationController 캐싱
        if (GetController(animator.transform.parent.gameObject) == null)
        {
            return;
        }

        // Inspector에서 설정한 애니메이션 타입으로 SetAnimatorEvent 호출
        _controller.SetAnimatorEvent(_eanimation);
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        GetController(animator.transform.parent.gameObject)?.ActiveExitAnimation(_eanimation);
    }
}