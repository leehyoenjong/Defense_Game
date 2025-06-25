using System;
using UnityEngine;

public class ProtectObject : BaseNPC
{
    [SerializeField] HpBarController_Text _hpbarController_text;

    protected override void Start()
    {
        base.Start();
        OnSpawn();
        _hpbarController_text.Hpbar_Update(_status._hp, _current_hp);
        _hit_event += () => _hpbarController_text.Hpbar_Update(_status._hp, _current_hp);
    }

    public override void PlayAnimation(EANIMATION eanimation)
    {

    }

    public override void PlayAnimation(EANIMATION eanimation, bool isaction)
    {

    }
    protected override void NPC_Die()
    {
        base.NPC_Die();
        PlayManager._play_gameover?.Invoke();
    }
}
