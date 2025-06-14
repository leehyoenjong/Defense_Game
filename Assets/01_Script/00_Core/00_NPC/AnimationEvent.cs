using UnityEngine;

public class AnimationEvent : StateMachineBehaviour
{
    [SerializeField] EANIMATION _eanimation;
    
    // AnimationController 캐싱용
    private AnimationController _controller;
    
    // 상태 진입 시 호출
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 첫 번째 호출 시에만 AnimationController 캐싱
        if (_controller == null)
        {
            _controller = animator.GetComponent<AnimationController>();
        }
        
        if (_controller == null)
            return;

        // Inspector에서 설정한 애니메이션 타입으로 SetAnimatorEvent 호출
        _controller.SetAnimatorEvent(_eanimation);
    }
}