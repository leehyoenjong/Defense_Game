using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LevelDesignTool : EditorWindow
{
    private SO_ChapterData _chapterData;
    private SO_StageData _selectedStage;
    private Vector2 _scrollPosition;
    private float _totalCombatPower;
    private Dictionary<int, int> _totalRewards;
    private Dictionary<int, bool> _chapterFoldouts = new Dictionary<int, bool>();
    private Dictionary<int, float> _chapterCopyMultipliers = new Dictionary<int, float>();

    private SO_MonsterTable _monsterTable;
    private SO_Item_Table _itemTable;

    [MenuItem("Tools/Tool List/Level Design Tool")]
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
        if (GUILayout.Button("Home", GUILayout.Width(60)))
        {
            ToolHome.ShowWindow();
            this.Close();
        }

        GUILayout.Label("레벨 디자인 툴", EditorStyles.boldLabel);

        _chapterData = (SO_ChapterData)EditorGUILayout.ObjectField("챕터 데이터", _chapterData, typeof(SO_ChapterData), false);

        if (_chapterData == null)
        {
            EditorGUILayout.HelpBox("챕터(SO_ChapterData) 에셋을 선택해주세요.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Add New Chapter"))
        {
            AddChapter();
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

        for (int i = chapterList.Count - 1; i >= 0; i--)
        {
            var chapter = chapterList[i];
            if (!_chapterFoldouts.ContainsKey(chapter._chapterid))
            {
                _chapterFoldouts[chapter._chapterid] = false;
            }
            if (!_chapterCopyMultipliers.ContainsKey(chapter._chapterid))
            {
                _chapterCopyMultipliers[chapter._chapterid] = 1f;
            }

            EditorGUILayout.BeginHorizontal();
            _chapterFoldouts[chapter._chapterid] = EditorGUILayout.Foldout(_chapterFoldouts[chapter._chapterid], $"챕터 ID: {chapter._chapterid}", true, EditorStyles.foldoutHeader);

            if (GUILayout.Button("챕터 삭제", GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("챕터 삭제 확인", $"챕터 {chapter._chapterid}와 모든 하위 스테이지를 삭제하시겠습니까? 이 작업은 되돌릴 수 없습니다.", "삭제", "취소"))
                {
                    DeleteChapter(chapter);
                    continue;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_chapterFoldouts[chapter._chapterid])
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("스테이지 추가", GUILayout.Width(100)))
                {
                    AddStage(chapter);
                }

                if (GUILayout.Button("챕터 복사", GUILayout.Width(100)))
                {
                    CopyChapter(chapter, _chapterCopyMultipliers[chapter._chapterid]);
                }
                _chapterCopyMultipliers[chapter._chapterid] = EditorGUILayout.FloatField("배율", _chapterCopyMultipliers[chapter._chapterid]);

                EditorGUILayout.EndHorizontal();

                if (chapter._stagedata != null)
                {
                    for (int j = 0; j < chapter._stagedata.Count; j++)
                    {
                        var stage = chapter._stagedata[j];
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        if (GUILayout.Button($"스테이지 {j} (ID: {stage._stageid})"))
                        {
                            _selectedStage = stage;
                            _totalCombatPower = CombatPowerCalculator.Calculate(_selectedStage);
                            CalculateTotalRewards();
                        }
                        if (GUILayout.Button("삭제", GUILayout.Width(50)))
                        {
                            if (EditorUtility.DisplayDialog("스테이지 삭제 확인", $"스테이지 {j} (ID: {stage._stageid})를 삭제하시겠습니까?", "삭제", "취소"))
                            {
                                DeleteStage(chapter, stage);
                                continue;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
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

            string monsterName = "몬스터 선택";
            if (wave._monsterid > 0)
            {
                var monsterInfo = _monsterTable.GetMonsterInfo(wave._monsterid);
                if (monsterInfo._npc != null)
                {
                    monsterName = monsterInfo._npc.name;
                }
                else
                {
                    monsterName = $"ID Not Found: {wave._monsterid}";
                }
            }

            if (EditorGUILayout.DropdownButton(new GUIContent(monsterName), FocusType.Passive, GUILayout.Width(150)))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var monsterEntry in _monsterTable.GetMonsterList())
                {
                    int currentId = i;
                    menu.AddItem(new GUIContent(monsterEntry._npc.name), wave._monsterid == monsterEntry._npc._mid, () =>
                    {
                        var changedWave = _selectedStage._monsterlist[currentId];
                        changedWave._monsterid = monsterEntry._npc._mid;
                        _selectedStage._monsterlist[currentId] = changedWave;
                        GUI.changed = true;
                    });
                }
                menu.ShowAsContext();
            }

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

    private void AddChapter()
    {
        var chapterDataField = typeof(SO_ChapterData).GetField("_chapterdata", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var chapterList = chapterDataField.GetValue(_chapterData) as List<St_ChapterData>;

        St_ChapterData newChapter = new St_ChapterData
        {
            _chapterid = chapterList.Any() ? chapterList.Max(c => c._chapterid) + 1 : 1,
            _stagedata = new List<SO_StageData>()
        };

        chapterList.Add(newChapter);
        EditorUtility.SetDirty(_chapterData);
        ShowNotification(new GUIContent($"Chapter {newChapter._chapterid} has been added."));
    }

    private void AddStage(St_ChapterData chapter)
    {
        SO_StageData newStage = CreateInstance<SO_StageData>();

        if (chapter._stagedata == null)
        {
            chapter._stagedata = new List<SO_StageData>();
        }

        int localStageIndex = chapter._stagedata.Count;
        newStage._stageid = localStageIndex;
        newStage._monsterlist = new List<St_Stage>();

        string chapterFolderPath = $"Assets/03_SO/04_Chapter/Chapter_{chapter._chapterid}";
        if (!Directory.Exists(chapterFolderPath))
        {
            Directory.CreateDirectory(chapterFolderPath);
        }

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{chapterFolderPath}/SO_Stage_{localStageIndex}.asset");
        AssetDatabase.CreateAsset(newStage, assetPath);

        chapter._stagedata.Add(newStage);

        EditorUtility.SetDirty(_chapterData);
        EditorUtility.SetDirty(newStage);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ShowNotification(new GUIContent($"Stage {newStage._stageid} has been added to Chapter {chapter._chapterid}."));
    }
    private void CopyChapter(St_ChapterData sourceChapter, float multiplier)
    {
        var chapterDataField = typeof(SO_ChapterData).GetField("_chapterdata", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var chapterList = chapterDataField.GetValue(_chapterData) as List<St_ChapterData>;

        St_ChapterData newChapter = new St_ChapterData
        {
            _chapterid = chapterList.Any() ? chapterList.Max(c => c._chapterid) + 1 : 1,
            _stagedata = new List<SO_StageData>()
        };

        string newChapterFolderPath = $"Assets/03_SO/04_Chapter/Chapter_{newChapter._chapterid}";
        Directory.CreateDirectory(newChapterFolderPath);

        foreach (var sourceStage in sourceChapter._stagedata)
        {
            SO_StageData newStage = Instantiate(sourceStage);
            newStage._stageid = sourceStage._stageid;

            for (int i = 0; i < newStage._monsterlist.Count; i++)
            {
                var wave = newStage._monsterlist[i];
                wave._count = (int)(wave._count * multiplier);
                newStage._monsterlist[i] = wave;
            }

            string newAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{newChapterFolderPath}/SO_Stage_{newStage._stageid}.asset");
            AssetDatabase.CreateAsset(newStage, newAssetPath);
            newChapter._stagedata.Add(newStage);
            EditorUtility.SetDirty(newStage);
        }

        chapterList.Add(newChapter);
        EditorUtility.SetDirty(_chapterData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ShowNotification(new GUIContent($"Chapter {sourceChapter._chapterid} has been copied to Chapter {newChapter._chapterid} with a {multiplier}x multiplier."));
    }

    private void DeleteChapter(St_ChapterData chapter)
    {
        if (chapter._stagedata != null)
        {
            // Delete all stage assets within this chapter
            foreach (var stage in chapter._stagedata)
            {
                if (stage != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(stage);
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }

        // Delete the chapter folder
        string chapterFolderPath = $"Assets/03_SO/04_Chapter/Chapter_{chapter._chapterid}";
        if (Directory.Exists(chapterFolderPath))
        {
            FileUtil.DeleteFileOrDirectory(chapterFolderPath);
            FileUtil.DeleteFileOrDirectory(chapterFolderPath + ".meta");
            AssetDatabase.Refresh();
        }

        // Remove the chapter from the list in SO_ChapterData
        var chapterDataField = typeof(SO_ChapterData).GetField("_chapterdata", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var chapterList = chapterDataField.GetValue(_chapterData) as List<St_ChapterData>;
        chapterList.Remove(chapter);

        _chapterFoldouts.Remove(chapter._chapterid);
        _chapterCopyMultipliers.Remove(chapter._chapterid);


        EditorUtility.SetDirty(_chapterData);
        ShowNotification(new GUIContent($"Chapter {chapter._chapterid} has been deleted."));
    }

    private void DeleteStage(St_ChapterData chapter, SO_StageData stage)
    {
        if (_selectedStage == stage)
        {
            _selectedStage = null;
        }

        string assetPath = AssetDatabase.GetAssetPath(stage);
        AssetDatabase.DeleteAsset(assetPath);

        chapter._stagedata.Remove(stage);

        EditorUtility.SetDirty(_chapterData);
        ShowNotification(new GUIContent($"Stage {stage._stageid} has been deleted."));
    }
}
