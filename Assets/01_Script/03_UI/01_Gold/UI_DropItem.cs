using TMPro;
using UnityEngine;

public class UI_DropItem : MonoBehaviour
{
    [SerializeField] int _itemid;
    [SerializeField] TextMeshProUGUI _drop_value;

    void Start()
    {
        DropItemObject._drop_item_event += ItemSetting;
        DropItemManager._ingameitem_event += ItemSetting;
    }

    void OnDisable()
    {
        DropItemObject._drop_item_event -= ItemSetting;
        DropItemManager._ingameitem_event -= ItemSetting;
    }


    void ItemSetting(int dropitemid, int dropvalue)
    {
        if (_itemid != dropitemid)
        {
            return;
        }

        _drop_value.text = dropvalue.ToString();
    }
}