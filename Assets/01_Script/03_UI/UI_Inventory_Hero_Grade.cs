using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Hero_Grade : MonoBehaviour
{
    [SerializeField] Image[] _upgrade;
    [SerializeField] Sprite[] _upgradestar;
    int _heroitemid;

    void OnEnable()
    {
        UI_Inventory_Hero._click_slot += UpgradeStar;
    }

    void OnDisable()
    {
        UI_Inventory_Hero._click_slot -= UpgradeStar;
    }

    void UpgradeStar(int heroitemid)
    {
        _heroitemid = heroitemid;
        if (_heroitemid <= 0)
        {
            return;
        }

        var useritemdata = UserData._userdata._userinventory.GetUserItemData(_heroitemid).itemdata;
        for (int i = 0; i < 5; i++)
        {
            //1부터 업그레이드가 되었다는 것이기 때문에 i+1을 해준다.
            if (useritemdata._grade < i + 1)
            {
                _upgrade[i].sprite = _upgradestar[0];
                continue;
            }
            _upgrade[i].sprite = _upgradestar[1];
        }
    }

    public void Btn_Upgrade()
    {
        if (_heroitemid <= 0)
        {
            return;
        }

        var useritemdata = UserData._userdata._userinventory.GetUserItemData(_heroitemid).itemdata;
        var upgradtable = DataManager.instance.GetUpgradeData(useritemdata._grade);
        if (upgradtable._priceitemid == 0)
        {
            //TODO: 최대치 도달이라 시스템 메시지 남기기
            return;
        }

        var userpricevalue = UserData._userdata._userinventory.GetUserItemData(upgradtable._priceitemid).itemdata._itemvalue;
        if (userpricevalue < upgradtable._price)
        {
            //TODO: 돈 없다는 시스템 메시지 남기기
            return;
        }

        UserData._userdata._userinventory.UpdateItemData(upgradtable._priceitemid, -upgradtable._price);
        UserData._userdata._userinventory.UpdateUpgrade(_heroitemid, upgradtable._grade);
        UpgradeStar(_heroitemid);
    }
}