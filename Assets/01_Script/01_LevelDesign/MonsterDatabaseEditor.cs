using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class MonsterDatabaseEditor : EditorWindow
{
    private enum View { List, Detail }
    private View _currentView = View.List;

    // List View
    private SO_MonsterTable _monsterTable;
    private Vector2 _listScrollPosition;

    // Detail View
    private SO_NPC _selectedMonster;
    private List<St_Status> _statusList;
    private SO_Status_Table _statusTable;
    private SO_Item_Table _itemTable;
    private Dictionary<int, float> _combatPowerPerGrade = new Dictionary<int, float>();
    private Vector2 _detailScrollPosition;
    private bool _showDropItemFoldout = true;
    private bool _showStatusFoldout = true;
    private bool _showSkillsFoldout = true;
    private bool _showBasicAttackFoldout = true;
    private bool _showUniqueSkillsFoldout = true;
    
    [MenuItem("Tools/Monster Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<MonsterDatabaseEditor>("Monster Database");
    }

    public static void ShowDetail(int monsterId)
    {
        MonsterDatabaseEditor window = GetWindow<MonsterDatabaseEditor>("Monster Database");
        window.LoadMonsterData(monsterId);
        window._currentView = View.Detail;
    }
    
    private void OnEnable()
    {
        LoadTables();
    }

    private void OnGUI()
    {
        if (_monsterTable == null)
        {
            EditorGUILayout.HelpBox("SO_MonsterTable을 찾을 수 없습니다. 프로젝트에 해당 에셋이 있는지 확인해주세요.", MessageType.Error);
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
        EditorGUILayout.LabelField("몬스터 목록", EditorStyles.boldLabel);
        
        _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);

        List<St_MonsterTable> monsterList = _monsterTable.GetMonsterList();
        int removeIndex = -1;

        for (int i = 0; i < monsterList.Count; i++)
        {
            St_MonsterTable monsterEntry = monsterList[i];
            SO_NPC monster = monsterEntry._npc;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            if (monster != null)
            {
                GUILayout.Box(monster._icon != null ? monster._icon.texture : Texture2D.grayTexture, GUILayout.Width(60), GUILayout.Height(60));
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(monster.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"ID: {monster._mid} / Status ID: {monster._statusid}");
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("상세정보", GUILayout.Width(80), GUILayout.Height(60)))
                {
                    _selectedMonster = monster;
                    LoadMonsterData(monster._mid);
                    _currentView = View.Detail;
                }
            }
            else
            {
                EditorGUILayout.LabelField("데이터 없음 (SO_NPC가 비어있습니다)");
            }

            if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(60)))
            {
                if (EditorUtility.DisplayDialog("몬스터 삭제 확인", $"'{monster?.name ?? "이름 없음"}' 몬스터를 목록에서 정말 삭제하시겠습니까?\n(연결된 SO_NPC 에셋은 삭제되지 않습니다.)", "삭제", "취소"))
                {
                    removeIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex != -1)
        {
            monsterList.RemoveAt(removeIndex);
            EditorUtility.SetDirty(_monsterTable);
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("신규 몬스터 추가"))
        {
            CreateNewMonster();
        }

        if (GUILayout.Button("Save Monster Table"))
        {
            EditorUtility.SetDirty(_monsterTable);
            AssetDatabase.SaveAssets();
            ShowNotification(new GUIContent("몬스터 테이블이 저장되었습니다."));
        }
    }

    private void CreateNewMonster()
    {
        string path = "Assets/03_SO/01_Monster";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        SO_NPC newNpc = CreateInstance<SO_NPC>();
        int newId = _monsterTable.GetMonsterList().Count > 0 ? _monsterTable.GetMonsterList()[_monsterTable.GetMonsterList().Count - 1]._npc._mid + 1 : 1;
        newNpc._mid = newId;
        newNpc.name = $"New Monster {newId}";
        
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/SO_Monster_{newId}.asset");
        AssetDatabase.CreateAsset(newNpc, assetPath);

        St_MonsterTable newEntry = new St_MonsterTable { _npc = newNpc };
        _monsterTable.GetMonsterList().Add(newEntry);

        EditorUtility.SetDirty(_monsterTable);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _selectedMonster = newNpc;
        LoadMonsterData(newId);
        _currentView = View.Detail;
    }
    #endregion
    
    #region Detail View
    private void DrawDetailView()
    {
        if (_selectedMonster == null)
        {
            EditorGUILayout.HelpBox("표시할 몬스터가 선택되지 않았습니다.", MessageType.Warning);
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
        _selectedMonster._icon = (Sprite)EditorGUILayout.ObjectField(_selectedMonster._icon, typeof(Sprite), false, GUILayout.Width(100), GUILayout.Height(100));
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("몬스터 ID:", _selectedMonster._mid.ToString());
        _selectedMonster.name = EditorGUILayout.TextField("에셋 이름", _selectedMonster.name);
        _selectedMonster._statusid = EditorGUILayout.IntField("스테이터스 ID", _selectedMonster._statusid);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        _selectedMonster._mybodyobject = (GameObject)EditorGUILayout.ObjectField("몬스터 프리팹", _selectedMonster._mybodyobject, typeof(GameObject), false);
        
        EditorGUILayout.Space(10);
        
        // --- 드랍 아이템 정보 (Foldout) ---
        _showDropItemFoldout = EditorGUILayout.Foldout(_showDropItemFoldout, "드랍 아이템 정보", true, EditorStyles.foldoutHeader);
        if (_showDropItemFoldout)
        {
            EditorGUI.indentLevel++;
            var monsterList = _monsterTable.GetMonsterList();
            int monsterIndex = monsterList.FindIndex(m => m._npc == _selectedMonster);
            if (monsterIndex != -1)
            {
                St_MonsterTable monsterEntry = monsterList[monsterIndex];

                monsterEntry._drop_itemid = EditorGUILayout.IntField("드랍 아이템 ID", monsterEntry._drop_itemid);
                if (_itemTable != null && monsterEntry._drop_itemid > 0)
                {
                    string itemName = _itemTable.SearchItemData(monsterEntry._drop_itemid)._itemname;
                    if (!string.IsNullOrEmpty(itemName))
                    {
                        EditorGUILayout.LabelField(" ", $"아이템 이름: {itemName}");
                    }
                }
                monsterEntry._drop_itemvalue = EditorGUILayout.IntField("드랍 아이템 수량", monsterEntry._drop_itemvalue);
                
                monsterList[monsterIndex] = monsterEntry;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // --- 등급별 스탯 정보 (Foldout) ---
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

        // --- 스킬 정보 (Foldout) ---
        _showSkillsFoldout = EditorGUILayout.Foldout(_showSkillsFoldout, "스킬 정보", true, EditorStyles.foldoutHeader);
        if (_showSkillsFoldout)
        {
            EditorGUI.indentLevel++;
            _showBasicAttackFoldout = EditorGUILayout.Foldout(_showBasicAttackFoldout, "기본 공격 스킬", true);
            if (_showBasicAttackFoldout)
            {
                EditorGUI.indentLevel++;
                _selectedMonster._basic_attack_skill = (BaseSkill)EditorGUILayout.ObjectField("스킬 에셋", _selectedMonster._basic_attack_skill, typeof(BaseSkill), false);
                DrawSkillDetails(_selectedMonster._basic_attack_skill);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            _showUniqueSkillsFoldout = EditorGUILayout.Foldout(_showUniqueSkillsFoldout, "고유 스킬 리스트", true);
            if (_showUniqueSkillsFoldout)
            {
                EditorGUI.indentLevel++;
                SerializedObject so = new SerializedObject(_selectedMonster);
                SerializedProperty sp = so.FindProperty("_skill_chose_list");
                EditorGUILayout.PropertyField(sp, true);
                so.ApplyModifiedProperties();

                if (_selectedMonster._skill_chose_list != null)
                {
                    for(int i = 0; i < _selectedMonster._skill_chose_list.Length; i++)
                    {
                        DrawSkillDetails(_selectedMonster._skill_chose_list[i], $"고유 스킬 {i+1} 상세");
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

    private void LoadMonsterData(int monsterId)
    {
        if (_monsterTable == null || _statusTable == null) LoadTables();
        
        var monsterInfo = _monsterTable.GetMonsterInfo(monsterId);
        if(monsterInfo._npc != null)
        {
            _selectedMonster = monsterInfo._npc;
            if (_statusTable != null)
            {
                _statusList = _statusTable.GetStatusData(_selectedMonster._statusid);
                CalculateAllCombatPowers();
            }
        }
        
        if (_selectedMonster == null)
        {
            Debug.LogError($"{monsterId} ID를 가진 몬스터를 찾을 수 없습니다.");
            _currentView = View.List;
        }
    }

    private void LoadTables()
    {
        string[] monsterTableGuid = AssetDatabase.FindAssets("t:SO_MonsterTable");
        if (monsterTableGuid.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(monsterTableGuid[0]);
            _monsterTable = AssetDatabase.LoadAssetAtPath<SO_MonsterTable>(path);
        }

        string[] statusTableGuid = AssetDatabase.FindAssets("t:SO_Status_Table");
        if (statusTableGuid.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(statusTableGuid[0]);
            _statusTable = AssetDatabase.LoadAssetAtPath<SO_Status_Table>(path);
        }
        
        string[] itemTableGuid = AssetDatabase.FindAssets("t:SO_Item_Table");
        if (itemTableGuid.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(itemTableGuid[0]);
            _itemTable = AssetDatabase.LoadAssetAtPath<SO_Item_Table>(path);
        }
    }
    
    private void SaveChanges()
    {
        SetAllDirty();
        if (_statusTable != null && _statusList != null)
        {
            _statusTable.UpdateStatusList(_selectedMonster._statusid, _statusList);
        }
        EditorUtility.SetDirty(_monsterTable);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ShowNotification(new GUIContent("모든 데이터가 저장되었습니다."));
    }

    private void SetAllDirty()
    {
        if (_selectedMonster == null) return;
        EditorUtility.SetDirty(_selectedMonster);
        if (_statusTable != null) EditorUtility.SetDirty(_statusTable);
        if (_monsterTable != null) EditorUtility.SetDirty(_monsterTable);
        if (_selectedMonster._basic_attack_skill != null) EditorUtility.SetDirty(_selectedMonster._basic_attack_skill);
        if (_selectedMonster._skill_chose_list != null)
        {
            foreach (var skill in _selectedMonster._skill_chose_list)
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

    private void DrawSkillDetails(BaseSkill skill, string label = "")
    {
        if (skill == null) return;
        
        if (!string.IsNullOrEmpty(label))
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
        }
        
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

        if(skill is SO_Skill_Attack attackSkill)
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
}
