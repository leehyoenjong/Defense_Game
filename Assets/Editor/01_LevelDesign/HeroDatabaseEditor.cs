using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public class HeroDatabaseEditor : EditorWindow
{
    private enum View { List, Detail }
    private View _currentView = View.List;

    // Common
    private SO_Status_Table _statusTable;

    // List View
    private SO_HeroTable _heroTable;
    private Vector2 _listScrollPosition;

    // Detail View
    private SO_NPC _selectedHero;
    private List<St_Status> _statusList;
    private Dictionary<int, float> _combatPowerPerGrade = new Dictionary<int, float>();
    private Vector2 _detailScrollPosition;
    private bool _showStatusFoldout = true;
    private bool _showSkillsFoldout = true;
    private bool _showBasicAttackFoldout = true;
    private bool _showUniqueSkillsFoldout = true;
    private List<bool> _uniqueSkillFoldouts = new List<bool>();

    [MenuItem("Tools/Level Design/Hero Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<HeroDatabaseEditor>("Hero Database");
    }

    private void OnEnable()
    {
        LoadTables();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Home", GUILayout.Width(60)))
        {
            LevelDesignHome.ShowWindow();
            this.Close();
        }

        if (_heroTable == null)
        {
            EditorGUILayout.HelpBox("SO_HeroTable을 찾을 수 없습니다. 프로젝트에 해당 에셋이 있는지 확인해주세요.", MessageType.Error);
            if (GUILayout.Button("테이블 다시 불러오기")) LoadTables();
            return;
        }

        switch (_currentView)
        {
            case View.List:
                DrawListView();
                break;
            case View.Detail:
                DrawDetailView();
                break;
        }
    }

    #region List View
    private void DrawListView()
    {
        EditorGUILayout.LabelField("영웅 목록", EditorStyles.boldLabel);

        _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);

        List<St_HeroTable> heroList = _heroTable.GetHeroList();
        int removeIndex = -1;

        for (int i = 0; i < heroList.Count; i++)
        {
            St_HeroTable heroEntry = heroList[i];
            SO_NPC hero = heroEntry._npc;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            if (hero != null)
            {
                GUILayout.Box(hero._icon != null ? hero._icon.texture : Texture2D.grayTexture, GUILayout.Width(60), GUILayout.Height(60));

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(hero.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"ID: {hero._mid} / Status ID: {hero._statusid}");
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("상세정보", GUILayout.Width(80), GUILayout.Height(60)))
                {
                    LoadHeroData(hero._mid);
                    _currentView = View.Detail;
                }
            }
            else
            {
                EditorGUILayout.LabelField("데이터 없음 (SO_NPC가 비어있습니다)");
            }

            if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(60)))
            {
                if (EditorUtility.DisplayDialog("영웅 삭제 확인", $"'{hero?.name ?? "이름 없음"}' 영웅을 목록에서 정말 삭제하시겠습니까?\n(연결된 SO_NPC 에셋은 삭제되지 않습니다.)", "삭제", "취소"))
                {
                    removeIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex != -1)
        {
            heroList.RemoveAt(removeIndex);
            EditorUtility.SetDirty(_heroTable);
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("신규 영웅 추가"))
        {
            CreateNewHero();
        }

        if (GUILayout.Button("Save Hero Table"))
        {
            EditorUtility.SetDirty(_heroTable);
            AssetDatabase.SaveAssets();
            ShowNotification(new GUIContent("영웅 테이블이 저장되었습니다."));
        }
    }

    private void CreateNewHero()
    {
        string path = "Assets/03_SO/00_Hero";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        SO_NPC newNpc = CreateInstance<SO_NPC>();

        int newId = 1;
        if (_heroTable.GetHeroList().Any())
        {
            newId = _heroTable.GetHeroList().Max(h => h._npc._mid) + 1;
        }

        newNpc._mid = newId;
        newNpc.name = $"New Hero {newId}";

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/SO_Hero_{newId}.asset");
        AssetDatabase.CreateAsset(newNpc, assetPath);

        St_HeroTable newEntry = new St_HeroTable { _npc = newNpc };
        _heroTable.GetHeroList().Add(newEntry);

        EditorUtility.SetDirty(_heroTable);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        LoadHeroData(newId);
        _currentView = View.Detail;
    }
    #endregion

    #region Detail View
    private void DrawDetailView()
    {
        if (_selectedHero == null)
        {
            EditorGUILayout.HelpBox("표시할 영웅이 선택되지 않았습니다.", MessageType.Warning);
            if (GUILayout.Button("목록으로 돌아가기")) _currentView = View.List;
            return;
        }

        if (GUILayout.Button("<< 목록으로 돌아가기"))
        {
            _currentView = View.List;
            return;
        }
        EditorGUILayout.Space();

        _detailScrollPosition = EditorGUILayout.BeginScrollView(_detailScrollPosition);

        // --- 기본 정보 ---
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        _selectedHero._icon = (Sprite)EditorGUILayout.ObjectField(_selectedHero._icon, typeof(Sprite), false, GUILayout.Width(100), GUILayout.Height(100));
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("영웅 ID:", _selectedHero._mid.ToString());
        _selectedHero.name = EditorGUILayout.TextField("에셋 이름", _selectedHero.name);
        _selectedHero._statusid = EditorGUILayout.IntField("스테이터스 ID", _selectedHero._statusid);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        _selectedHero._mybodyobject = (GameObject)EditorGUILayout.ObjectField("영웅 프리팹", _selectedHero._mybodyobject, typeof(GameObject), false);

        EditorGUILayout.Space(10);

        // --- 등급별 스탯 정보 ---
        _showStatusFoldout = EditorGUILayout.Foldout(_showStatusFoldout, "등급별 스탯 정보", true, EditorStyles.foldoutHeader);
        if (_showStatusFoldout && _statusList != null)
        {
            EditorGUI.indentLevel++;
            int removeIndex = -1;
            for (int i = 0; i < _statusList.Count; i++)
            {
                St_Status status = _statusList[i];
                float combatPower = _combatPowerPerGrade.ContainsKey(status._grade) ? _combatPowerPerGrade[status._grade] : 0;
                EditorGUILayout.LabelField($"등급 {status._grade}", $"전투력: {combatPower:N0}", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                status._hp = EditorGUILayout.IntField("HP", status._hp);
                status._damge = EditorGUILayout.IntField("Damage", status._damge);
                status._armor = EditorGUILayout.IntField("Armor", status._armor);
                status._critical = EditorGUILayout.FloatField("Critical", status._critical);
                status._critical_damage = EditorGUILayout.FloatField("Critical Damage", status._critical_damage);
                _statusList[i] = status;
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(88)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            if (removeIndex != -1) _statusList.RemoveAt(removeIndex);
            if (GUILayout.Button("등급 추가"))
            {
                int newGrade = _statusList.Count > 0 ? _statusList[_statusList.Count - 1]._grade + 1 : 1;
                _statusList.Add(new St_Status() { _grade = newGrade });
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // --- 스킬 정보 ---
        _showSkillsFoldout = EditorGUILayout.Foldout(_showSkillsFoldout, "스킬 정보", true, EditorStyles.foldoutHeader);
        if (_showSkillsFoldout)
        {
            EditorGUI.indentLevel++;
            _showBasicAttackFoldout = EditorGUILayout.Foldout(_showBasicAttackFoldout, "기본 공격 스킬", true);
            if (_showBasicAttackFoldout)
            {
                EditorGUI.indentLevel++;
                _selectedHero._basic_attack_skill = (BaseSkill)EditorGUILayout.ObjectField("스킬 에셋", _selectedHero._basic_attack_skill, typeof(BaseSkill), false);
                DrawSkillDetails(_selectedHero._basic_attack_skill);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            _showUniqueSkillsFoldout = EditorGUILayout.Foldout(_showUniqueSkillsFoldout, "고유 스킬 리스트", true);
            if (_showUniqueSkillsFoldout)
            {
                EditorGUI.indentLevel++;
                SerializedObject so = new SerializedObject(_selectedHero);
                SerializedProperty sp = so.FindProperty("_skill_chose_list");
                EditorGUILayout.PropertyField(sp, true);
                so.ApplyModifiedProperties();

                if (_selectedHero._skill_chose_list != null)
                {
                    while (_uniqueSkillFoldouts.Count < _selectedHero._skill_chose_list.Length)
                    {
                        _uniqueSkillFoldouts.Add(false);
                    }
                    while (_uniqueSkillFoldouts.Count > _selectedHero._skill_chose_list.Length)
                    {
                        _uniqueSkillFoldouts.RemoveAt(_uniqueSkillFoldouts.Count - 1);
                    }

                    for (int i = 0; i < _selectedHero._skill_chose_list.Length; i++)
                    {
                        BaseSkill skill = _selectedHero._skill_chose_list[i];
                        if (skill != null)
                        {
                            _uniqueSkillFoldouts[i] = EditorGUILayout.Foldout(_uniqueSkillFoldouts[i], $"고유 스킬 {i + 1}: {skill._skillInfo._name ?? "이름 없음"}", true);
                            if (_uniqueSkillFoldouts[i])
                            {
                                EditorGUI.indentLevel++;
                                DrawSkillDetails(skill);
                                EditorGUI.indentLevel--;
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"고유 스킬 {i + 1}: (비어있음)");
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            CalculateAllCombatPowers();
            SetAllDirty();
        }

        if (GUILayout.Button("Save All Changes"))
        {
            SaveChanges();
        }
    }

    private void LoadHeroData(int heroId)
    {
        if (_heroTable == null || _statusTable == null) LoadTables();

        var heroInfo = _heroTable.GetHeroList(heroId);
        if (heroInfo._npc != null)
        {
            _selectedHero = heroInfo._npc;
            if (_statusTable != null)
            {
                _statusList = _statusTable.GetStatusData(_selectedHero._statusid);
                if (_statusList == null)
                {
                    _statusList = new List<St_Status>();
                    _statusTable.UpdateStatusList(_selectedHero._statusid, _statusList);
                }
                CalculateAllCombatPowers();
            }
        }

        if (_selectedHero == null)
        {
            Debug.LogError($"{heroId} ID를 가진 영웅을 찾을 수 없습니다.");
            _currentView = View.List;
        }
    }

    private void SaveChanges()
    {
        SetAllDirty();
        if (_statusTable != null && _statusList != null)
        {
            _statusTable.UpdateStatusList(_selectedHero._statusid, _statusList);
        }
        EditorUtility.SetDirty(_heroTable);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ShowNotification(new GUIContent("모든 데이터가 저장되었습니다."));
    }

    private void SetAllDirty()
    {
        if (_selectedHero == null) return;
        EditorUtility.SetDirty(_selectedHero);
        if (_statusTable != null) EditorUtility.SetDirty(_statusTable);
        if (_heroTable != null) EditorUtility.SetDirty(_heroTable);
        if (_selectedHero._basic_attack_skill != null) EditorUtility.SetDirty(_selectedHero._basic_attack_skill);
        if (_selectedHero._skill_chose_list != null)
        {
            foreach (var skill in _selectedHero._skill_chose_list)
            {
                if (skill != null) EditorUtility.SetDirty(skill);
            }
        }
    }

    private void CalculateAllCombatPowers()
    {
        _combatPowerPerGrade.Clear();
        if (_statusList == null) return;

        foreach (var status in _statusList)
        {
            float attackPower = status._damge * (1 + status._critical * status._critical_damage);
            float defensePower = status._hp * (1 + status._armor / 100f);
            _combatPowerPerGrade[status._grade] = attackPower + defensePower;
        }
    }

    private void DrawSkillDetails(BaseSkill skill)
    {
        if (skill == null) return;

        SerializedObject skillSO = new SerializedObject(skill);

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();
        skill._skillInfo._skillicon = (Sprite)EditorGUILayout.ObjectField(skill._skillInfo._skillicon, typeof(Sprite), false, GUILayout.Width(100), GUILayout.Height(100));

        EditorGUILayout.BeginVertical();
        skill._skillInfo._name = EditorGUILayout.TextField("이름", skill._skillInfo._name);
        skill._skillInfo._level = EditorGUILayout.IntField("레벨", skill._skillInfo._level);
        skill._skillInfo._mid = EditorGUILayout.IntField("스킬 ID", skill._skillInfo._mid);
        skill._skillInfo._cooltime = EditorGUILayout.FloatField("쿨타임", skill._skillInfo._cooltime);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("설명");
        skill._skillInfo._explain = EditorGUILayout.TextArea(skill._skillInfo._explain, GUILayout.Height(35));

        EditorGUILayout.PropertyField(skillSO.FindProperty("_active_skillEffect"), new GUIContent("시전자 위치 이펙트"), true);
        EditorGUILayout.PropertyField(skillSO.FindProperty("_enter_hit_object"), new GUIContent("발사체/충돌 오브젝트"), true);

        if (skill is SO_Skill_Attack attackSkill)
        {
            EditorGUILayout.PropertyField(skillSO.FindProperty("_target_attackeffect"), new GUIContent("타겟 위치 이펙트"), true);

            FieldInfo field = typeof(SO_Skill_Attack).GetField("_skilldamagepercent", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                float currentValue = (float)field.GetValue(attackSkill);
                float newValue = EditorGUILayout.FloatField("데미지 계수(%)", currentValue);
                if (currentValue != newValue)
                {
                    field.SetValue(attackSkill, newValue);
                }

                if (_statusList != null && _statusList.Count > 0)
                {
                    St_Status baseStatus = _statusList[0];
                    float baseDamage = baseStatus._damge * (1 + baseStatus._critical * baseStatus._critical_damage);
                    float skillDamage = baseDamage * newValue;

                    if (skill._skillInfo._cooltime > 0)
                    {
                        float dps = skillDamage / skill._skillInfo._cooltime;
                        EditorGUILayout.LabelField("1초당 데미지 (DPS)", $"{dps:F2}");
                    }
                    else
                    {
                        EditorGUILayout.LabelField("1초당 데미지 (DPS)", "계산 불가 (쿨타임 0)");
                    }
                }
            }
        }

        skillSO.ApplyModifiedProperties();
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }
    #endregion

    private void LoadTables()
    {
        string[] heroTableGuid = AssetDatabase.FindAssets("t:SO_HeroTable");
        if (heroTableGuid.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(heroTableGuid[0]);
            _heroTable = AssetDatabase.LoadAssetAtPath<SO_HeroTable>(path);
        }
        else
        {
            Debug.LogError("SO_HeroTable 에셋을 찾을 수 없습니다.");
        }

        string[] statusTableGuid = AssetDatabase.FindAssets("t:SO_Status_Table");
        if (statusTableGuid.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(statusTableGuid[0]);
            _statusTable = AssetDatabase.LoadAssetAtPath<SO_Status_Table>(path);
        }
        else
        {
            Debug.LogError("SO_Status_Table 에셋을 찾을 수 없습니다.");
        }
    }
}
