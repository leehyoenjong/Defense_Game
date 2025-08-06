using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestDatabaseEditor : EditorWindow
{
    private SO_QuestTable _questTable;
    private SO_Item_Table _itemTable;
    private Vector2 _scrollPosition;

    // Foldouts for each quest list
    private bool _repeatQuestsFoldout = true;
    private bool _dayQuestsFoldout = true;
    private bool _weekQuestsFoldout = true;
    private bool _achievementsQuestsFoldout = true;

    // For editing a specific quest
    private St_QuestTable _editableQuest;
    private EQUESTTYPE _originalQuestType;
    private int _selectedQuestIndex = -1;
    private bool _isEditingNewQuest = false;

    [MenuItem("Tools/Tool List/Quest Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<QuestDatabaseEditor>("Quest Database");
    }

    private void OnEnable()
    {
        LoadTables();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Home", GUILayout.Width(60)))
        {
            ToolHome.ShowWindow();
            this.Close();
        }

        if (_questTable == null || _itemTable == null)
        {
            EditorGUILayout.HelpBox("필요한 테이블(SO_QuestTable, SO_Item_Table)을 찾을 수 없습니다.", MessageType.Error);
            if (GUILayout.Button("테이블 다시 불러오기")) LoadTables();
            return;
        }
        
        if (GUI.changed)
        {
            if(_questTable != null) EditorUtility.SetDirty(_questTable);
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        if (_selectedQuestIndex != -1)
        {
            DrawDetailView();
        }
        else
        {
            DrawListView();
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void LoadTables()
    {
        string[] questTableGuid = AssetDatabase.FindAssets("t:SO_QuestTable");
        if (questTableGuid.Length > 0)
        {
            _questTable = AssetDatabase.LoadAssetAtPath<SO_QuestTable>(AssetDatabase.GUIDToAssetPath(questTableGuid[0]));
        }

        string[] itemTableGuid = AssetDatabase.FindAssets("t:SO_Item_Table");
        if (itemTableGuid.Length > 0)
        {
            _itemTable = AssetDatabase.LoadAssetAtPath<SO_Item_Table>(AssetDatabase.GUIDToAssetPath(itemTableGuid[0]));
        }
    }

    private bool SaveTable()
    {
        if (_selectedQuestIndex != -1)
        {
            _questTable.GetQuestTypeList(_originalQuestType)[_selectedQuestIndex] = _editableQuest;
        }

        var allQuests = _questTable._repeatquestlist
            .Concat(_questTable._dayquestlist)
            .Concat(_questTable._weekquestlist)
            .Concat(_questTable._achievementsquestlist)
            .ToList();

        var duplicateGroups = allQuests
            .GroupBy(q => q._mid)
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicateGroups.Any())
        {
            string errorMessage = "중복된 퀘스트 ID가 있습니다:\n";
            foreach (var group in duplicateGroups)
            {
                string questTitles = string.Join(", ", group.Select(q => $"'{q._title}'"));
                errorMessage += $"- ID {group.Key}: {questTitles}\n";
            }
            EditorUtility.DisplayDialog("ID 중복 오류", errorMessage, "확인");
            
            // Revert the change if it was a new item causing the issue
            // This is a bit complex as we don't know the "original" state easily after list manipulation
            // A simple approach is to advise the user to fix it manually.
            return false;
        }
        
        EditorUtility.SetDirty(_questTable);
        AssetDatabase.SaveAssets();
        
        typeof(SO_QuestTable).GetField("_alllist", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_questTable, new List<St_QuestTable>());

        ShowNotification(new GUIContent("퀘스트 테이블이 저장되었습니다."));
        return true;
    }

    #region List View
    private void DrawListView()
    {
        if (GUILayout.Button("테이블 저장"))
        {
            SaveTable();
        }
        
        _repeatQuestsFoldout = EditorGUILayout.Foldout(_repeatQuestsFoldout, "반복 퀘스트", true, EditorStyles.foldoutHeader);
        if (_repeatQuestsFoldout) DrawQuestList(_questTable._repeatquestlist, EQUESTTYPE.REPEAT);

        _dayQuestsFoldout = EditorGUILayout.Foldout(_dayQuestsFoldout, "일일 퀘스트", true, EditorStyles.foldoutHeader);
        if (_dayQuestsFoldout) DrawQuestList(_questTable._dayquestlist, EQUESTTYPE.DAY);

        _weekQuestsFoldout = EditorGUILayout.Foldout(_weekQuestsFoldout, "주간 퀘스트", true, EditorStyles.foldoutHeader);
        if (_weekQuestsFoldout) DrawQuestList(_questTable._weekquestlist, EQUESTTYPE.WEEK);

        _achievementsQuestsFoldout = EditorGUILayout.Foldout(_achievementsQuestsFoldout, "업적 퀘스트", true, EditorStyles.foldoutHeader);
        if (_achievementsQuestsFoldout) DrawQuestList(_questTable._achievementsquestlist, EQUESTTYPE.ACHIEVEMENTS);
    }

    private void DrawQuestList(List<St_QuestTable> questList, EQUESTTYPE type)
    {
        int removeIndex = -1;
        for (int i = 0; i < questList.Count; i++)
        {
            St_QuestTable quest = questList[i];
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"ID:{quest._mid}", GUILayout.Width(70));
            EditorGUILayout.LabelField(quest._title);
            if (GUILayout.Button("수정", GUILayout.Width(50)))
            {
                _selectedQuestIndex = i;
                _originalQuestType = type;
                _editableQuest = quest;
            }
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                if (EditorUtility.DisplayDialog("퀘스트 삭제 확인", $"'{quest._title}' 퀘스트를 목록에서 정말 삭제하시겠습니까?", "삭제", "취소"))
                {
                    removeIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        if (removeIndex != -1)
        {
            questList.RemoveAt(removeIndex);
        }

        if (GUILayout.Button("+ " + type.ToString() + " 퀘스트 추가"))
        {
            AddNewQuest(questList, type);
        }
        EditorGUILayout.Space();
    }

    private void AddNewQuest(List<St_QuestTable> questList, EQUESTTYPE type)
    {
        St_QuestTable newQuest = new St_QuestTable();
        newQuest._questtype = type;
        
        var allQuests = _questTable._repeatquestlist.Concat(_questTable._dayquestlist).Concat(_questTable._weekquestlist).Concat(_questTable._achievementsquestlist);
        int newId = allQuests.Any() ? allQuests.Max(q => q._mid) + 1 : 1;
        
        newQuest._mid = newId;
        newQuest._title = $"New {type} Quest";
        
        questList.Add(newQuest);
        _selectedQuestIndex = questList.Count - 1;
        _originalQuestType = type;
        _editableQuest = newQuest;
        _isEditingNewQuest = true;
    }
    #endregion

    #region Detail View
    private void DrawDetailView()
    {
        EditorGUILayout.LabelField($"퀘스트 상세정보: {_editableQuest._title}", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // --- 기본 정보 ---
        _editableQuest._mid = EditorGUILayout.IntField("퀘스트 ID", _editableQuest._mid);
        _editableQuest._title = EditorGUILayout.TextField("제목", _editableQuest._title);
        _editableQuest._explain = EditorGUILayout.TextField("설명", _editableQuest._explain);
        _editableQuest._isclearactiveoff = EditorGUILayout.Toggle("완료 시 비활성화", _editableQuest._isclearactiveoff);

        // --- 오픈 조건 ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("오픈 조건", EditorStyles.boldLabel);
        _editableQuest._questopentype = (EQUESTVALUETYPE)EditorGUILayout.EnumPopup("타입", _editableQuest._questopentype);
        _editableQuest._questopentarget = EditorGUILayout.IntField("타겟 ID", _editableQuest._questopentarget);
        _editableQuest._questopenvalue = EditorGUILayout.IntField("값", _editableQuest._questopenvalue);
        
        // --- 클리어 조건 ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("클리어 조건", EditorStyles.boldLabel);
        _editableQuest._questcleartype = (EQUESTVALUETYPE)EditorGUILayout.EnumPopup("타입", _editableQuest._questcleartype);
        _editableQuest._questcleartarget = EditorGUILayout.IntField("타겟 ID", _editableQuest._questcleartarget);
        _editableQuest._questclearvalue = EditorGUILayout.IntField("값", _editableQuest._questclearvalue);

        // --- 보상 ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("보상", EditorStyles.boldLabel);
        
        string rewardItemName = _itemTable.SearchItemData(_editableQuest._rewarditemid)._itemname ?? "아이템 ID 없음";
        if (EditorGUILayout.DropdownButton(new GUIContent(rewardItemName), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();
            foreach (var itemEntry in _itemTable._itemlist)
            {
                menu.AddItem(new GUIContent($"{itemEntry._itemname} (ID: {itemEntry._itemid})"), itemEntry._itemid == _editableQuest._rewarditemid, () => 
                {
                    _editableQuest._rewarditemid = itemEntry._itemid;
                });
            }
            menu.ShowAsContext();
        }
        _editableQuest._rewarditemvalue = EditorGUILayout.IntField("보상 수량", _editableQuest._rewarditemvalue);

        // --- Buttons ---
        EditorGUILayout.Space(20);
        if (GUILayout.Button("적용하고 목록으로"))
        {
            if (SaveTable())
            {
                _selectedQuestIndex = -1;
                _isEditingNewQuest = false;
            }
        }
        if (GUILayout.Button("취소"))
        {
            if (_isEditingNewQuest)
            {
                _questTable.GetQuestTypeList(_originalQuestType).RemoveAt(_selectedQuestIndex);
            }
            _selectedQuestIndex = -1;
            _isEditingNewQuest = false;
        }
    }
    #endregion
}
