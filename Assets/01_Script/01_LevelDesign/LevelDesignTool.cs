using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class LevelDesignTool : EditorWindow
{
    private SO_ChapterData _chapterData;
    private SO_StageData _selectedStage;
    private Vector2 _scrollPosition;
    private float _totalCombatPower;
    private Dictionary<int, int> _totalRewards;

    private SO_MonsterTable _monsterTable;
    private SO_Item_Table _itemTable;

    [MenuItem("Tools/Level Design Tool")]
    public static void ShowWindow()
    {
        GetWindow<LevelDesignTool>("Level Design Tool");
    }

    private void OnEnable()
    {
        // 에디터가 활성화될 때 테이블 에셋 로드
        string[] monsterTableGuid = AssetDatabase.FindAssets("t:SO_MonsterTable");
        if (monsterTableGuid.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(monsterTableGuid[0]);
            _monsterTable = AssetDatabase.LoadAssetAtPath<SO_MonsterTable>(path);
        }

        string[] itemTableGuid = AssetDatabase.FindAssets("t:SO_Item_Table");
        if (itemTableGuid.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(itemTableGuid[0]);
            _itemTable = AssetDatabase.LoadAssetAtPath<SO_Item_Table>(path);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("레벨 디자인 툴", EditorStyles.boldLabel);

        _chapterData = (SO_ChapterData)EditorGUILayout.ObjectField("챕터 데이터", _chapterData, typeof(SO_ChapterData), false);

        if (_chapterData == null)
        {
            EditorGUILayout.HelpBox("챕터(SO_ChapterData) 에셋을 선택해주세요.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();

        if (_selectedStage != null)
        {
            if (GUILayout.Button("Save Stage Data", GUILayout.Width(position.width / 2)))
            {
                AssetDatabase.SaveAssets();
                EditorUtility.ClearDirty(_selectedStage);
                ShowNotification(new GUIContent("스테이지 데이터가 저장되었습니다."));
            }
        }

        EditorGUILayout.Space();

        var chapterDataField = typeof(SO_ChapterData).GetField("_chapterdata", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (chapterDataField == null)
        {
            EditorGUILayout.HelpBox("_chapterdata 필드를 찾을 수 없습니다. SO_ChapterData 스크립트를 확인해주세요.", MessageType.Error);
            return;
        }

        var chapterList = chapterDataField.GetValue(_chapterData) as List<St_ChapterData>;
        if (chapterList == null) return;

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        foreach (var chapter in chapterList)
        {
            EditorGUILayout.LabelField($"챕터 ID: {chapter._chapterid}", EditorStyles.boldLabel);

            if (chapter._stagedata != null)
            {
                foreach (var stage in chapter._stagedata)
                {
                    if (GUILayout.Button($"스테이지 {stage._stageid}"))
                    {
                        _selectedStage = stage;
                        _totalCombatPower = CombatPowerCalculator.Calculate(_selectedStage);
                        CalculateTotalRewards();
                    }
                }
            }
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(20);

        if (_selectedStage != null)
        {
            DrawStageDetails();
        }
    }

    private void DrawStageDetails()
    {
        EditorGUILayout.LabelField($"선택된 스테이지: {_selectedStage._stageid}", EditorStyles.boldLabel);

        if (_totalRewards != null && _totalRewards.Count > 0)
        {
            EditorGUILayout.LabelField("클리어 총 보상", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            foreach (var reward in _totalRewards)
            {
                string itemName = _itemTable?.SearchItemData(reward.Key)._itemname ?? $"ItemID: {reward.Key}";
                EditorGUILayout.LabelField(itemName, reward.Value.ToString());
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        EditorGUILayout.LabelField("총 전투력:", $"{_totalCombatPower:N0}");
        EditorGUILayout.HelpBox("전투력 = (공격력 * (1 + 치명타확률 * 치명타피해량)) + (체력 * (1 + 방어력/100)) 의 총합", MessageType.None);
        EditorGUILayout.Space();

        int removeIndex = -1;
        for (int i = 0; i < _selectedStage._monsterlist.Count; i++)
        {
            var wave = _selectedStage._monsterlist[i];

            EditorGUILayout.BeginHorizontal();

            wave._monsterid = EditorGUILayout.IntField("몬스터 ID", wave._monsterid);
            wave._count = EditorGUILayout.IntField("수량", wave._count);
            wave._delaytime = EditorGUILayout.FloatField("딜레이", wave._delaytime);

            if (GUILayout.Button("상세정보", GUILayout.Width(70)))
            {
                if (wave._monsterid > 0) MonsterDatabaseEditor.ShowDetail(wave._monsterid);
            }

            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                removeIndex = i;
            }

            _selectedStage._monsterlist[i] = wave;

            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex != -1)
        {
            _selectedStage._monsterlist.RemoveAt(removeIndex);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("+ 몬스터 웨이브 추가"))
        {
            _selectedStage._monsterlist.Add(new St_Stage());
        }

        EditorGUILayout.Space();

        if (GUI.changed)
        {
            _totalCombatPower = CombatPowerCalculator.Calculate(_selectedStage);
            CalculateTotalRewards();
            EditorUtility.SetDirty(_selectedStage);
        }
        EditorGUILayout.Space();
    }

    private void CalculateTotalRewards()
    {
        _totalRewards = new Dictionary<int, int>();
        if (_selectedStage == null || _monsterTable == null) return;

        foreach (var wave in _selectedStage._monsterlist)
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
