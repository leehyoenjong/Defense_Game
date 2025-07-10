using UnityEngine;

public class Player_Base : BaseNPC
{
    protected int _heroid;
    public int GetID() => _heroid;
    [SerializeField] AttackAreaController _attackareacontroller;

    public virtual void OnSpawn(Vector2 spawnpoint)
    {
        transform.position = spawnpoint;
        StatusUpgradeManager._statusupgrade_event += AddStatus;
        UI_Hero_Btn._hero_click_event += AttackAreaView;

        var heroitemid = DataManager.instance.GetItemTable().FindConnectTableData(EITEMKIND.HERO, _heroid);
        _status = _so_npc.GetStatus(heroitemid._itemid);
        base.OnSpawn();
    }

    public virtual void IDSetting(int id)
    {
        _heroid = id;
    }

    void AddStatus(int id)
    {
        if (id != _heroid)
        {
            return;
        }

        // 1. 이전 업그레이드 값 제거
        var beforeUpgradeStatus = StatusUpgradeManager.instance.GetStatusBeforeUpgradeAsStatus(_heroid);
        base.RemoveStatus(beforeUpgradeStatus);

        // 2. 새로운 업그레이드 값 적용
        var newUpgradeStatus = StatusUpgradeManager.instance.GetStatusUpgradeAsStatus(_heroid);
        base.AddStatus(newUpgradeStatus);
    }

    void AttackAreaView(int heroid)
    {
        if (heroid != _heroid)
        {
            return;
        }

        _attackareacontroller.SetAttackAreaVisibility_Active();
    }
}