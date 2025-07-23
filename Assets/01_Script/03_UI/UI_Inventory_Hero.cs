using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UI_Inventory_Hero : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _explain;
    [SerializeField] TextMeshProUGUI _explain_value;
    [SerializeField] Transform _heroslotparent;
    [SerializeField] Transform _heroskillinfoparent;
    [SerializeField] GameObject _heroslot;
    [SerializeField] GameObject _heroskillinfoslot;
    [SerializeField] UI_Inventory_EquipWindow _inventory_equipwindow;
    [SerializeField] Animator _herobody;

    List<UI_ItemSlotUse_Btn> _activeheroslot = new List<UI_ItemSlotUse_Btn>();
    List<UI_Inventory_Hero_SkillInfo> _inventory_skill_infos = new List<UI_Inventory_Hero_SkillInfo>();
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
        var list = UserData._userdata._userequiphero.GetEquipHeroList();
        var maxcount = itemherolist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            var slot = Instantiate(_heroslot, _heroslotparent).GetComponent<UI_ItemSlotUse_Btn>();
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
        if (heroinfo._npc._mid == 0)
        {
            //TODO: 무조건 있다는 가정 
            Debug.LogError("캐릭터 정보가 없습니다.");
            return;
        }

        //더블클릭한거임
        if (_clickheroslot == slot)
        {
            if (UserData._userdata._userinventory.GetUserItemData(_clickheroslot.GetItemID()).itemdata._itemvalue > 0)
            {
                return;
            }

            _inventory_equipwindow.SetActive(true, _clickheroslot.GetItemID());
            return;
        }
        _explain.text = string.Format(ITEMEXPLAIN, heroinfo._npc._name);
        Setting_SkillInfo(heroinfo, slot);
        Setting_Status(heroitemid);
        Setting_HeroBody(heroitemid);
    }

    void Setting_SkillInfo(St_HeroTable heroinfo, UI_ItemSlotUse_Btn slot)
    {
        slot.Click();

        var maxcount = _inventory_skill_infos.Count;
        for (int i = 0; i < maxcount; i++)
        {
            _inventory_skill_infos[i].gameObject.SetActive(false);
        }

        var skillidlist = heroinfo._npc._skill_chose_list.Select(x => x._skillInfo._mid).Distinct().ToList();
        maxcount = skillidlist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            var choseskill = heroinfo._npc._skill_chose_list.First(x => x._skillInfo._mid == skillidlist[i]);

            UI_Inventory_Hero_SkillInfo skillinfoslot = null;
            if (i < _inventory_skill_infos.Count)
            {
                skillinfoslot = _inventory_skill_infos[i];
            }
            else
            {
                skillinfoslot = Instantiate(_heroskillinfoslot, _heroskillinfoparent).GetComponent<UI_Inventory_Hero_SkillInfo>();
                _inventory_skill_infos.Add(skillinfoslot);
            }
            skillinfoslot.gameObject.SetActive(true);
            skillinfoslot.Setting(choseskill);
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

    void Setting_HeroBody(int heroitemid)
    {
        var connectheroid = DataManager.instance.GetItemTable().GetItemdata()[heroitemid]._connecttableid;
        var heroinfo = DataManager.instance.GetHeroData(connectheroid);
        _herobody.runtimeAnimatorController = heroinfo._animator_with_ui;
    }

    /// <summary>
    /// 장착한 영웅 앞으로 땡기기
    /// _inventory_equipwindow 팝업창 종료 이벤트에도 넣어둠
    /// </summary>
    public void SortUseHeroSlot()
    {
        var maxcount = _activeheroslot.Count;
        var userequipherolist = UserData._userdata._userequiphero.GetEquipHeroList();
        for (int i = maxcount - 1; i >= 0; i--)
        {
            //장착여부는 항상 체크
            _activeheroslot[i].UseItem(userequipherolist);
            var isequip = userequipherolist.Contains(_activeheroslot[i].GetItemID());
            if (isequip == false)
            {
                continue;
            }
            _activeheroslot[i].transform.SetAsFirstSibling();

        }
    }
}