using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory_EquipWindow : MonoBehaviour
{
    [SerializeField] List<UI_ItemSlot_Btn> _inventory_equio_hero;
    int _choseheroid;

    public void SetActive(bool isactive, int heroitemid)
    {
        this.gameObject.SetActive(isactive);
        SettingEquipHero();
        _choseheroid = heroitemid;
    }

    void SettingEquipHero()
    {
        var equipheroidlist = UserData._userdata._userequiphero.GetEquipHeroList();

        var maxcount = _inventory_equio_hero.Count;
        for (int i = 0; i < maxcount; i++)
        {
            _inventory_equio_hero[i].Setting(equipheroidlist[i], 1, Btn_Click);
        }
    }

    void Btn_Click(UI_ItemSlot_Btn slot)
    {
        var idx = _inventory_equio_hero.FindIndex(x => x == slot);
        if (slot.GetItemID() > 0)
        {
            UserData._userdata._userequiphero.UnequipHero(slot.GetItemID());
        }

        UserData._userdata._userequiphero.EquipHero(_choseheroid, idx);
        this.gameObject.SetActive(false);
    }
}