using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class UI_RewardPopup : MonoBehaviour
{
    [SerializeField] GameObject _createslotparent;
    [SerializeField] GameObject _itemslot;
    [SerializeField] TextMeshProUGUI _exitexplain;

    public void Setting(St_RewardItemList itemlist)
    {
        CreateSlot(itemlist);
        _exitexplain.gameObject.SetActive(false);
    }

    public void Setting(List<St_RewardItemList> itemlist)
    {
        var maxcount = itemlist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            CreateSlot(itemlist[i]);
        }
    }

    void CreateSlot(St_RewardItemList itemlist)
    {
        var itemslot = Instantiate<GameObject>(_itemslot, _createslotparent.transform).GetComponent<UI_ItemSlot>();
        itemslot.Setting(itemlist);
    }
}