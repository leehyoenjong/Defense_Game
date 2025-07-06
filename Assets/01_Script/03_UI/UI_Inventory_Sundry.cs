using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Sundry : MonoBehaviour
{
    [SerializeField] GameObject _slotparent;
    [SerializeField] GameObject _slot;
    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] TextMeshProUGUI _explain;
    [SerializeField] Image _itemicon;

    UI_ItemSlot_Btn _clickitemslot;

    void Start()
    {
        SettingSlot();
    }

    void SettingSlot()
    {
        //영웅, 가챠 상자 같은 아이템을 제외하고 가져올 것 
        var itemlist = DataManager.instance.GetItemTable()._itemlist.FindAll(x => x._itemkind != EITEMKIND.HERO && x._itemkind != EITEMKIND.GACHA);

        var maxcount = itemlist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            var itemslot = Instantiate(_slot, _slotparent.transform).GetComponent<UI_ItemSlot_Btn>();
            var useritemslot = UserData._userdata._userinventory.GetUserItemData(itemlist[i]._itemid);
            itemslot.Setting(useritemslot.itemdata._itemid, useritemslot.itemdata._itemvalue, Btn_Click);

            //선택하지 않아도 첫번째 슬롯이 선택되어 있도록 하기 위함
            if (i == 0)
            {
                Btn_Click(itemslot);
            }
        }
    }

    /// <summary>
    /// UI_ItemSlot에 UnityEvent를 이용
    /// </summary>
    void Btn_Click(UI_ItemSlot_Btn slot)
    {
        var itemid = slot.GetItemID();
        if (DataManager.instance.GetItemTable().GetItemdata().TryGetValue(itemid, out var itemdata) == false)
        {
            //에러 송출 무조건 있어야함!
            Debug.LogError("아이템 정보가 없습니다!");
            return;
        }

        _title.text = itemdata._itemname;
        _explain.text = itemdata._itemexplain;
        _itemicon.sprite = itemdata._itemicon;

        //이전 선택한거 해제 
        if (_clickitemslot)
        {
            _clickitemslot.DisableClick();
        }
        _clickitemslot = slot;
        _clickitemslot.Click();
    }
}