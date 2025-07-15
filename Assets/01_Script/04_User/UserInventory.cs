
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

        var userdatajson = loadresult.FlattenRows();

        // 인벤토리 아이템 데이터 로드
        if (userdatajson.ContainsKey("_userinvendata"))
        {
            var userinvendata = userdatajson["_userinvendata"];
            var maxcount = userinvendata.Count;
            for (int i = 0; i < maxcount; i++)
            {
                try
                {
                    var itemData = JsonConvert.DeserializeObject<St_UserInvenItemList>(userinvendata[i].ToString());
                    _userinvendata.Add(itemData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"인벤토리 아이템 데이터 로드 실패: {ex.Message}");
                    continue;
                }
            }
        }

        return true;
    }

    public void UpdateItemData(int itemid, int itemvalue)
    {
        var getitemresult = GetUserItemData(itemid);
        getitemresult.itemdata._itemvalue += itemvalue;
        _userinvendata[getitemresult.itemidx] = getitemresult.itemdata;
    }

    public void UpdateUpgrade(int itemid, int upgrade)
    {
        var getitemresult = GetUserItemData(itemid);
        getitemresult.itemdata._grade = upgrade;
        _userinvendata[getitemresult.itemidx] = getitemresult.itemdata;
        _upgrade_event?.Invoke();
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