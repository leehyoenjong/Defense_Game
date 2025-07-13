using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Hero_Equip : MonoBehaviour
{
    [SerializeField] Image _icon;

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
        if (herodata._npc._mid == 0)
        {
            Debug.LogError("영웅 아이디가 없습니다!");
            _icon.gameObject.SetActive(false);
            return;
        }

        _icon.sprite = herodata._npc._icon;
    }
}