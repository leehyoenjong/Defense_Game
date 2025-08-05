using UnityEngine;

public class UI_Shop : MonoBehaviour
{
    [SerializeField] int[] _shopidlist;
    [SerializeField] UI_Shop_Slot[] _shopslotlist;


    void OnEnable()
    {
        SettingShopSlot();
    }

    void SettingShopSlot()
    {
        int maxcount = _shopidlist.Length;
        for (int i = 0; i < maxcount; i++)
        {
            _shopslotlist[i].Setting(_shopidlist[i]);
        }
    }
}
