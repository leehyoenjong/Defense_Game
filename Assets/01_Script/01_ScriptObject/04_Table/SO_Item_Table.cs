using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Item_Table", menuName = "Table/SO_Item_Table", order = 0)]
public class SO_Item_Table : ScriptableObject
{
    public List<St_ItemTable> _itemlist = new List<St_ItemTable>();

    Dictionary<int, St_ItemTable> _itemlist_dic = new Dictionary<int, St_ItemTable>();
    public Dictionary<int, St_ItemTable> GetItemdata()
    {
        if (_itemlist_dic.Count <= 0)
        {
            var maxcount = _itemlist.Count;
            for (int i = 0; i < maxcount; i++)
            {
                //TODO: 에러 날 시 문제가 있는 것이기 때문에 검사하지 않고 넣을 것
                _itemlist_dic.Add(_itemlist[i]._itemid, _itemlist[i]);
            }
        }
        return _itemlist_dic;
    }

    public St_ItemTable SearchItemData(int itemid)
    {
        if (GetItemdata().TryGetValue(itemid, out var itemdata) == false)
        {
            return default;
        }
        return itemdata;
    }

    /// <summary>
    /// 아이템 테이블에 연결된 테이블의 정보 가져오기 
    /// </summary>
    /// <returns></returns>
    public T FindConnectTableData<T>(int itemid, Dictionary<int, T> findtable)
    {
        if (!GetItemdata().TryGetValue(itemid, out var itemdata))
        {
            return default(T);
        }

        findtable.TryGetValue(itemdata._connecttableid, out var finddata);
        return finddata;
    }
}

public enum EITEMKIND
{
    NONE,
    COIN,
    HERO,
    GACHA
}


[Serializable]
public struct St_ItemTable
{
    public int _itemid;
    public string _itemname;
    public string _itemexplain;
    public Sprite _itemicon;
    public EITEMKIND _itemkind;
    public int _connecttableid;
}