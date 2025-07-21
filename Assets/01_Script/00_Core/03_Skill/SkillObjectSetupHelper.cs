using UnityEngine;

/// <summary>
/// 스킬 오브젝트 설정을 도와주는 헬퍼 스크립트
/// Inspector에서 쉽게 스킬 오브젝트를 설정할 수 있게 해줍니다
/// </summary>
public class SkillObjectSetupHelper : MonoBehaviour
{
    [Header("자동 설정 옵션")]
    [SerializeField] private bool _autoSetupOnAwake = true;
    [SerializeField] private bool _autoAddCollider = true;
    [SerializeField] private bool _autoAddRigidbody = false;

    [Header("충돌 감지 설정")]
    [SerializeField] private LayerMask _targetLayer = -1;
    [SerializeField] private bool _isTrigger = true;
    [SerializeField] private bool _destroyOnHit = true;
    [SerializeField] private float _lifeTime = 5f;

    [Header("움직임 설정")]
    [SerializeField] private EMOVEMENTTYPE _movementType = EMOVEMENTTYPE.STRAIGHT;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _homingStrength = 2f;
    [SerializeField] private float _detectionRadius = 10f;

    private void Awake()
    {
        if (_autoSetupOnAwake)
        {
            SetupSkillObject();
        }
    }

    /// <summary>
    /// 스킬 오브젝트를 자동으로 설정
    /// </summary>
    [ContextMenu("스킬 오브젝트 자동 설정")]
    public void SetupSkillObject()
    {
        // SkillEffectController 추가/설정
        var effectController = GetComponent<SkillEffectController>();
        if (effectController == null)
        {
            effectController = gameObject.AddComponent<SkillEffectController>();
        }

        // SkillMovementController 추가/설정
        if (_movementType != EMOVEMENTTYPE.NONE)
        {
            var movementController = GetComponent<SkillMovementController>();
            if (movementController == null)
            {
                movementController = gameObject.AddComponent<SkillMovementController>();
            }
            movementController._movementType = _movementType;
        }

        // Collider 추가
        if (_autoAddCollider && GetComponent<Collider>() == null && GetComponent<Collider2D>() == null)
        {
            // 2D 게임인지 3D 게임인지 판단해서 적절한 콜라이더 추가
            if (transform.position.z == 0f || Mathf.Approximately(transform.localScale.z, 0f))
            {
                var col2D = gameObject.AddComponent<CircleCollider2D>();
                col2D.isTrigger = _isTrigger;
                col2D.radius = 0.5f;
            }
            else
            {
                var col3D = gameObject.AddComponent<SphereCollider>();
                col3D.isTrigger = _isTrigger;
                col3D.radius = 0.5f;
            }
        }

        // Rigidbody 추가 (움직임이 있는 경우)
        if (_autoAddRigidbody && _movementType != EMOVEMENTTYPE.NONE)
        {
            if (GetComponent<Rigidbody>() == null && GetComponent<Rigidbody2D>() == null)
            {
                if (transform.position.z == 0f || Mathf.Approximately(transform.localScale.z, 0f))
                {
                    var rb2D = gameObject.AddComponent<Rigidbody2D>();
                    rb2D.gravityScale = 0f; // 중력 무시
                    rb2D.freezeRotation = true;
                }
                else
                {
                    var rb3D = gameObject.AddComponent<Rigidbody>();
                    rb3D.useGravity = false; // 중력 무시
                    rb3D.freezeRotation = true;
                }
            }
        }

        Debug.Log($"[{gameObject.name}] 스킬 오브젝트 설정 완료!");
    }

    /// <summary>
    /// 설정을 실제 컴포넌트에 적용
    /// </summary>
    private void ApplySettingsToComponents()
    {
        var effectController = GetComponent<SkillEffectController>();
        if (effectController != null)
        {
            // SkillEffectController의 설정을 위해서는 리플렉션이나 public 프로퍼티가 필요
            // 여기서는 기본적인 설정만 가능
        }

        var movementController = GetComponent<SkillMovementController>();
        if (movementController != null)
        {
            movementController._movementType = _movementType;
        }
    }

    /// <summary>
    /// 설정 검증
    /// </summary>
    [ContextMenu("설정 검증")]
    public void ValidateSetup()
    {
        bool isValid = true;
        string errorMessage = "";

        // SkillEffectController 확인
        if (GetComponent<SkillEffectController>() == null)
        {
            isValid = false;
            errorMessage += "- SkillEffectController가 없습니다.\n";
        }

        // Collider 확인
        if (GetComponent<Collider>() == null && GetComponent<Collider2D>() == null)
        {
            isValid = false;
            errorMessage += "- Collider가 없습니다.\n";
        }

        // Movement가 설정되어 있다면 MovementController 확인
        if (_movementType != EMOVEMENTTYPE.NONE && GetComponent<SkillMovementController>() == null)
        {
            isValid = false;
            errorMessage += "- 움직임이 설정되었지만 SkillMovementController가 없습니다.\n";
        }

        if (isValid)
        {
            Debug.Log($"[{gameObject.name}] 설정이 올바릅니다!");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 설정 문제:\n{errorMessage}");
        }
    }
} 