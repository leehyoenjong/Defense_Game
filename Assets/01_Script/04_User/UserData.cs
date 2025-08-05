using System;
using System.Collections.Generic;

[Serializable]
public class UserData
{
    static UserData _instance;
    public static UserData _userdata
    {
        get
        {
            return _instance;
        }
    }

    public static void Create()
    {
        _instance = new UserData();
    }

    public St_UserChapterData _userchapterdata;
    public St_UserInventory _userinventory;
    public St_UserEquitHero _userequiphero;
    public St_UserQuestData _userquestdata;

    public UserData()
    {
        CreateNewUserData();
        AddAction();
    }

    ~UserData()
    {
        RemoveAction();
    }

    void CreateNewUserData()
    {
        _userchapterdata = new St_UserChapterData();
        _userchapterdata._lastchapternumber = 0;

        _userinventory = new St_UserInventory();
        _userinventory._userinvendata = new List<St_UserInvenItemList>();

        _userequiphero = new St_UserEquitHero();
        _userequiphero._equipheroid = new List<int>() { 10000, 0, 0, 0, 0 };

        _userquestdata = new St_UserQuestData();
        _userquestdata._questclearid = new List<int>();
        _userquestdata._questvaluelist = new List<St_UserQuestList>();
    }

    void AddAction()
    {
        PlayManager._ongamestatechanged += _userchapterdata.UserChapterUpdate;
        Monster_Base._monsterdie += (monster) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.MONSTERKILL, 0, 1);
        Monster_Base._monsterdie += (monster) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.TARGETMONSTERKILL, monster.GetMonsterInfo()._npc._mid, 1);
        UI_Shop_Slot._shop_buy_complted_event += (shopid) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.GACHA, 0, 1);
        UI_Shop_Slot._shop_buy_complted_event += (shopid) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.TARGETGACHA, shopid, 1);
        UI_Inventory_Hero_Grade._upgrade_event += (heroid) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.UPGRADE, 0, 1);
        UI_Inventory_Hero_Grade._upgrade_event += (heroid) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.TARGETHEROUPGRADE, heroid, 1);
        UI_QuestSlot._quest_clear_event += (questinfo) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.QUESTCLEARCOUNT, 0, 1);
        UI_QuestSlot._quest_clear_event += (questinfo) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.TARGETTYPEQUESTCLEARCOUNT, (int)questinfo._questtype, 1);
    }

    void RemoveAction()
    {
        PlayManager._ongamestatechanged -= _userchapterdata.UserChapterUpdate;
        Monster_Base._monsterdie -= (monster) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.MONSTERKILL, 0, 1);
        Monster_Base._monsterdie -= (monster) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.TARGETMONSTERKILL, monster.GetMonsterInfo()._npc._mid, 1);
        UI_Shop_Slot._shop_buy_complted_event -= (shopid) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.GACHA, 0, 1);
        UI_Shop_Slot._shop_buy_complted_event -= (shopid) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.TARGETGACHA, shopid, 1);
        UI_Inventory_Hero_Grade._upgrade_event -= (heroid) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.UPGRADE, 0, 1);
        UI_Inventory_Hero_Grade._upgrade_event -= (heroid) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.TARGETHEROUPGRADE, heroid, 1);
        UI_QuestSlot._quest_clear_event -= (questinfo) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.QUESTCLEARCOUNT, 0, 1);
        UI_QuestSlot._quest_clear_event -= (questinfo) => _userquestdata.UpdateQuestValue(EQUESTVALUETYPE.TARGETTYPEQUESTCLEARCOUNT, (int)questinfo._questtype, 1);
    }
}