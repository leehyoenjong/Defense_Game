using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UI_ItemSlotUse_Btn : UI_ItemSlot_Btn
{
    [SerializeField] GameObject _use;
    [SerializeField] protected UnityEvent<int, UI_ItemSlotUse_Btn> _click_itemid_event;

    public virtual void Setting(int itemid, int itemvalue, UnityAction<int, UI_ItemSlotUse_Btn> clickaction)
    {
        DisableClick();
        base.Setting(itemid, itemvalue);
        _click_itemid_event.RemoveAllListeners();
        _click_itemid_event.AddListener(clickaction);
    }


    /// <summary>
    /// 장착하는 아이템의 데이터가 각각 다를 수 있기 때문에 매개변수로 따로 빼서 처리
    /// </summary>
    /// <param name="equiplist"></param>
    public void UseItem(List<int> equiplist)
    {
        _use.SetActive(equiplist.Contains(_itemid));
    }

    public void Btn_Click_itemid()
    {
        _click_itemid_event?.Invoke(_itemid, this);
    }
}