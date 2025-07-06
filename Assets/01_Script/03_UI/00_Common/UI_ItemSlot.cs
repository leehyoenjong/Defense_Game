using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour
{
    [SerializeField] RectTransform _panel;
    [SerializeField] RectTransform _iconrect;
    [SerializeField] Image _icon;
    [SerializeField] TextMeshProUGUI _value;


    int _itemid;
    public int GetItemID() => _itemid;

    public void Setting(int itemid, int itemvalue)
    {
        if (DataManager.instance.GetItemTable().GetItemdata().TryGetValue(itemid, out var itemdata) == false)
        {
            //TODO: 아이템이 없습니다. 무조건 있어야함 에러 송출
            Debug.LogError($"아이템{itemid} 이 없습니다!");
            return;
        }
        _itemid = itemid;
        _icon.sprite = itemdata._itemicon;
        _value.text = itemvalue.ToString();
    }
    public void Setting(int itemid, int itemvalue, float panelsize, float iconsize)
    {
        Setting(itemid, itemvalue);
        _panel.sizeDelta = new Vector2(panelsize, panelsize);
        _iconrect.sizeDelta = new Vector2(iconsize, iconsize);
    }


    public void Setting(St_RewardItemList itemlist)
    {
        if (DataManager.instance.GetItemTable().GetItemdata().TryGetValue(itemlist._itemid, out var itemdata) == false)
        {
            //TODO: 아이템이 없습니다. 무조건 있어야함 에러 송출
            Debug.LogError($"아이템{itemlist._itemid} 이 없습니다!");
            return;
        }
        _itemid = itemlist._itemid;
        _icon.sprite = itemdata._itemicon;
        _value.text = itemlist._itemvalue.ToString();
    }

    public void Setting(St_RewardItemList itemlist, float panelsize, float iconsize)
    {
        Setting(itemlist);
        _panel.sizeDelta = new Vector2(panelsize, panelsize);
        _iconrect.sizeDelta = new Vector2(iconsize, iconsize);
    }
}