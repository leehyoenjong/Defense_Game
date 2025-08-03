using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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

    public async UniTask LoadTable()
    {
        _questtable = await AddressableSystem.LoadAsync<SO_QuestTable>("SO_QuestTable");
        _chapterdata = await AddressableSystem.LoadAsync<SO_ChapterData>("SO_ChapterData");
        _playerprefablist = await AddressableSystem.LoadAsync<SO_HeroTable>("SO_HeroTable");
        _mosntertable = await AddressableSystem.LoadAsync<SO_MonsterTable>("SO_MonsterTable");
        _statustable = await AddressableSystem.LoadAsync<SO_Status_Table>("SO_Status_Table");
        _upgradtable = await AddressableSystem.LoadAsync<SO_Upgrade_Table>("SO_Upgrade_Table");
        _shoptable = await AddressableSystem.LoadAsync<SO_Shop_Table>("SO_Shop_Table");
        _gachatable = await AddressableSystem.LoadAsync<SO_Gacha_Table>("SO_Gacha_Table");
        _itemtable = await AddressableSystem.LoadAsync<SO_Item_Table>("SO_Item_Table");
    }
}