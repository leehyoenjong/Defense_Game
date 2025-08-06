using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CombatSimulator
{
    public class SimulationResult
    {
        public bool Success;
        public float ClearTime;
        public string FailureReason;
        public int ClearedStage;
        public float ProtectObjectHp;
        public int RemainingMonsters;
    }

    public class Simulator
    {
        private const float MAP_DISTANCE = 4f; // 맵의 총 길이 (가정)

        #region Simulation Entities
        private class SimCharacter
        {
            public string Name;
            public St_Status Stats;
            public float CurrentHP;
            public SO_NPC SourceNPC;
            public Dictionary<BaseSkill, float> SkillCooldowns = new Dictionary<BaseSkill, float>();
            public float TotalDamageDealt = 0;

            public SimCharacter(SO_NPC npc, St_Status initialStatus)
            {
                SourceNPC = npc;
                Name = npc.name;
                Stats = initialStatus;
                CurrentHP = initialStatus._hp;

                if (npc._basic_attack_skill != null)
                {
                    SkillCooldowns[npc._basic_attack_skill] = 0;
                }
                if (npc._skill_chose_list != null)
                {
                    foreach (var skill in npc._skill_chose_list)
                    {
                        if (skill != null) SkillCooldowns[skill] = 0;
                    }
                }
            }

            public virtual void Tick(float deltaTime)
            {
                var cooldownKeys = new List<BaseSkill>(SkillCooldowns.Keys);
                foreach (var skill in cooldownKeys)
                {
                    SkillCooldowns[skill] = Mathf.Max(0, SkillCooldowns[skill] - deltaTime);
                }
            }

            public bool IsReadyToAttack(BaseSkill skill) => SkillCooldowns.ContainsKey(skill) && SkillCooldowns[skill] <= 0;
            public void ResetCooldown(BaseSkill skill)
            {
                if (SkillCooldowns.ContainsKey(skill))
                {
                    SkillCooldowns[skill] = skill._skillInfo._cooltime;
                }
            }
        }

        private class SimHero : SimCharacter
        {
            public int Grade;
            public SimHero(SO_NPC heroData, St_Status status, int grade) : base(heroData, status)
            {
                Grade = grade;
            }
        }

        private class SimMonster : SimCharacter
        {
            public float DistanceToTarget;
            private readonly float _speed;

            public SimMonster(SO_NPC monsterData, St_Status status) : base(monsterData, status)
            {
                DistanceToTarget = MAP_DISTANCE;
                if (monsterData._mybodyobject != null)
                {
                    var moveController = monsterData._mybodyobject.GetComponent<MoveController>();
                    if (moveController != null)
                    {
                        _speed = moveController.GetSpeed();
                        Debug.Log($"[CombatSim] 스폰: '{monsterData.name}', Speed: {_speed}");
                    }
                    else
                    {
                        Debug.LogWarning($"[CombatSim] '{monsterData.name}' 프리팹에 MoveController가 없습니다. Speed가 0으로 설정됩니다.");
                        _speed = 0;
                    }
                }
                else
                {
                    Debug.LogWarning($"[CombatSim] '{monsterData.name}'에 Body Object 프리팹이 할당되지 않았습니다. Speed가 0으로 설정됩니다.");
                    _speed = 0;
                }
            }

            public override void Tick(float deltaTime)
            {
                base.Tick(deltaTime);
                if (DistanceToTarget > 0)
                {
                    float previousDistance = DistanceToTarget;
                    DistanceToTarget -= _speed * deltaTime;

                    if (DistanceToTarget <= 0 && previousDistance > 0)
                    {
                        Debug.Log($"[CombatSim] 도착: '{this.Name}' (이)가 보호 오브젝트에 도착했습니다.");
                    }
                }
            }
        }
        #endregion

        #region Simulation State & Data
        private float _currentTime;
        private int _totalGold;
        private List<SimHero> _heroes;
        private List<SimMonster> _monsters;
        private List<St_Stage> _spawnQueue;
        private SimCharacter _protectObject;

        private SO_HeroTable _heroTable;
        private SO_MonsterTable _monsterTable;
        private SO_Item_Table _itemTable;
        private SO_Status_Table _statusTable;
        private SO_ChapterData _chapterData;
        private StatusUpgradeManagerSim _upgradeSim;
        #endregion

        private static class EditorAssetLoader
        {
            public static T LoadTable<T>() where T : ScriptableObject
            {
                var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
                if (guids.Length == 0)
                {
                    Debug.LogError($"{typeof(T).Name} 에셋을 찾을 수 없습니다.");
                    return null;
                }
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
        }
        private void LoadAllTables()
        {
            _heroTable = EditorAssetLoader.LoadTable<SO_HeroTable>();
            _monsterTable = EditorAssetLoader.LoadTable<SO_MonsterTable>();
            _itemTable = EditorAssetLoader.LoadTable<SO_Item_Table>();
            _statusTable = EditorAssetLoader.LoadTable<SO_Status_Table>();
            _chapterData = EditorAssetLoader.LoadTable<SO_ChapterData>();
        }


        public SimulationResult RunSimulation(List<HeroPlacementEditor.HeroSimulationData> heroSlots, int targetStage)
        {
            LoadAllTables();
            Initialize(heroSlots);

            // Handle stage 0 case
            int startStage = 0; // Let's assume stages can start from 0

            for (int i = startStage; i <= targetStage; i++)
            {
                bool success = RunStage(i);
                if (!success)
                {
                    string reason = $"보호 오브젝트 파괴됨";
                    return new SimulationResult { Success = false, ClearedStage = i - 1, FailureReason = reason, ClearTime = _currentTime, ProtectObjectHp = _protectObject.CurrentHP, RemainingMonsters = _monsters.Count };
                }
                HandleUpgrades(i);
            }

            return new SimulationResult { Success = true, ClearedStage = targetStage, ClearTime = _currentTime, ProtectObjectHp = _protectObject.CurrentHP, RemainingMonsters = _monsters.Count };
        }

        private void Initialize(List<HeroPlacementEditor.HeroSimulationData> heroSlots)
        {
            _upgradeSim = new StatusUpgradeManagerSim();
            _totalGold = 0;
            _currentTime = 0;

            _heroes = new List<SimHero>();
            foreach (var slot in heroSlots)
            {
                if (slot.Npc != null)
                {
                    var status = slot.GetUpgradedStatus(_statusTable, _upgradeSim);
                    _heroes.Add(new SimHero(slot.Npc, status, slot.Grade));
                }
            }

            _monsters = new List<SimMonster>();
            _spawnQueue = new List<St_Stage>();

            var protectObjectSO = AssetDatabase.LoadAssetAtPath<SO_NPC>("Assets/03_SO/02_ProtectObject/SO_ProtectObject.asset");
            if (protectObjectSO != null)
            {
                var statusList = _statusTable.GetStatusData(protectObjectSO._statusid);
                if (statusList != null && statusList.Count > 0)
                {
                    _protectObject = new SimCharacter(protectObjectSO, statusList[0]);
                }
            }
        }
        private void HandleUpgrades(int clearedStageId)
        {
            // 1. 재화 획득
            var chapterListField = typeof(SO_ChapterData).GetField("_chapterdata", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var chapterList = (List<St_ChapterData>)chapterListField.GetValue(_chapterData);

            SO_StageData stageDataAsset = null;
            foreach (var chapter in chapterList)
            {
                if (chapter._stagedata != null)
                {
                    stageDataAsset = chapter._stagedata.FirstOrDefault(s => s != null && s._stageid == clearedStageId);
                    if (stageDataAsset != null) break;
                }
            }

            if (stageDataAsset != null && stageDataAsset._monsterlist != null)
            {
                foreach (var monsterSpawnInfo in stageDataAsset._monsterlist)
                {
                    var monsterInfo = _monsterTable.GetMonsterInfo(monsterSpawnInfo._monsterid);
                    if (monsterInfo._drop_itemid == 1) // 1 == Gold
                    {
                        _totalGold += monsterInfo._drop_itemvalue * monsterSpawnInfo._count;
                    }
                }
            }

            // 2. 스탯 강화 (가장 강한 영웅 공격력 몰빵)
            var strongestHero = _heroes.OrderByDescending(h => h.Stats._damge).FirstOrDefault();
            if (strongestHero != null)
            {
                int currentLevel = _upgradeSim.GetUpgradeLevel(strongestHero.SourceNPC._mid, ESTATUSUPGRADE.ATTACKPER);
                int cost = _upgradeSim.GetNextUpgradeCost(currentLevel);
                while (_totalGold >= cost)
                {
                    _totalGold -= cost;
                    _upgradeSim.ApplyUpgrade(strongestHero.SourceNPC._mid, ESTATUSUPGRADE.ATTACKPER);
                    currentLevel++;
                    cost = _upgradeSim.GetNextUpgradeCost(currentLevel);
                }

                var baseStatusList = _statusTable.GetStatusData(strongestHero.SourceNPC._statusid);
                var baseStatus = baseStatusList.Find(s => s._grade == strongestHero.Grade);
                if (baseStatus._grade == 0) baseStatus = baseStatusList.LastOrDefault(s => s._grade <= strongestHero.Grade);

                var totalUpgradeValue = _upgradeSim.GetTotalUpgradeValue(strongestHero.SourceNPC._mid, baseStatus);
                strongestHero.Stats._damge = baseStatus._damge + totalUpgradeValue._damge;
                strongestHero.Stats._critical = baseStatus._critical + totalUpgradeValue._critical;
                strongestHero.Stats._critical_damage = baseStatus._critical_damage + totalUpgradeValue._critical_damage;
            }

            // 3. 스킬 강화 (2 스테이지 마다, 등급 높은 영웅 우선)
            if (clearedStageId > 0 && clearedStageId % SkillChoseManager.CREATESKILLCHOSESTAGE == 0)
            {
                var heroToUpgrade = _heroes.OrderByDescending(h => h.Grade).FirstOrDefault();
                if (heroToUpgrade != null && heroToUpgrade.SourceNPC._basic_attack_skill != null)
                {
                    var currentSkill = heroToUpgrade.SourceNPC._basic_attack_skill;
                    var skillNamePart = currentSkill.name.Split(new[] { "_Lv" }, System.StringSplitOptions.None)[0];
                    var currentLevel = currentSkill._skillInfo._level;

                    if (currentLevel < BaseSkill.MAXLEVEL)
                    {
                        var nextLevelSkillName = $"{skillNamePart}_Lv{currentLevel + 1}";
                        var skillGuids = AssetDatabase.FindAssets($"t:BaseSkill {nextLevelSkillName}");
                        if (skillGuids.Any())
                        {
                            var path = AssetDatabase.GUIDToAssetPath(skillGuids[0]);
                            var nextSkill = AssetDatabase.LoadAssetAtPath<BaseSkill>(path);
                            heroToUpgrade.SourceNPC._basic_attack_skill = nextSkill;
                            Debug.Log($"{heroToUpgrade.Name}의 스킬이 {nextSkill.name}(으)로 강화되었습니다.");
                        }
                        else
                        {
                            Debug.LogWarning($"{nextLevelSkillName} 스킬 에셋을 찾지 못해 강화할 수 없습니다.");
                        }
                    }
                }
            }
        }


        private bool RunStage(int stageId)
        {
            var chapterListField = typeof(SO_ChapterData).GetField("_chapterdata", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var chapterList = (List<St_ChapterData>)chapterListField.GetValue(_chapterData);

            SO_StageData stageDataAsset = null;
            foreach (var chapter in chapterList)
            {
                if (chapter._stagedata != null)
                {
                    stageDataAsset = chapter._stagedata.FirstOrDefault(s => s != null && s._stageid == stageId);
                    if (stageDataAsset != null)
                    {
                        break;
                    }
                }
            }

            if (stageDataAsset == null)
            {
                Debug.LogError($"{stageId}에 해당하는 스테이지 데이터를 찾을 수 없습니다.");
                return false;
            }

            if (stageDataAsset._monsterlist == null || !stageDataAsset._monsterlist.Any())
            {
                Debug.LogWarning($"{stageId} 스테이지에 몬스터 데이터가 없습니다. (클리어로 간주)");
                return true;
            }

            _spawnQueue = new List<St_Stage>(stageDataAsset._monsterlist);
            _spawnQueue = _spawnQueue.OrderBy(s => s._delaytime).ToList();

            _monsters.Clear();
            float stageStartTime = _currentTime;
            int totalMonstersToSpawn = _spawnQueue.Sum(s => s._count);
            int defeatedMonsters = 0;

            while (true)
            {
                float deltaTime = 0.1f;
                _currentTime += deltaTime;

                while (_spawnQueue.Any() && (_currentTime - stageStartTime) >= _spawnQueue.First()._delaytime)
                {
                    var spawnInfo = _spawnQueue.First();
                    _spawnQueue.RemoveAt(0);

                    var monsterData = _monsterTable.GetMonsterInfo(spawnInfo._monsterid);
                    var statusList = _statusTable.GetStatusData(monsterData._npc._statusid);
                    for (int i = 0; i < spawnInfo._count; i++)
                    {
                        _monsters.Add(new SimMonster(monsterData._npc, statusList[0]));
                    }
                }

                _heroes.ForEach(h => h.Tick(deltaTime));
                _monsters.ForEach(m => m.Tick(deltaTime));

                DoHeroActions();
                DoMonsterActions();

                int defeatedThisTick = _monsters.RemoveAll(m => m.CurrentHP <= 0);
                defeatedMonsters += defeatedThisTick;

                if (_protectObject.CurrentHP <= 0)
                {
                    return false;
                }

                if (defeatedMonsters >= totalMonstersToSpawn && !_monsters.Any() && !_spawnQueue.Any())
                {
                    return true;
                }

                if ((_currentTime - stageStartTime) > 600) // 10분 타임아웃
                {
                    return false;
                }
            }
        }

        private void DoHeroActions()
        {
            foreach (var hero in _heroes)
            {
                foreach (var skill in hero.SkillCooldowns.Keys.ToList())
                {
                    if (hero.IsReadyToAttack(skill))
                    {
                        var targets = FindTargets(hero, skill, _monsters.Cast<SimCharacter>().ToList());
                        if (targets.Any())
                        {
                            ApplySkill(hero, skill, targets);
                            hero.ResetCooldown(skill);
                            break;
                        }
                    }
                }
            }
        }
        private void DoMonsterActions()
        {
            foreach (var monster in _monsters)
            {
                // 보호 오브젝트에 도달한 몬스터만 공격
                if (monster.DistanceToTarget > 0) continue;

                foreach (var skill in monster.SkillCooldowns.Keys.ToList())
                {
                    if (monster.IsReadyToAttack(skill))
                    {
                        var targets = new List<SimCharacter> { _protectObject };
                        ApplySkill(monster, skill, targets);
                        monster.ResetCooldown(skill);
                        break;
                    }
                }
            }
        }

        private List<SimCharacter> FindTargets(SimCharacter caster, BaseSkill skill, List<SimCharacter> potentialTargets)
        {
            var aliveTargets = potentialTargets.Where(t => t.CurrentHP > 0).ToList();
            if (!aliveTargets.Any()) return new List<SimCharacter>();

            List<SimCharacter> filteredList;
            switch (skill._etargetfiltertype)
            {
                case ETARGETFILTERTYPE.POS_NEAR_MONSTER:
                case ETARGETFILTERTYPE.POS_NEAR_HERO:
                    filteredList = aliveTargets.OrderBy(t => t.Name).Take(1).ToList();
                    break;
                case ETARGETFILTERTYPE.POS_FAR_MONSTER:
                case ETARGETFILTERTYPE.POS_FAR_HERO:
                    filteredList = aliveTargets.OrderByDescending(t => t.Name).Take(1).ToList();
                    break;
                case ETARGETFILTERTYPE.MOST_CURRENT_HP_HERO:
                case ETARGETFILTERTYPE.MOST_CURRENT_HP_MONSTER:
                    filteredList = aliveTargets.OrderByDescending(t => t.CurrentHP).Take(1).ToList();
                    break;
                case ETARGETFILTERTYPE.MOST_SMALL_CURRENT_HP_HERO:
                case ETARGETFILTERTYPE.MOST_SMALL_CURRENT_HP_MONSTER:
                    filteredList = aliveTargets.OrderBy(t => t.CurrentHP).Take(1).ToList();
                    break;
                case ETARGETFILTERTYPE.MOST_MAXHP_HERO:
                case ETARGETFILTERTYPE.MOST_MAXHP_MONSTER:
                    filteredList = aliveTargets.OrderByDescending(t => t.Stats._hp).Take(1).ToList();
                    break;
                case ETARGETFILTERTYPE.MOST_SMALL_MAXHP_HERO:
                case ETARGETFILTERTYPE.MOST_SMALL_MAXHP_MONSTER:
                    filteredList = aliveTargets.OrderBy(t => t.Stats._hp).Take(1).ToList();
                    break;
                case ETARGETFILTERTYPE.MOST_POWER_HERO:
                case ETARGETFILTERTYPE.MOST_POWER_MONSTER:
                    filteredList = aliveTargets.OrderByDescending(t => t.Stats._damge).Take(1).ToList();
                    break;
                default:
                    filteredList = aliveTargets;
                    break;
            }

            switch (skill._eskillarea)
            {
                case ESKILLAREA.ONE:
                    return filteredList.Take(1).ToList();
                case ESKILLAREA.ALL:
                    return filteredList;
                default:
                    return filteredList;
            }
        }

        private void ApplySkill(SimCharacter caster, BaseSkill skill, List<SimCharacter> targets)
        {
            if (skill is SO_Skill_Attack attackSkill)
            {
                int totalDamage = (int)(caster.Stats._damge * (1 + caster.Stats._critical * caster.Stats._critical_damage));
                int skillDamage = attackSkill.SkillDamage(totalDamage);

                foreach (var target in targets)
                {
                    float damageReduction = 1 - (target.Stats._armor / (target.Stats._armor + 100f));
                    int finalDamage = (int)(skillDamage * damageReduction);
                    target.CurrentHP -= finalDamage;
                    caster.TotalDamageDealt += finalDamage;
                }
            }
        }
    }
}
