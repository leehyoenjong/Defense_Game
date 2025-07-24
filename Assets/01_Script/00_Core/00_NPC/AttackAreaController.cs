using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class AttackAreaController : MonoBehaviour
{
    [SerializeField] LayerMask _targetlayer;
    [SerializeField] float _radius;
    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] bool _showAttackArea = false;
    [SerializeField] Color _attackAreaColor = Color.red;
    [SerializeField] int _segments = 36;
    [SerializeField] float _targetSearchInterval = 0.1f; // 타겟 검색 주기 (초)

    public event Action<BaseNPC, ESKILLTRIGGER> _enter_active_skill_event;

    private BaseNPC _targetnpc;
    private static Material _sharedLineMaterial; // Material 캐싱
    private Collider2D[] _overlapResults = new Collider2D[20]; // NonAlloc용 배열
    private Coroutine _targetSearchCoroutine;
    private bool _hasTriggeredEvent = false; // 이벤트 중복 호출 방지
    private ContactFilter2D _contactFilter; // ContactFilter2D 캐싱

    private void Start()
    {
        SetupLineRenderer();
        AttackAreaView();
        SetAttackAreaVisibility_Disable();
        UI_Status._status_disable_event += SetAttackAreaVisibility_Disable;

        // ContactFilter2D 초기화 및 캐싱
        _contactFilter = new ContactFilter2D
        {
            layerMask = _targetlayer,
            useLayerMask = true,
            useTriggers = false // 트리거는 검색하지 않음
        };

        // 코루틴으로 타겟 검색 시작
        _targetSearchCoroutine = StartCoroutine(SearchTargetRoutine());
    }

    private void Update()
    {
        // 현재 타겟 상태 확인
        if (_targetnpc != null && _targetnpc.CheckDie() == false)
        {
            // 이벤트가 아직 발생하지 않았을 때만 호출
            if (!_hasTriggeredEvent)
            {
                _enter_active_skill_event?.Invoke(_targetnpc, ESKILLTRIGGER.AREAENTER);
                _hasTriggeredEvent = true;
            }
            return;
        }

        // 타겟이 없거나 사망했을 경우 이벤트 플래그 리셋
        if (_hasTriggeredEvent)
        {
            _hasTriggeredEvent = false;
        }

        _targetnpc = null;
    }

    private IEnumerator SearchTargetRoutine()
    {
        var wait = new WaitForSeconds(_targetSearchInterval);
        while (true)
        {
            // 현재 타겟이 유효하지 않을 때만 새로운 타겟 검색
            if (_targetnpc == null || _targetnpc.CheckDie())
            {
                SearchForNewTarget();
            }

            yield return wait;
        }
    }

    private void SearchForNewTarget()
    {
        // 새로운 Physics2D.OverlapCircle 방식 사용 (NonAlloc)
        int hitCount = Physics2D.OverlapCircle(transform.position, _radius, _contactFilter, _overlapResults
        );

        if (hitCount <= 0)
        {
            return;
        }

        // 가장 가까운 살아있는 타겟 찾기
        BaseNPC closestTarget = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            if (_overlapResults[i] == null) continue;

            var targetnpc = _overlapResults[i].GetComponent<BaseNPC>();
            if (targetnpc == null || targetnpc.CheckDie())
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, targetnpc.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = targetnpc;
            }
        }

        _targetnpc = closestTarget;
    }

    void SetupLineRenderer()
    {
        if (_lineRenderer == null)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }

        // Material 캐싱으로 메모리 할당 최소화
        if (_sharedLineMaterial == null)
        {
            _sharedLineMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        _lineRenderer.material = _sharedLineMaterial;
        _lineRenderer.startColor = _attackAreaColor;
        _lineRenderer.endColor = _attackAreaColor;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.positionCount = _segments + 1;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.sortingOrder = 10;
    }

    void AttackAreaView()
    {
        //게임 뷰에서 공격범위가 보이도록 추가 
        if (_lineRenderer == null)
            return;

        if (!_showAttackArea)
        {
            _lineRenderer.enabled = false;
            return;
        }

        _lineRenderer.enabled = true;

        // 원형 공격 범위를 LineRenderer로 그리기
        float angleStep = 360f / _segments;

        for (int i = 0; i <= _segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 point = new Vector3(
                Mathf.Cos(angle) * _radius,
                Mathf.Sin(angle) * _radius,
                0f
            );
            _lineRenderer.SetPosition(i, point);
        }
    }

    void SetAttackAreaVisibility_Disable()
    {
        _showAttackArea = false;
        AttackAreaView();
    }

    public void SetAttackAreaVisibility_Active()
    {
        _showAttackArea = true;
        AttackAreaView();
    }

    // 타겟 검색 주기 동적 조정 메서드
    public void SetTargetSearchInterval(float interval)
    {
        _targetSearchInterval = Mathf.Max(0.05f, interval); // 최소 0.05초
    }

    private void OnDestroy()
    {
        UI_Status._status_disable_event -= SetAttackAreaVisibility_Disable;

        if (_targetSearchCoroutine != null)
        {
            StopCoroutine(_targetSearchCoroutine);
        }
    }

    private void OnDrawGizmos()
    {
        // 공격 범위를 빨간색 원으로 그리기
        Gizmos.color = Color.red;

        // 2D 환경에서 원 그리기 (z축을 0으로 설정)
        Vector3 center = new Vector3(transform.position.x, transform.position.y, 0f);

        // 원을 여러 개의 선분으로 그리기
        int segments = 36;
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * _radius, Mathf.Sin(angle1) * _radius, 0f);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * _radius, Mathf.Sin(angle2) * _radius, 0f);

            Gizmos.DrawLine(point1, point2);
        }
    }
}