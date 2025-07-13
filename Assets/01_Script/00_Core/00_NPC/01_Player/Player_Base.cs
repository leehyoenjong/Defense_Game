using UnityEngine;

public class Player_Base : BaseNPC
{
    [SerializeField] AttackAreaController _attackareacontroller;

    public virtual void OnSpawn(Vector2 spawnpoint)
    {
        transform.position = spawnpoint;
        StatusUpgradeManager._statusupgrade_event += AddStatus;
        UI_Hero_Btn._hero_click_event += AttackAreaView;

        var heroitemid = DataManager.instance.GetItemTable().FindConnectTableData(EITEMKIND.HERO, _myid);
        _status = _so_npc.GetStatus(heroitemid._itemid);
        base.OnSpawn();
    }

    void AddStatus(int id)
    {
        if (id != _myid)
        {
            return;
        }

        // 1. 이전 업그레이드 값 제거
        var beforeUpgradeStatus = StatusUpgradeManager.instance.GetStatusBeforeUpgradeAsStatus(_myid);
        base.RemoveStatus(beforeUpgradeStatus);

        // 2. 새로운 업그레이드 값 적용
        var newUpgradeStatus = StatusUpgradeManager.instance.GetStatusUpgradeAsStatus(_myid);
        base.AddStatus(newUpgradeStatus);
    }

    void AttackAreaView(int heroid)
    {
        if (heroid != _myid)
        {
            return;
        }

        _attackareacontroller.SetAttackAreaVisibility_Active();
    }
}