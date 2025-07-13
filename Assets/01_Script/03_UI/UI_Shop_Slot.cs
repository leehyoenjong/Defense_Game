using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Shop_Slot : MonoBehaviour
{
    [SerializeField] Image _priceicon;
    [SerializeField] TextMeshProUGUI _titleandprice;

    const string VEIWTEXT = "{0}\n<size=50><color=yellow>{1}</color></size>";
    int _shopid;
    public static event Action<int> _shop_buy_complted_event;

    public void Setting(int shopid)
    {
        if (DataManager.instance.GetShopTable().GetShopData().TryGetValue(shopid, out var shopdata) == false)
        {
            //TODO: 무조건 데이터가 있어야기 하기 때문에 없다면 에러 송출
            Debug.LogError("상품이 없습니다 체크하세요!");
            this.gameObject.SetActive(false);
            return;
        }
        _shopid = shopid;

        _titleandprice.text = string.Format(VEIWTEXT, shopdata._title, shopdata.GetPriceText());

        //-1 이하부턴 특수한 광고나 인앱 결제 상품이기 때문에 아이콘은 제거
        if (shopdata._priceitemid <= -1)
        {
            _priceicon.gameObject.SetActive(false);
            return;
        }

        if (DataManager.instance.GetItemTable().GetItemdata().TryGetValue(shopdata._priceitemid, out var itemdata) == false)
        {
            //TODO: 무조건 데이터가 있어야기 하기 때문에 없다면 에러 송출
            Debug.LogError("가격 데이터가 없습니다 체크하세요!");
            this.gameObject.SetActive(false);
            return;
        }
        _priceicon.sprite = itemdata._itemicon;
    }

    public void Btn_Buy()
    {
        var buycheck = DataManager.instance.GetShopTable().BuyProduct(_shopid);
        if (buycheck == false)
        {
            return;
        }

        _shop_buy_complted_event?.Invoke(_shopid);
    }

    public void SettingProbability(GameObject popup)
    {
        if (DataManager.instance.GetShopTable().GetShopData().TryGetValue(_shopid, out var shopdata) == false)
        {
            //TODO: 무조건 데이터가 있어야기 하기 때문에 없다면 에러 송출
            Debug.LogError("상품이 없습니다 체크하세요!");
            return;
        }

        var itemtable = DataManager.instance.GetItemTable().GetItemdata();
        var gachaitem = shopdata._sellitemlist.Where(x => itemtable.ContainsKey(x._itemid)).Select(x => (x._itemid, itemtable[x._itemid]._connecttableid)).First(x => itemtable[x._itemid]._itemkind == EITEMKIND.GACHA);

        if (gachaitem._itemid == 0)
        {
            //TODO: 무조건 데이터가 있어야기 하기 때문에 없다면 에러 송출
            Debug.LogError("상품이 없습니다 체크하세요!");
            return;
        }

        popup.GetComponent<UI_Probability>().Setting(gachaitem._connecttableid);
    }
}