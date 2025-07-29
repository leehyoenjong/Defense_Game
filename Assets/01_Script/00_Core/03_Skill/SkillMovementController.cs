using System.Collections.Generic;
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

    [Header("도달 후 삭제 시간")]
    [SerializeField] private float _destorytime = 1f;

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
    private BaseNPC _homingTarget;
    private Vector3 _lastTargetPosition; // 타겟의 마지막 위치 저장
    private bool _isTargetLost = false;  // 타겟을 잃었는지 여부
    private bool _isarraive;

    private Vector3 _startPosition;
    private float _elapsedTime = 0f;




    private void Start()
    {
        _startPosition = transform.position;
        InitializeMovement();

        // 생존 시간 후 제거
        if (_lifeTime > 0)
        {
            Destroy(gameObject, _lifeTime);
        }
    }

    private void Update()
    {
        if (_isarraive)
        {
            return;
        }

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
            case EMOVEMENTTYPE.NOW:
                NowMove();
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
                break;
            case EMOVEMENTTYPE.PARABOLA:
                if (_targetPosition == Vector3.zero)
                    _targetPosition = transform.position + transform.forward * 10f;
                break;
            case EMOVEMENTTYPE.ROTATE:
                if (_rotationCenter == null)
                    _rotationCenter = transform.parent;
                break;
            case EMOVEMENTTYPE.NOW:
                break;
        }
    }

    /// <summary>
    /// 타겟 지정
    /// </summary>
    void TargetSetting()
    {
        // 타겟이 존재하면 타겟 위치로 
        if (_homingTarget != null && _homingTarget.CheckDie())
        {
            _lastTargetPosition = _homingTarget.transform.position;
            return;
        }

        // 타겟이 아직 있다면 타겟의 마지막 위치로 
        if (!_isTargetLost && _homingTarget == null)
        {
            _lastTargetPosition = _homingTarget.transform.position;
            _isTargetLost = true;
            return;
        }

        // 타겟을 잃은 상태가 아니고 새로운 타겟을 찾을 수 없다면
        if (!_isTargetLost && _homingTarget == null)
        {
            _isTargetLost = true;
            _lastTargetPosition = transform.position + _direction * 10f; // 현재 방향으로 일정 거리
            return;
        }
    }

    /// <summary>
    /// 직진 움직임
    /// </summary>
    private void MoveStraight()
    {
        Vector3 movement = _direction * _speed * Time.deltaTime;
        transform.position += movement;
    }

    private void NowMove()
    {
        TargetSetting();
        var newposition = _lastTargetPosition;
        transform.position = new Vector3(newposition.x, newposition.y, transform.position.z);
        _isarraive = true;
        Destroy(gameObject, _destorytime);
    }

    /// <summary>
    /// 유도 움직임
    /// </summary>
    private void MoveHoming()
    {
        TargetSetting();

        // 타겟이 있는 경우
        Vector2 newPosition = Vector2.MoveTowards(this.transform.position, _lastTargetPosition, _speed * Time.deltaTime);
        // 이동
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);

        // 마지막 위치에 충분히 가까워지면 삭제
        if (Vector3.Distance(transform.position, _lastTargetPosition) < 0.01f)
        {
            _isarraive = true;
            Destroy(gameObject, _destorytime);
            return;
        }
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
    /// 유도 타겟 리스트 설정 (외부에서 호출)
    /// </summary>
    public void SetTargets(BaseNPC targets)
    {
        _homingTarget = targets;
        _isTargetLost = false;
        if (targets != null)
        {
            _lastTargetPosition = targets.transform.position;
        }
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
                if (_homingTarget != null && !_isTargetLost)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, _homingTarget.transform.position);
                }
                else if (_isTargetLost)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(_lastTargetPosition, 0.5f);
                    Gizmos.DrawLine(transform.position, _lastTargetPosition);
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
    ROTATE,        // 회전
    NOW,           // 타겟 위치에 생성
}