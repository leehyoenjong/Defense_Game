using System;
using UnityEngine;

public class ProtectObject : BaseNPC
{
    public static event Action _protectobject_death;
    [SerializeField] HpBarController_Text _hpbarController_text;

    protected override void Start()
    {
        base.Start();
        _hpbarController_text.Hpbar_Update(_status._hp, _current_hp);
        _hit_event += () => _hpbarController_text.Hpbar_Update(_status._hp, _current_hp);
    }

    protected override void PlayAnimation(EANIMATION eanimation)
    {

    }

    protected override void PlayAnimation(EANIMATION eanimation, bool isaction)
    {

    }
    protected override void NPC_Die()
    {
        base.NPC_Die();
        _protectobject_death?.Invoke();
    }
}
