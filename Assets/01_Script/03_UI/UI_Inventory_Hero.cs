using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Hero : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _explain;
    [SerializeField] TextMeshProUGUI _explain_value;
    [SerializeField] Image[] _upgrade;
    [SerializeField] Sprite[] _upgradestar;
    [SerializeField] Transform _parent;
    [SerializeField] GameObject _slot;
    [SerializeField] UI_Inventory_Hero_SkillInfo[] _inventory_skill_infos;
    [SerializeField] UI_Inventory_Hero_Equip[] _inventory_equip_heros;

    UI_ItemSlotUse_Btn _clickheroslot;

    const string ITEMEXPLAIN = "{0}\n\nDAMAGE:\nCRITICAL:\nCRITICAL DAMAGE:";
    const string ITEMEXPLAINVALUE = "\n\n{0}\n{1}\n{2}";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SettingSlot();
        SettingEquipHero();
    }

    void SettingSlot()
    {
        var itemherolist = DataManager.instance.GetItemTable().GetItemdata().Where(x => x.Value._itemkind == EITEMKIND.HERO).ToList();

        var maxcount = itemherolist.Count;
        var list = UserData._userdata._userequiphero.GetEquipHeroList();
        for (int i = 0; i < maxcount; i++)
        {
            var slot = Instantiate(_slot, _parent).GetComponent<UI_ItemSlotUse_Btn>();
            var itemid = itemherolist[i].Value._itemid;
            var uservalue = UserData._userdata._userinventory.GetUserItemData(itemid).itemdata._itemvalue;
            slot.Setting(itemid, uservalue, SettingSkillInfo);
            slot.UseItem(list);
            if (i == 0)
            {
                SettingSkillInfo(itemid, slot);
            }
        }
    }

    void SettingSkillInfo(int itemid, UI_ItemSlotUse_Btn slot)
    {
        var connectheroid = DataManager.instance.GetItemTable().GetItemdata()[itemid]._connecttableid;
        var heroinfo = DataManager.instance.GetHeroData(connectheroid);
        if (heroinfo._player_id == 0)
        {
            //TODO: 무조건 있다는 가정 
            Debug.LogError("캐릭터 정보가 없습니다.");
            return;
        }

        //더블클릭한거임
        if (_clickheroslot == slot)
        {
            return;
        }

        SettingSkillInfo(heroinfo, slot);
    }

    void SettingSkillInfo(St_PlayerList heroinfo, UI_ItemSlotUse_Btn slot)
    {
        slot.Click();
        _explain.text = string.Format(ITEMEXPLAIN, heroinfo._name);
        var critical = heroinfo._npc._status._critical * 100f;
        var criticaldamage = heroinfo._npc._status._critical * 100f;
        _explain_value.text = string.Format(ITEMEXPLAINVALUE, heroinfo._npc._status._damge, critical.ToString("F1") + "%", criticaldamage.ToString("F1") + "%");

        var skillidlist = heroinfo._npc._skill_chose_list.Select(x => x._skillInfo._mid).Distinct().ToList();
        var maxcount = skillidlist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            var choseskill = heroinfo._npc._skill_chose_list.First(x => x._skillInfo._mid == skillidlist[i]);
            _inventory_skill_infos[i].Setting(choseskill);
        }

        if (_clickheroslot)
        {
            _clickheroslot.DisableClick();

        }
        _clickheroslot = slot;
    }

    /// <summary>
    /// 영웅 장착 창이 꺼지면 불리도록 인스펙터 이벤트에 넣어둠
    /// </summary>
    public void SettingEquipHero()
    {
        var equipheroidlist = UserData._userdata._userequiphero.GetEquipHeroList();

        var maxcount = _inventory_equip_heros.Length;
        for (int i = 0; i < maxcount; i++)
        {
            _inventory_equip_heros[i].Setting(equipheroidlist[i]);
        }
    }
}