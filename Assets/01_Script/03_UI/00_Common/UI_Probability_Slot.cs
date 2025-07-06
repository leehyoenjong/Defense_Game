using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Probability_Slot : MonoBehaviour
{
    [SerializeField] UI_ItemSlot _slot;
    [SerializeField] TextMeshProUGUI _percent;
    const string PERCENT = "{0}%";
    public void Setting(St_GachaItemList itemlist)
    {
        _slot.Setting(itemlist._itemid, itemlist._itemvalue);
        _percent.text = string.Format(PERCENT, itemlist._percent);
    }
}