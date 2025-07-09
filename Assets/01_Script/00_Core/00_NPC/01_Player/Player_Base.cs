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

        var userheroiteminfo = DataManager.instance.GetItemTable().FindConnectTableData(EITEMKIND.HERO, _id);
        _status = _so_npc.GetStatus(userheroiteminfo._itemid);
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

        // 1. 이전 업그레이드 값 제거
        var beforeUpgradeStatus = StatusUpgradeManager.instance.GetStatusBeforeUpgradeAsStatus(_id);
        base.RemoveStatus(beforeUpgradeStatus);

        // 2. 새로운 업그레이드 값 적용
        var newUpgradeStatus = StatusUpgradeManager.instance.GetStatusUpgradeAsStatus(_id);
        base.AddStatus(newUpgradeStatus);
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