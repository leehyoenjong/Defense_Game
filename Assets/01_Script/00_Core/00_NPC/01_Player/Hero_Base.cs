using UnityEngine;

public class Hero_Base : BaseNPC
{
    [SerializeField] AttackAreaController _attackareacontroller;

    protected override void Start()
    {

    }

    public virtual void OnSpawn(Vector2 spawnpoint)
    {
        transform.position = spawnpoint;
        StatusUpgradeManager._statusupgrade_event += AddStatus;
        UI_Hero_Btn._hero_click_event += AttackAreaView;
        base.OnSpawn();
    }

    protected override void Setting_Status(int itemid = 0)
    {
        var heroiteminfo = DataManager.instance.GetItemTable().FindConnectTableData(EITEMKIND.HERO, _so_npc._mid);
        base.Setting_Status(heroiteminfo._itemid);
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