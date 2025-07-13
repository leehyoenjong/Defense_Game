using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [SerializeField] SO_Item_Table _itemtable;
    public SO_Item_Table GetItemTable() => _itemtable;
    [SerializeField] SO_Gacha_Table _gachatable;
    public SO_Gacha_Table GetGachaTable() => _gachatable;
    [SerializeField] SO_Shop_Table _shoptable;
    public SO_Shop_Table GetShopTable() => _shoptable;

    [SerializeField] SO_Upgrade_Table _upgradtable;
    public St_UpgradeTable GetUpgradeData(int curretgrade) => _upgradtable.GetUpgradeData(curretgrade);

    [SerializeField] SO_Status_Table _statustable;
    public List<St_Status> GetStatusData(int statusid) => _statustable.GetStatusData(statusid);

    [SerializeField] SO_MonsterTable _mosntertable;
    public St_MonsterTable GetMonsterInfo(int monsterid) => _mosntertable.GetMonsterInfo(monsterid);

    [SerializeField] SO_HeroTable _playerprefablist;
    public St_HeroTable GetHeroData(int heroid) => _playerprefablist.GetHeroList(heroid);
    public SO_HeroTable GetHeroData() => _playerprefablist;

    [SerializeField] SO_ChapterData _chapterdata;
    public St_ChapterData GetChapterData(int chapterid) => _chapterdata.GetChapterData(chapterid);
    public SO_StageData GetStageData(int chapterid, int stageid) => GetChapterData(chapterid)._stagedata.Find(x => x._stageid == stageid);


    [SerializeField] SO_QuestTable _questtable;
    public SO_QuestTable GetQuestTable() => _questtable;
    public St_QuestTable GetQuestInfo(int questid) => _questtable.GetQuestInfo(questid);

    void Awake()
    {
        instance = this;
    }
}