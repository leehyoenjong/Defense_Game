using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Hero : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _explain;
    [SerializeField] TextMeshProUGUI _explain_value;
    [SerializeField] Transform _parent;
    [SerializeField] GameObject _slot;
    [SerializeField] UI_Inventory_Hero_SkillInfo[] _inventory_skill_infos;
    [SerializeField] UI_Inventory_EquipWindow _inventory_equipwindow;

    List<UI_ItemSlotUse_Btn> _activeheroslot = new List<UI_ItemSlotUse_Btn>();
    UI_ItemSlotUse_Btn _clickheroslot;

    public static event Action<int> _click_slot;

    const string ITEMEXPLAIN = "{0}\n\nDAMAGE:\nCRITICAL:\nCRITICAL DAMAGE:";
    const string ITEMEXPLAINVALUE = "\n\n{0}\n{1}\n{2}";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SettingSlot();
    }

    void OnEnable()
    {
        UI_Inventory_Hero_Grade._upgrade_event += Setting_Status;
    }
    void OnDisable()
    {
        UI_Inventory_Hero_Grade._upgrade_event -= Setting_Status;
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
            slot.Setting(itemid, uservalue, Setting);
            slot.UseItem(list);
            if (i == 0)
            {
                Setting(itemid, slot);
            }
            _activeheroslot.Add(slot);
        }
    }

    void Setting(int heroitemid, UI_ItemSlotUse_Btn slot)
    {
        var connectheroid = DataManager.instance.GetItemTable().GetItemdata()[heroitemid]._connecttableid;
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
            _inventory_equipwindow.SetActive(true, _clickheroslot.GetItemID());
            return;
        }
        _explain.text = string.Format(ITEMEXPLAIN, heroinfo._name);
        Setting_SkillInfo(heroinfo, slot);
        Setting_Status(heroitemid);
    }

    void Setting_SkillInfo(St_PlayerList heroinfo, UI_ItemSlotUse_Btn slot)
    {
        slot.Click();

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
        _click_slot?.Invoke(slot.GetItemID());
    }

    void Setting_Status(int heroitemid)
    {
        var connectheroid = DataManager.instance.GetItemTable().GetItemdata()[heroitemid]._connecttableid;
        var heroinfo = DataManager.instance.GetHeroData(connectheroid);
        var critical = heroinfo._npc.GetStatus(heroitemid)._critical * 100f;
        var criticaldamage = heroinfo._npc.GetStatus(heroitemid)._critical * 100f;
        _explain_value.text = string.Format(ITEMEXPLAINVALUE, heroinfo._npc.GetStatus(heroitemid)._damge, critical.ToString("F1") + "%", criticaldamage.ToString("F1") + "%");
    }

    /// <summary>
    /// 장착한 영웅 앞으로 땡기기
    /// _inventory_equipwindow 팝업창 종료 이벤트에도 넣어둠
    /// </summary>
    public void SortUseHeroSlot()
    {
        var maxcount = _activeheroslot.Count;
        var userequipherolist = UserData._userdata._userequiphero.GetEquipHeroList();
        for (int i = 0; i < maxcount; i++)
        {
            var isequip = userequipherolist.Contains(_activeheroslot[i].GetItemID());
            if (isequip == false)
            {
                continue;
            }
            _activeheroslot[i].transform.SetAsFirstSibling();
        }
    }
}