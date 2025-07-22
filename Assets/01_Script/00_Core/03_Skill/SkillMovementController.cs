using UnityEngine;

/// <summary>
/// 스킬 오브젝트의 움직임을 제어하는 컴포넌트
/// 직진, 유도, 포물선 등 다양한 움직임 패턴 지원
/// </summary>
public class SkillMovementController : MonoBehaviour
{
    [Header("움직임 설정")]
    public EMOVEMENTTYPE _movementType = EMOVEMENTTYPE.STRAIGHT;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _lifeTime = 5f;

    [Header("유도 설정")]
    [ConditionalField("_movementType", (int)EMOVEMENTTYPE.HOMING)]
    [SerializeField] private float _homingStrength = 2f;
    [ConditionalField("_movementType", (int)EMOVEMENTTYPE.HOMING)]
    [SerializeField] private float _detectionRadius = 10f;
    [ConditionalField("_movementType", (int)EMOVEMENTTYPE.HOMING)]
    [SerializeField] private LayerMask _targetLayer;

    [Header("포물선 설정")]
    [ConditionalField("_movementType", (int)EMOVEMENTTYPE.PARABOLA)]
    [SerializeField] private float _arcHeight = 5f;
    [ConditionalField("_movementType", (int)EMOVEMENTTYPE.PARABOLA)]
    [SerializeField] private Vector3 _targetPosition;

    [Header("회전 설정")]
    [ConditionalField("_movementType", (int)EMOVEMENTTYPE.ROTATE)]
    [SerializeField] private float _rotationSpeed = 90f;
    [ConditionalField("_movementType", (int)EMOVEMENTTYPE.ROTATE)]
    [SerializeField] private float _rotationRadius = 3f;
    [ConditionalField("_movementType", (int)EMOVEMENTTYPE.ROTATE)]
    [SerializeField] private Transform _rotationCenter;

    // 프라이빗 변수들
    private Vector3 _direction;
    private Transform _homingTarget;
    private Vector3 _startPosition;
    private float _elapsedTime = 0f;
    private Rigidbody2D _rigidbody2D;

    private void Start()
    {
        _startPosition = transform.position;
        _rigidbody2D = GetComponent<Rigidbody2D>();

        InitializeMovement();

        // 생존 시간 후 제거
        if (_lifeTime > 0)
        {
            Destroy(gameObject, _lifeTime);
        }
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        switch (_movementType)
        {
            case EMOVEMENTTYPE.STRAIGHT:
                MoveStraight();
                break;
            case EMOVEMENTTYPE.HOMING:
                MoveHoming();
                break;
            case EMOVEMENTTYPE.PARABOLA:
                MoveParabola();
                break;
            case EMOVEMENTTYPE.ROTATE:
                MoveRotate();
                break;
        }
    }

    /// <summary>
    /// 움직임 초기화
    /// </summary>
    private void InitializeMovement()
    {
        switch (_movementType)
        {
            case EMOVEMENTTYPE.STRAIGHT:
                _direction = transform.forward;
                break;
            case EMOVEMENTTYPE.HOMING:
                _direction = transform.forward;
                FindHomingTarget();
                break;
            case EMOVEMENTTYPE.PARABOLA:
                if (_targetPosition == Vector3.zero)
                    _targetPosition = transform.position + transform.forward * 10f;
                break;
            case EMOVEMENTTYPE.ROTATE:
                if (_rotationCenter == null)
                    _rotationCenter = transform.parent;
                break;
        }
    }

    /// <summary>
    /// 직진 움직임
    /// </summary>
    private void MoveStraight()
    {
        Vector3 movement = _direction * _speed * Time.deltaTime;

        if (_rigidbody2D != null)
            _rigidbody2D.MovePosition(transform.position + movement);
        else
            transform.position += movement;
    }

    /// <summary>
    /// 유도 움직임
    /// </summary>
    private void MoveHoming()
    {
        // 타겟이 없거나 죽었으면 새로 찾기
        if (_homingTarget == null || (_homingTarget.GetComponent<BaseNPC>()?.CheckDie() ?? false))
        {
            FindHomingTarget();
        }

        if (_homingTarget != null)
        {
            Vector3 targetDirection = (_homingTarget.position - transform.position).normalized;
            _direction = Vector3.Slerp(_direction, targetDirection, _homingStrength * Time.deltaTime).normalized;

            // 회전
            transform.LookAt(transform.position + _direction);
        }

        // 이동
        Vector3 movement = _direction * _speed * Time.deltaTime;

        if (_rigidbody2D != null)
            _rigidbody2D.MovePosition(transform.position + movement);
        else
            transform.position += movement;
    }

    /// <summary>
    /// 포물선 움직임
    /// </summary>
    private void MoveParabola()
    {
        float totalDistance = Vector3.Distance(_startPosition, _targetPosition);
        float totalTime = totalDistance / _speed;
        float progress = _elapsedTime / totalTime;

        if (progress >= 1f)
        {
            transform.position = _targetPosition;
            return;
        }

        // 수평 이동
        Vector3 horizontalPos = Vector3.Lerp(_startPosition, _targetPosition, progress);

        // 수직 이동 (포물선)
        float arc = _arcHeight * Mathf.Sin(progress * Mathf.PI);

        transform.position = horizontalPos + Vector3.up * arc;
    }

    /// <summary>
    /// 회전 움직임
    /// </summary>
    private void MoveRotate()
    {
        if (_rotationCenter == null)
            return;

        // 중심점을 기준으로 회전
        transform.RotateAround(_rotationCenter.position, Vector3.up, _rotationSpeed * Time.deltaTime);

        // 중심점으로부터 일정 거리 유지
        Vector3 directionToCenter = (_rotationCenter.position - transform.position).normalized;
        float currentDistance = Vector3.Distance(transform.position, _rotationCenter.position);

        if (Mathf.Abs(currentDistance - _rotationRadius) > 0.1f)
        {
            Vector3 targetPosition = _rotationCenter.position - directionToCenter * _rotationRadius;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// 유도 타겟 찾기
    /// </summary>
    private void FindHomingTarget()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, _detectionRadius, _targetLayer);
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;

        foreach (var target in targets)
        {
            var npc = target.GetComponent<BaseNPC>();
            if (npc == null || npc.CheckDie())
                continue;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target.transform;
            }
        }

        _homingTarget = closestTarget;
    }

    /// <summary>
    /// 타겟 위치 설정 (외부에서 호출)
    /// </summary>
    public void SetTargetPosition(Vector3 targetPos)
    {
        _targetPosition = targetPos;
    }

    /// <summary>
    /// 회전 중심점 설정 (외부에서 호출)
    /// </summary>
    public void SetRotationCenter(Transform center)
    {
        _rotationCenter = center;
    }

    /// <summary>
    /// 디버그용 기즈모
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        switch (_movementType)
        {
            case EMOVEMENTTYPE.HOMING:
                Gizmos.DrawWireSphere(transform.position, _detectionRadius);
                if (_homingTarget != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, _homingTarget.position);
                }
                break;

            case EMOVEMENTTYPE.PARABOLA:
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_targetPosition, 0.5f);
                Gizmos.DrawLine(transform.position, _targetPosition);
                break;

            case EMOVEMENTTYPE.ROTATE:
                if (_rotationCenter != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(_rotationCenter.position, _rotationRadius);
                }
                break;
        }
    }
}

public enum EMOVEMENTTYPE
{
    NONE,         // 움직임 없음
    STRAIGHT,     // 직진
    HOMING,       // 유도탄
    PARABOLA,     // 포물선
    ROTATE        // 회전
}