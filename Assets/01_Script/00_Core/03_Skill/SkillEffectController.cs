using System;
using UnityEngine;

/// <summary>
/// 스킬 이펙트 오브젝트에 붙여서 충돌 감지 및 스킬 효과를 처리하는 컴포넌트
/// </summary>
public class SkillEffectController : MonoBehaviour
{
    [Header("충돌 감지 설정")]
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private bool _destroyOnHit = true;
    [SerializeField] private float _lifeTime; // 생존 시간

    [Header("디버그")]
    [SerializeField] private bool _showDebugGizmos = true;

    // 스킬 정보
    BaseNPC _caster; // 스킬 사용자
    BaseNPC _target; // 타겟
    private Action<BaseNPC, BaseNPC> _action; // 실행할 스킬

    private void Start()
    {
        // 지정된 시간 후 자동 제거
        if (_lifeTime > 0)
        {
            Destroy(gameObject, _lifeTime);
        }
    }

    /// <summary>
    /// 스킬 효과 초기화 (스킬 생성 시 호출)
    /// </summary>
    /// <param name="caster">스킬 사용자</param>
    /// <param name="skill">실행할 스킬</param>
    public void Initialize(BaseNPC caster, Action<BaseNPC, BaseNPC> action)
    {
        _caster = caster;
        _action = action;
    }

    /// <summary>
    /// 충돌 감지 (2D)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        var targetNPC = other.gameObject.GetComponent<BaseNPC>();
        if (targetNPC == null || targetNPC.CheckDie() || (_targetLayer & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        _action(_caster, targetNPC);
        if (_destroyOnHit)
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 디버그용 기즈모
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!_showDebugGizmos)
            return;

        Gizmos.color = Color.yellow;

        // 2D 컬라이더가 있다면 그 영역을 표시
        var collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            Gizmos.DrawWireCube(transform.position, collider2D.bounds.size);
        }
    }
}