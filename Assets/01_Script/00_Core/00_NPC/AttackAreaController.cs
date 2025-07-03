using System;
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

    public event Action<BaseNPC, ESKILLTRIGGER> _enter_active_skill_event;
    BaseNPC _targetnpc;

    private void Start()
    {
        SetupLineRenderer();
        AttackAreaView();
        SetAttackAreaVisibility_Disable();
        UI_Status._status_disable_event += SetAttackAreaVisibility_Disable;
    }

    private void Update()
    {
        if (_targetnpc != null && _targetnpc.CheckDie() == false)
        {
            _enter_active_skill_event?.Invoke(_targetnpc, ESKILLTRIGGER.AREAENTER);
            return;
        }

        var ovelap = Physics2D.OverlapCircleAll(transform.position, _radius, _targetlayer);
        if (ovelap == null || ovelap.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < ovelap.Length; i++)
        {
            var targetnpc = ovelap[i].GetComponent<BaseNPC>();
            if (targetnpc == null)
            {
                continue;
            }

            if (targetnpc.CheckDie())
            {
                continue;
            }

            _targetnpc = targetnpc;
            _enter_active_skill_event?.Invoke(_targetnpc, ESKILLTRIGGER.AREAENTER);
            return;
        }
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

        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
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