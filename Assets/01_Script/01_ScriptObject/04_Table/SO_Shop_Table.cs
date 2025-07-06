using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Shop_Table", menuName = "Table/SO_Shop_Table", order = 0)]
public class SO_Shop_Table : ScriptableObject
{
    public List<St_ShopTable> _shoplist = new List<St_ShopTable>();
    Dictionary<int, St_ShopTable> _shoplist_dic = new Dictionary<int, St_ShopTable>();
    public Dictionary<int, St_ShopTable> GetShopData()
    {
        if (_shoplist_dic.Count <= 0)
        {
            var maxcount = _shoplist.Count;
            for (int i = 0; i < maxcount; i++)
            {
                _shoplist_dic.Add(_shoplist[i]._shopid, _shoplist[i]);
            }
        }

        return _shoplist_dic;
    }

    bool CheckMoney(St_ShopTable shopdata)
    {
        var itemvalue = UserData._userdata._userinventory.GetUserItemData(shopdata._priceitemid).itemdata._itemvalue;
        return itemvalue >= shopdata._price;
    }

    List<St_RewardItemList> GetItemOpen(St_RewardItemList sellitemlist)
    {
        var itemdata = DataManager.instance.GetItemTable().SearchItemData(sellitemlist._itemid);

        if (itemdata._itemid == 0)
        {
            return default;
        }

        List<St_RewardItemList> getitemopenList = new List<St_RewardItemList>();

        if (itemdata._itemkind != EITEMKIND.GACHA)
        {
            getitemopenList.Add(sellitemlist);
            return getitemopenList;
        }


        for (int i = 0; i < sellitemlist._itemvalue; i++)
        {
            var gacharesult = DataManager.instance.GetGachaTable().OpenGacha(itemdata._connecttableid);

            St_RewardItemList gachaopenitemlist = new St_RewardItemList();
            gachaopenitemlist._itemid = gacharesult._itemid;
            gachaopenitemlist._itemvalue = gacharesult._itemvalue;

            getitemopenList.Add(gachaopenitemlist);
        }

        return getitemopenList;
    }

    public void BuyProduct(int shopid)
    {
        if (GetShopData().TryGetValue(shopid, out var shopdata) == false)
        {
            //TODO: 상품이 무조건 있는 것으로 할 것이기 때문에 상품이 없다면 에러 송출
            Debug.LogError("상품이 없습니다!");
            return;
        }

        // if (CheckMoney(shopdata) == false)
        // {
        //     //TODO: 돈이 부족하다는 시스템 메시지 띄우기
        //     Debug.Log("재화가 부족합니다!");
        //     return;
        // }

        //상품 가격만큼 아이템 제거
        UserData._userdata._userinventory.UpdateItemData(shopdata._priceitemid, -shopdata._price);

        var totalresult = new List<St_RewardItemList>();

        var maxcount = shopdata._sellitemlist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            totalresult.AddRange(GetItemOpen(shopdata._sellitemlist[i]));
        }

        RewardManager.instance.CraeteReward(totalresult);
    }
}

[Serializable]
public struct St_ShopTable
{
    public int _shopid;
    public int _priceitemid;
    public int _price;
    public string _title;
    public List<St_RewardItemList> _sellitemlist;

    public string GetPriceText()
    {
        if (_priceitemid == -1)
        {
            return "AD";
        }

        return _price.ToString();
    }
}

[Serializable]
public struct St_RewardItemList
{
    public int _itemid;
    public int _itemvalue;
}