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
    [SerializeField] private float _destorytime = 0.5f;

    // 프라이빗 변수들
    private Vector3 _direction;
    private BaseNPC _homingTarget;
    private Vector3 _lastTargetPosition; // 타겟의 마지막 위치 저장
    private bool _isTargetLost = false;  // 타겟을 잃었는지 여부
    private bool _isarraive;

    float _currentlifetime;

    void OnEnable()
    {
        InitializeMovement();
    }

    private void Update()
    {
        if (_isarraive)
        {
            _currentlifetime -= Time.deltaTime;
            gameObject.SetActive(_currentlifetime > 0);
            return;
        }

        switch (_movementType)
        {
            case EMOVEMENTTYPE.STRAIGHT:
                MoveStraight();
                break;
            case EMOVEMENTTYPE.HOMING:
                MoveHoming();
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
            case EMOVEMENTTYPE.NOW:
                break;
        }
        _currentlifetime = _lifeTime;
        _isarraive = false;
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
            _currentlifetime = _destorytime;
            return;
        }
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
            _currentlifetime = _destorytime;
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