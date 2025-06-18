using System;
using System.Linq;
using UnityEngine;

public class AttackAreaController : MonoBehaviour
{
    [SerializeField] CircleCollider2D _attackarea;
    [SerializeField] LayerMask _targetlayer;
    public event Action<BaseNPC, ESKILLTRIGGER> _enter_active_skill_event;
    BaseNPC _targetnpc;

    private void Update()
    {
        if (_targetnpc != null && _targetnpc.CheckDie() == false)
        {
            _enter_active_skill_event?.Invoke(_targetnpc, ESKILLTRIGGER.AreaEnter);
            return;
        }

        var ovelap = Physics2D.OverlapCircleAll(transform.position, _attackarea.radius, _targetlayer);
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
            _enter_active_skill_event?.Invoke(_targetnpc, ESKILLTRIGGER.AreaEnter);
            return;
        }
    }
}