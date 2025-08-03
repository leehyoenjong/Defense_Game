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

    [Header("현재 이 오브젝트 대신 닿았을때 생성되며 데미지가 들어가는 이펙트")]
    [SerializeField] GameObject _boom_effect;

    // 스킬 정보
    BaseNPC _caster; // 스킬 사용자
    private Action<BaseNPC, BaseNPC> _action; // 실행할 스킬

    float _currentlifetime;

    void Update()
    {
        if (_lifeTime <= 0)
        {
            return;
        }
        _currentlifetime -= Time.deltaTime;
        // 지정된 시간 후 자동 제거
        if (_currentlifetime <= 0)
        {
            gameObject.SetActive(false);
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
        _currentlifetime = _lifeTime;
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

        if (_destroyOnHit)
        {
            this.gameObject.SetActive(false);
        }

        if (_boom_effect != null)
        {
            Instantiate(_boom_effect, other.transform.position, default).GetComponent<SkillEffectController>().Initialize(_caster, _action);
            return;
        }

        _action(_caster, targetNPC);
    }
}