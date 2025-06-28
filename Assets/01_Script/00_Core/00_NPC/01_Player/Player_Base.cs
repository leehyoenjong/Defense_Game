using UnityEngine;

public class Player_Base : BaseNPC
{
    protected int _id;
    public int GetID() => _id;
    [SerializeField] AttackAreaController _attackareacontroller;

    public virtual void OnSpawn(Vector2 spawnpoint)
    {
        transform.position = spawnpoint;
        StatusUpgradeManager._statusupgrade_event += AddStatus;
        UI_Hero_Btn._hero_click_event += AttackAreaView;
        base.OnSpawn();
    }

    public virtual void IDSetting(int id)
    {
        _id = id;
    }

    void AddStatus(int id)
    {
        if (id != _id)
        {
            return;
        }

        // 업그레이드 차이값을 가져와서 적용
        var upgradeDifference = StatusUpgradeManager.instance.GetStatusUpgradeDifference(_id);
        base.AddStatus(upgradeDifference);
    }

    void AttackAreaView(int heroid)
    {
        if (heroid != _id)
        {
            return;
        }

        _attackareacontroller.SetAttackAreaVisibility_Active();
    }
}