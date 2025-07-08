using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Hero_Equip : MonoBehaviour
{
    [SerializeField] Image _icon;
    int _heroitemid;

    public void Setting(int heroitemid)
    {
        _icon.gameObject.SetActive(heroitemid > 0);
        if (heroitemid <= 0)
        {
            return;
        }

        if (DataManager.instance.GetItemTable().GetItemdata().TryGetValue(heroitemid, out var heroitemdata) == false)
        {
            Debug.LogError("영웅 아이디가 없습니다!");
            _icon.gameObject.SetActive(false);
            return;
        }

        var herodata = DataManager.instance.GetHeroData(heroitemdata._connecttableid);
        if (herodata._player_id == 0)
        {
            Debug.LogError("영웅 아이디가 없습니다!");
            _icon.gameObject.SetActive(false);
            return;
        }

        _icon.sprite = herodata._icon;
    }

    public void Btn_Click()
    {
        if (_heroitemid > 0)
        {
            UserData._userdata._userequiphero.UnequipHero(_heroitemid);
            return;
        }

        UserData._userdata._userequiphero.EquipHero(_heroitemid);
    }
}