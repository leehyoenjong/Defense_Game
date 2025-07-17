
using System;
using System.Collections.Generic;
using BackEnd;
using BackEnd.BackndNewtonsoft.Json;
using UnityEngine;

[Serializable]
public struct St_UserInventory
{
    public List<St_UserInvenItemList> _userinvendata;
    public static event Action _upgrade_event;

    public Param Get_UserData()
    {
        var param = new Param();
        param.Add("_userinvendata", _userinvendata);
        return param;
    }

    public bool Load_UserData(BackendReturnObject loadresult)
    {
        if (loadresult.IsSuccess() == false)
        {
            return false;
        }

        var userdatajson = loadresult.FlattenRows()[0];
        Debug.Log($"{userdatajson.ToString()}");

        // 인벤토리 아이템 데이터 로드
        if (userdatajson.ContainsKey("_userinvendata"))
        {
            var userinvendata = userdatajson["_userinvendata"];
            var maxcount = userinvendata.Count;
            for (int i = 0; i < maxcount; i++)
            {
                try
                {
                    Debug.Log($"가져온 데이터 :{userinvendata[i].ToString()}");
                    var itemData = JsonConvert.DeserializeObject<St_UserInvenItemList>(userinvendata[i].ToString());
                    _userinvendata.Add(itemData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"인벤토리 아이템 데이터 로드 실패: {ex.Message}");
                    return false;
                }
            }
        }

        return true;
    }

    public void UpdateItemData(int itemid, int itemvalue)
    {
        var getitemresult = GetUserItemData(itemid);
        var beforevalue = getitemresult.itemdata._itemvalue;
        getitemresult.itemdata._itemvalue += itemvalue;
        _userinvendata[getitemresult.itemidx] = getitemresult.itemdata;
        BackEndLog.WriteLog(LogType.INVENTORY, $"획득한 아이템 아이디 :{itemid} / 획득 전 갯수:{beforevalue} / 획득 후 갯수:{getitemresult.itemdata._itemvalue}");
    }

    public void UpdateUpgrade(int itemid, int upgrade)
    {
        var getitemresult = GetUserItemData(itemid);
        var beforeupgrade = getitemresult.itemdata._grade;
        getitemresult.itemdata._grade = upgrade;
        _userinvendata[getitemresult.itemidx] = getitemresult.itemdata;
        _upgrade_event?.Invoke();
        BackEndLog.WriteLog(LogType.INVENTORY, $"업그레이드 아이디:{itemid} 업그레이드 전:{beforeupgrade} / 업그레이드 후:{getitemresult.itemdata._grade}");
    }

    public (St_UserInvenItemList itemdata, int itemidx) GetUserItemData(int itemid)
    {
        var itemidx = _userinvendata.FindIndex(x => x._itemid == itemid);
        if (itemidx == -1)
        {
            var itemdata = new St_UserInvenItemList();
            itemdata._itemid = itemid;
            _userinvendata.Add(itemdata);
            itemidx = _userinvendata.Count - 1;
        }
        return (_userinvendata[itemidx], itemidx);
    }
}

[Serializable]
public struct St_UserInvenItemList
{
    public int _itemid;
    public int _itemvalue;
    public int _grade;
}