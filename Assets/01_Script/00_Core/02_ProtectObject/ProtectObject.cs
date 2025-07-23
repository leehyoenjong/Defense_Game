using System;
using UnityEngine;

public class ProtectObject : BaseNPC
{
    [SerializeField] HpBarController_Text _hpbarController_text;

    void OnEnable()
    {
        BaseSkill._skill_target_dictionary_list.Add(ETARGETKIND.PROTECT, new System.Collections.Generic.List<BaseNPC>() { this });
    }

    void OnDisable()
    {
        BaseSkill._skill_target_dictionary_list.Remove(ETARGETKIND.PROTECT);
    }

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