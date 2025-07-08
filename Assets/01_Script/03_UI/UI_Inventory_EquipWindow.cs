using UnityEngine;

public class UI_Inventory_EquipWindow : MonoBehaviour
{
    [SerializeField] UI_ItemSlot_Btn[] _inventory_equio_hero;

    void OnEnable()
    {
        SettingEquipHero();
    }

    void SettingEquipHero()
    {
        var equipheroidlist = UserData._userdata._userequiphero.GetEquipHeroList();

        var maxcount = _inventory_equio_hero.Length;
        for (int i = 0; i < maxcount; i++)
        {
            _inventory_equio_hero[i].Setting(equipheroidlist[i], 0);
        }
    }
}