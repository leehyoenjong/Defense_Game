using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// 에디터용 강화 시뮬레이션 클래스
public class StatusUpgradeManagerSim
{
    // Key: Hero ID, Value: { Key: Upgrade Type, Value: Level }
    private Dictionary<int, Dictionary<ESTATUSUPGRADE, int>> _statusupgrade = new Dictionary<int, Dictionary<ESTATUSUPGRADE, int>>();
    
    // StatusUpgradeManager의 상수값들을 그대로 가져옴
    private readonly List<float> _maxlevelvalue = new List<float>() { 0, 1000, 1, 1, 1000, 100 };
    private const int MAXCOINVALUE = 100000;
    private const int MAXLEVEL = 100;

    public int GetUpgradeLevel(int heroid, ESTATUSUPGRADE type)
    {
        if (_statusupgrade.TryGetValue(heroid, out var upgrades) && upgrades.TryGetValue(type, out int level))
        {
            return level;
        }
        return 0;
    }
    
    public void ResetAllUpgrades()
    {
        _statusupgrade.Clear();
    }
    
    public void ResetHeroUpgrades(int heroid)
    {
        if (_statusupgrade.ContainsKey(heroid))
        {
            _statusupgrade.Remove(heroid);
        }
    }

    public int GetTotalCostForHero(int heroid)
    {
        int totalCost = 0;
        if (_statusupgrade.TryGetValue(heroid, out var upgrades))
        {
            foreach (var upgrade in upgrades)
            {
                for (int level = 0; level < upgrade.Value; level++)
                {
                    totalCost += GetNextUpgradeCost(level);
                }
            }
        }
        return totalCost;
    }

    public void ApplyUpgrade(int heroid, ESTATUSUPGRADE type)
    {
        if (!_statusupgrade.ContainsKey(heroid))
        {
            _statusupgrade[heroid] = new Dictionary<ESTATUSUPGRADE, int>();
        }
        if (!_statusupgrade[heroid].ContainsKey(type))
        {
            _statusupgrade[heroid][type] = 0;
        }
        _statusupgrade[heroid][type]++;
    }
    
    public int GetNextUpgradeCost(int currentLevel)
    {
        int nextLevel = currentLevel + 1;
        if (nextLevel > MAXLEVEL) return int.MaxValue;
        
        return (MAXCOINVALUE / MAXLEVEL) * nextLevel;
    }

    public St_Status GetTotalUpgradeValue(int heroid, St_Status baseStatus)
    {
        var upgradeStatus = new St_Status();
        if (!_statusupgrade.ContainsKey(heroid)) return upgradeStatus;

        foreach (var upgrade in _statusupgrade[heroid])
        {
            ESTATUSUPGRADE type = upgrade.Key;
            int level = upgrade.Value;
            if (level <= 0) continue;

            float percentPerLevel = _maxlevelvalue[(int)type] / (MAXLEVEL - 1);
            float totalValue = percentPerLevel * (level - 1);

            switch (type)
            {
                case ESTATUSUPGRADE.ATTACKPER:
                    upgradeStatus._damge += Mathf.FloorToInt(baseStatus._damge * (totalValue / 100f));
                    break;
                case ESTATUSUPGRADE.CRITICALPER:
                    upgradeStatus._critical += totalValue;
                    break;
                case ESTATUSUPGRADE.CRITICALDAMAGE:
                    upgradeStatus._critical_damage += totalValue;
                    break;
            }
        }
        return upgradeStatus;
    }
}

public class HeroPlacementEditor : EditorWindow
{
    private class HeroSimulationData
    {
        public SO_NPC Npc;
        public int Grade = 1;
        
        public St_Status GetBaseStatus(SO_Status_Table statusTable)
        {
            if (Npc == null || statusTable == null) return default;
            
            var statusData = statusTable.GetStatusData(Npc._statusid);
            if (statusData == null || statusData.Count == 0) return default;
            
            var gradeStatus = statusData.Find(s => s._grade == Grade);
            if(gradeStatus._grade == 0)
            {
                gradeStatus = statusData.OrderBy(s => s._grade).LastOrDefault(s => s._grade <= Grade);
            }
            return gradeStatus;
        }

        public St_Status GetUpgradedStatus(SO_Status_Table statusTable, StatusUpgradeManagerSim sim)
        {
            St_Status currentStatus = GetBaseStatus(statusTable);
            if(Npc == null) return currentStatus;

            St_Status upgradeValue = sim.GetTotalUpgradeValue(Npc._mid, currentStatus);
            currentStatus._damge += upgradeValue._damge;
            currentStatus._critical += upgradeValue._critical;
            currentStatus._critical_damage += upgradeValue._critical_damage;
            
            return currentStatus;
        }
    }

    private const int MAX_SLOTS = 5;
    private HeroSimulationData[] _heroSlots = new HeroSimulationData[MAX_SLOTS];
    private int _simulatedStage = 1;
    private Dictionary<int, int> _totalRewards = new Dictionary<int, int>();
    private Dictionary<int, int> _availableCurrency = new Dictionary<int, int>();

    private SO_HeroTable _heroTable;
    private SO_MonsterTable _monsterTable;
    private SO_Item_Table _itemTable;
    private SO_Status_Table _statusTable;
    private SO_ChapterData _chapterData;

    private StatusUpgradeManagerSim _upgradeSim;
    private Vector2 _scrollPosition;
    private List<St_ChapterData> _chapterListCache;

    [MenuItem("Tools/Hero Placement Editor")]
    public static void ShowWindow()
    {
        GetWindow<HeroPlacementEditor>("Hero Placement");
    }

    private void OnEnable()
    {
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            _heroSlots[i] = new HeroSimulationData();
        }
        LoadAllTables();
        _upgradeSim = new StatusUpgradeManagerSim();
        UpdateRewardsAndCurrency(_simulatedStage);
    }

    private void OnGUI()
    {
        if (_heroTable == null)
        {
            EditorGUILayout.HelpBox("필요한 테이블 에셋들을 찾을 수 없습니다.", MessageType.Error);
            if (GUILayout.Button("테이블 다시 불러오기")) LoadAllTables();
            return;
        }

        EditorGUILayout.LabelField("영웅 배치 및 강화 시뮬레이터", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawStageAndCurrencySetup();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        for(int i = 0; i < MAX_SLOTS; i++)
        {
            DrawHeroSlot(i);
        }

        EditorGUILayout.EndScrollView();

        DrawTotalCombatPower();
    }

    private void DrawStageAndCurrencySetup()
    {
        EditorGUI.BeginChangeCheck();
        _simulatedStage = EditorGUILayout.IntField("현재 클리어한 스테이지", _simulatedStage);
        if (EditorGUI.EndChangeCheck())
        {
            if (_simulatedStage < 1) _simulatedStage = 1;
            UpdateRewardsAndCurrency(_simulatedStage);
        }
        
        EditorGUILayout.LabelField("보유 재화:", EditorStyles.boldLabel);
        if (_availableCurrency.Count > 0)
        {
            foreach(var currency in _availableCurrency)
            {
                string itemName = _itemTable.SearchItemData(currency.Key)._itemname ?? $"ItemID: {currency.Key}";
                EditorGUILayout.LabelField($"    {itemName}", currency.Value.ToString());
            }
        }
        else
        {
            EditorGUILayout.LabelField("    (재화 없음)");
        }
    }
    
    private void DrawHeroSlot(int index)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"영웅 슬롯 #{index + 1}", EditorStyles.boldLabel);

        string heroName = _heroSlots[index].Npc != null ? _heroSlots[index].Npc.name : "(영웅 없음)";
        
        if (EditorGUILayout.DropdownButton(new GUIContent(heroName), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("(비우기)"), _heroSlots[index].Npc == null, () => {
                HandleHeroChange(index, null);
            });
            
            foreach(var hero in _heroTable.GetHeroList())
            {
                menu.AddItem(new GUIContent(hero._npc.name), _heroSlots[index].Npc == hero._npc, () => {
                    HandleHeroChange(index, hero._npc);
                });
            }
            menu.ShowAsContext();
        }
        
        if(_heroSlots[index].Npc != null)
        {
            _heroSlots[index].Grade = EditorGUILayout.IntSlider("등급", _heroSlots[index].Grade, 1, 5);
            DrawUpgradeButtons(index);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    private void HandleHeroChange(int slotIndex, SO_NPC newNpc)
    {
        SO_NPC oldNpc = _heroSlots[slotIndex].Npc;
        if(oldNpc == newNpc) return;

        if(oldNpc != null)
        {
            RefundUpgrades(oldNpc._mid);
        }

        _heroSlots[slotIndex].Npc = newNpc;
        UpdateAvailableCurrency();
    }
    
    private void RefundUpgrades(int heroId)
    {
        int totalCost = _upgradeSim.GetTotalCostForHero(heroId);
        int currencyId = 1;
        if (totalCost > 0)
        {
             if(!_availableCurrency.ContainsKey(currencyId)) _availableCurrency[currencyId] = 0;
            _availableCurrency[currencyId] += totalCost;
        }
        _upgradeSim.ResetHeroUpgrades(heroId);
    }


    private void DrawUpgradeButtons(int index)
    {
        EditorGUILayout.LabelField("인게임 강화", EditorStyles.miniBoldLabel);
        
        var heroData = _heroSlots[index];
        if (heroData.Npc == null) return;

        EditorGUILayout.BeginHorizontal();
        foreach (ESTATUSUPGRADE upgradeType in System.Enum.GetValues(typeof(ESTATUSUPGRADE)))
        {
            if (upgradeType == ESTATUSUPGRADE.NONE || upgradeType == ESTATUSUPGRADE.PROTECTARMOR || upgradeType == ESTATUSUPGRADE.PROTECTMAXHPPER) continue;

            int currentLevel = _upgradeSim.GetUpgradeLevel(heroData.Npc._mid, upgradeType);
            int cost = _upgradeSim.GetNextUpgradeCost(currentLevel);
            
            int currencyId = 1; 
            bool canAfford = _availableCurrency.ContainsKey(currencyId) && _availableCurrency[currencyId] >= cost;

            string buttonText = $"{upgradeType.ToString()}\n(Lv.{currentLevel}) Cost: {cost}";

            EditorGUI.BeginDisabledGroup(!canAfford);
            if (GUILayout.Button(buttonText, GUILayout.Height(40)))
            {
                _availableCurrency[currencyId] -= cost;
                _upgradeSim.ApplyUpgrade(heroData.Npc._mid, upgradeType);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTotalCombatPower()
    {
        float totalPower = 0;
        foreach(var heroData in _heroSlots)
        {
            if(heroData.Npc != null)
            {
                St_Status finalStatus = heroData.GetUpgradedStatus(_statusTable, _upgradeSim);
                totalPower += CalculateCombatPower(finalStatus);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("팀 총 전투력", $"{totalPower:N0}", EditorStyles.boldLabel);
    }
    
    private float CalculateCombatPower(St_Status status)
    {
        float attackPower = status._damge * (1 + status._critical * status._critical_damage);
        float defensePower = status._hp * (1 + status._armor / 100f);
        return attackPower + defensePower;
    }
    
    private void UpdateRewardsAndCurrency(int targetStage)
    {
        CalculateTotalRewardsUpTo(targetStage);
        
        int spentCurrency = 0;
        foreach (var slot in _heroSlots)
        {
            if (slot.Npc != null)
            {
                spentCurrency += _upgradeSim.GetTotalCostForHero(slot.Npc._mid);
            }
        }

        int totalObtained = _totalRewards.ContainsKey(1) ? _totalRewards[1] : 0;
        if (spentCurrency > totalObtained)
        {
            _upgradeSim.ResetAllUpgrades();
            ShowNotification(new GUIContent($"예산 초과! ({spentCurrency} > {totalObtained}) 모든 강화가 초기화됩니다."));
        }
        
        UpdateAvailableCurrency();
    }

    private void UpdateAvailableCurrency()
    {
        _availableCurrency.Clear();
        foreach (var reward in _totalRewards)
        {
            _availableCurrency[reward.Key] = reward.Value;
        }

        int spentCurrency = 0;
        foreach (var slot in _heroSlots)
        {
            if (slot.Npc != null)
            {
                spentCurrency += _upgradeSim.GetTotalCostForHero(slot.Npc._mid);
            }
        }
        
        if (_availableCurrency.ContainsKey(1))
        {
            _availableCurrency[1] -= spentCurrency;
        }
    }

    private void CalculateTotalRewardsUpTo(int targetStage)
    {
        _totalRewards.Clear();
        if (_chapterData == null || _monsterTable == null || _chapterListCache == null) return;

        for (int i = 1; i <= targetStage; i++)
        {
            SO_StageData stageData = null;
            foreach (var chapter in _chapterListCache)
            {
                stageData = chapter._stagedata.FirstOrDefault(s => s._stageid == i);
                if (stageData != null) break;
            }
            if (stageData == null) continue;

            foreach (var wave in stageData._monsterlist)
            {
                var monsterInfo = _monsterTable.GetMonsterInfo(wave._monsterid);
                if (monsterInfo._npc != null && monsterInfo._drop_itemid > 0)
                {
                    if (_totalRewards.ContainsKey(monsterInfo._drop_itemid))
                    {
                        _totalRewards[monsterInfo._drop_itemid] += monsterInfo._drop_itemvalue * wave._count;
                    }
                    else
                    {
                        _totalRewards.Add(monsterInfo._drop_itemid, monsterInfo._drop_itemvalue * wave._count);
                    }
                }
            }
        }
    }

    private void LoadAllTables()
    {
        _heroTable = AssetDatabase.FindAssets("t:SO_HeroTable").Select(guid => AssetDatabase.LoadAssetAtPath<SO_HeroTable>(AssetDatabase.GUIDToAssetPath(guid))).FirstOrDefault();
        _monsterTable = AssetDatabase.FindAssets("t:SO_MonsterTable").Select(guid => AssetDatabase.LoadAssetAtPath<SO_MonsterTable>(AssetDatabase.GUIDToAssetPath(guid))).FirstOrDefault();
        _itemTable = AssetDatabase.FindAssets("t:SO_Item_Table").Select(guid => AssetDatabase.LoadAssetAtPath<SO_Item_Table>(AssetDatabase.GUIDToAssetPath(guid))).FirstOrDefault();
        _statusTable = AssetDatabase.FindAssets("t:SO_Status_Table").Select(guid => AssetDatabase.LoadAssetAtPath<SO_Status_Table>(AssetDatabase.GUIDToAssetPath(guid))).FirstOrDefault();
        _chapterData = AssetDatabase.FindAssets("t:SO_ChapterData").Select(guid => AssetDatabase.LoadAssetAtPath<SO_ChapterData>(AssetDatabase.GUIDToAssetPath(guid))).FirstOrDefault();

        if (_chapterData != null)
        {
            var field = typeof(SO_ChapterData).GetField("_chapterdata", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                _chapterListCache = (List<St_ChapterData>)field.GetValue(_chapterData);
            }
        }
    }
}
