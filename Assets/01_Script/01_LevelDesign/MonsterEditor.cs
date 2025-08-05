using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class MonsterEditor : EditorWindow
{
    private SO_NPC _monsterData;
    private SO_MonsterTable _monsterTable;
    private SO_Status_Table _statusTable;
    private List<St_Status> _statusList;
    private Dictionary<int, float> _combatPowerPerGrade = new Dictionary<int, float>();

    private Vector2 _scrollPosition;
    private bool _showStatusFoldout = true;
    private bool _showSkillsFoldout = true;

    public static void ShowWindow(int monsterId)
    {
        MonsterEditor window = GetWindow<MonsterEditor>("Monster Editor");
        window.LoadMonsterData(monsterId);
    }

    private void LoadMonsterData(int monsterId)
    {
        if (_monsterTable == null || _statusTable == null) LoadTables();
        
        if (_monsterTable != null)
        {
            var monsterInfo = _monsterTable.GetMonsterInfo(monsterId);
            if(monsterInfo._npc != null)
            {
                _monsterData = monsterInfo._npc;
                if (_statusTable != null)
                {
                    _statusList = _statusTable.GetStatusData(_monsterData._statusid);
                    CalculateAllCombatPowers();
                }
            }
        }
        
        if (_monsterData == null)
        {
            Debug.LogError($"{monsterId} ID를 가진 몬스터를 찾을 수 없습니다.");
            Close();
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
    }

    private void OnGUI()
    {
        if (_monsterData == null)
        {
            EditorGUILayout.HelpBox("몬스터 데이터를 로드 중이거나, 찾을 수 없습니다.", MessageType.Info);
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        // --- 기본 정보 ---
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        _monsterData._icon = (Sprite)EditorGUILayout.ObjectField(_monsterData._icon, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64));
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("몬스터 ID:", _monsterData._mid.ToString());
        _monsterData.name = EditorGUILayout.TextField("에셋 이름", _monsterData.name);
        _monsterData._statusid = EditorGUILayout.IntField("스테이터스 ID", _monsterData._statusid);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        _monsterData._mybodyobject = (GameObject)EditorGUILayout.ObjectField("몬스터 프리팹", _monsterData._mybodyobject, typeof(GameObject), false);
        
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

            if (removeIndex != -1)
            {
                _statusList.RemoveAt(removeIndex);
            }

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
            _monsterData._basic_attack_skill = (BaseSkill)EditorGUILayout.ObjectField("기본 공격 스킬", _monsterData._basic_attack_skill, typeof(BaseSkill), false);
            DrawSkillDetails("기본 공격 상세", _monsterData._basic_attack_skill);
            
            EditorGUILayout.Space();
            
            SerializedObject so = new SerializedObject(_monsterData);
            SerializedProperty sp = so.FindProperty("_skill_chose_list");
            EditorGUILayout.PropertyField(sp, new GUIContent("고유 스킬 리스트"), true);
            so.ApplyModifiedProperties();

            if (_monsterData._skill_chose_list != null)
            {
                for(int i = 0; i < _monsterData._skill_chose_list.Length; i++)
                {
                    DrawSkillDetails($"고유 스킬 {i+1} 상세", _monsterData._skill_chose_list[i]);
                }
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
    
    private void SetAllDirty()
    {
        EditorUtility.SetDirty(_monsterData);
        if (_statusTable != null) EditorUtility.SetDirty(_statusTable);
        if (_monsterData._basic_attack_skill != null) EditorUtility.SetDirty(_monsterData._basic_attack_skill);
        if (_monsterData._skill_chose_list != null)
        {
            foreach (var skill in _monsterData._skill_chose_list)
            {
                if (skill != null) EditorUtility.SetDirty(skill);
            }
        }
    }

    private void SaveChanges()
    {
        SetAllDirty();
        if (_statusTable != null && _statusList != null)
        {
            _statusTable.UpdateStatusList(_monsterData._statusid, _statusList);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ShowNotification(new GUIContent("모든 데이터가 저장되었습니다."));
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

    private void DrawSkillDetails(string label, BaseSkill skill)
    {
        if (skill == null) return;
        
        SerializedObject skillSO = new SerializedObject(skill);
        
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        skill._skillInfo._skillicon = (Sprite)EditorGUILayout.ObjectField(skill._skillInfo._skillicon, typeof(Sprite), false, GUILayout.Width(60), GUILayout.Height(60));
        
        EditorGUILayout.BeginVertical();
        skill._skillInfo._name = EditorGUILayout.TextField("이름", skill._skillInfo._name);
        skill._skillInfo._level = EditorGUILayout.IntField("레벨", skill._skillInfo._level);
        skill._skillInfo._mid = EditorGUILayout.IntField("스킬 ID", skill._skillInfo._mid);
        skill._skillInfo._cooltime = EditorGUILayout.FloatField("쿨타임", skill._skillInfo._cooltime);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.LabelField("설명");
        skill._skillInfo._explain = EditorGUILayout.TextArea(skill._skillInfo._explain, GUILayout.Height(35));
        
        // 이펙트 오브젝트 필드 추가
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

                // DPS 계산
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
}
