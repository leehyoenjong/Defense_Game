
using System;
using System.Collections.Generic;

[Serializable]
public struct St_UserInventory
{
    public List<St_UserInvenItemList> _userinvendata;
    public static event Action _upgrade_event;

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