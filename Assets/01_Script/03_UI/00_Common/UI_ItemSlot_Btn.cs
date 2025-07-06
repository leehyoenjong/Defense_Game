using System;
using UnityEngine;
using UnityEngine.Events;

public class UI_ItemSlot_Btn : UI_ItemSlot
{
    [SerializeField] GameObject _click;
    [SerializeField] UnityEvent<UI_ItemSlot_Btn> _clickevent;

    public void Click()
    {
        _click.SetActive(true);
    }

    public void DisableClick()
    {
        _click.SetActive(false);
    }

    public void Setting(int itemid, int itemvalue, UnityAction<UI_ItemSlot_Btn> clickaction)
    {
        DisableClick();
        base.Setting(itemid, itemvalue);
        _clickevent.RemoveAllListeners();
        _clickevent.AddListener(clickaction);
    }

    public void Btn_Clikc()
    {
        _clickevent?.Invoke(this);
    }
}