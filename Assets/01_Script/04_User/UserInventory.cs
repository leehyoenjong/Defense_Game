
using System;
using System.Collections.Generic;

[Serializable]
public struct St_UserInventory
{
    public List<St_UserInvenItemList> _userinvendata;

    public void UpdateItemData(int itemid, int itemvalue)
    {
        var getitemresult = GetUserItemData(itemid);
        getitemresult.itemdata._itemvalue += itemvalue;
        _userinvendata[getitemresult.itemidx] = getitemresult.itemdata;
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
}