using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GachaDatabaseEditor : EditorWindow
{
    private enum View { List, Detail }
    private View _currentView = View.List;

    private SO_Gacha_Table _gachaTable;
    private SO_Item_Table _itemTable;
    private Vector2 _scrollPosition;

    private int _selectedGachaIndex = -1;
    private St_GachaTable _editableGachaItem;
    private bool _isEditingNewItem = false;

    [MenuItem("Tools/Tool List/Gacha Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<GachaDatabaseEditor>("Gacha Database");
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

        if (_gachaTable == null || _itemTable == null)
        {
            EditorGUILayout.HelpBox("필요한 테이블(SO_Gacha_Table, SO_Item_Table)을 찾을 수 없습니다.", MessageType.Error);
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

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_gachaTable);
        }
    }

    private void LoadTables()
    {
        string[] gachaTableGuid = AssetDatabase.FindAssets("t:SO_Gacha_Table");
        if (gachaTableGuid.Length > 0)
        {
            _gachaTable = AssetDatabase.LoadAssetAtPath<SO_Gacha_Table>(AssetDatabase.GUIDToAssetPath(gachaTableGuid[0]));
        }

        string[] itemTableGuid = AssetDatabase.FindAssets("t:SO_Item_Table");
        if (itemTableGuid.Length > 0)
        {
            _itemTable = AssetDatabase.LoadAssetAtPath<SO_Item_Table>(AssetDatabase.GUIDToAssetPath(itemTableGuid[0]));
        }
    }

    private bool SaveTable()
    {
        if (_selectedGachaIndex != -1)
        {
            _gachaTable._gachatable[_selectedGachaIndex] = _editableGachaItem;
        }
        
        var duplicateGroups = _gachaTable._gachatable
            .GroupBy(item => item._gachaid)
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicateGroups.Any())
        {
            string errorMessage = "중복된 가챠 ID가 있습니다:\n";
            foreach (var group in duplicateGroups)
            {
                errorMessage += $"- ID {group.Key} \n";
            }
            EditorUtility.DisplayDialog("ID 중복 오류", errorMessage, "확인");
            return false;
        }

        EditorUtility.SetDirty(_gachaTable);
        AssetDatabase.SaveAssets();

        typeof(SO_Gacha_Table).GetField("_gachatable_dic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_gachaTable, new Dictionary<int, St_GachaTable>());

        ShowNotification(new GUIContent("가챠 테이블이 저장되었습니다."));
        return true;
    }

    #region List View
    private void DrawListView()
    {
        EditorGUILayout.LabelField("가챠 목록", EditorStyles.boldLabel);

        if (GUILayout.Button("신규 가챠 추가"))
        {
            AddNewGacha();
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        int removeIndex = -1;
        for (int i = 0; i < _gachaTable._gachatable.Count; i++)
        {
            St_GachaTable gachaItem = _gachaTable._gachatable[i];

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"Gacha ID: {gachaItem._gachaid}", GUILayout.Width(150));
            EditorGUILayout.LabelField($"보상 가짓수: {gachaItem._rewardlist.Count}");

            if (GUILayout.Button("상세정보", GUILayout.Width(80)))
            {
                _selectedGachaIndex = i;
                _editableGachaItem = gachaItem;
                _currentView = View.Detail;
            }

            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                if (EditorUtility.DisplayDialog("가챠 삭제 확인", $"Gacha ID '{gachaItem._gachaid}' 가챠를 목록에서 정말 삭제하시겠습니까?", "삭제", "취소"))
                {
                    removeIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex != -1)
        {
            _gachaTable._gachatable.RemoveAt(removeIndex);
        }

        EditorGUILayout.EndScrollView();
    }

    private void AddNewGacha()
    {
        St_GachaTable newItem = new St_GachaTable();
        int newId = 1;
        if (_gachaTable._gachatable.Any())
        {
            newId = _gachaTable._gachatable.Max(item => item._gachaid) + 1;
        }
        newItem._gachaid = newId;
        newItem._rewardlist = new List<St_GachaItemList>();

        _gachaTable._gachatable.Add(newItem);
        _selectedGachaIndex = _gachaTable._gachatable.Count - 1;
        _editableGachaItem = newItem;
        _currentView = View.Detail;
        _isEditingNewItem = true;
    }
    #endregion

    #region Detail View
    private void DrawDetailView()
    {
        if (_selectedGachaIndex == -1)
        {
            EditorGUILayout.HelpBox("표시할 가챠가 선택되지 않았습니다.", MessageType.Warning);
            if (GUILayout.Button("목록으로 돌아가기")) _currentView = View.List;
            return;
        }

        EditorGUILayout.LabelField($"가챠 상세정보: ID {_editableGachaItem._gachaid}", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _editableGachaItem._gachaid = EditorGUILayout.IntField("가챠 ID", _editableGachaItem._gachaid);
        _editableGachaItem._equaldistribution = EditorGUILayout.Toggle("균등 확률", _editableGachaItem._equaldistribution);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("보상 아이템 목록", EditorStyles.boldLabel);

        int removeIndex = -1;
        for (int i = 0; i < _editableGachaItem._rewardlist.Count; i++)
        {
            St_GachaItemList rewardItem = _editableGachaItem._rewardlist[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Item ID Dropdown
            int currentItemId = rewardItem._itemid;
            string currentItemName = _itemTable.SearchItemData(currentItemId)._itemname ?? (currentItemId == 0 ? "없음" : "ID 없음");

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.DropdownButton(new GUIContent(currentItemName), FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("없음"), currentItemId == 0, () =>
                {
                    var item = _editableGachaItem._rewardlist[i];
                    item._itemid = 0;
                    _editableGachaItem._rewardlist[i] = item;
                });

                foreach (var itemEntry in _itemTable._itemlist)
                {
                    menu.AddItem(new GUIContent($"{itemEntry._itemname} (ID: {itemEntry._itemid})"), itemEntry._itemid == currentItemId, () =>
                    {
                        var item = _editableGachaItem._rewardlist[i];
                        item._itemid = itemEntry._itemid;
                        _editableGachaItem._rewardlist[i] = item;
                    });
                }
                menu.ShowAsContext();
            }
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            rewardItem._itemvalue = EditorGUILayout.IntField("수량", rewardItem._itemvalue);
            if (!_editableGachaItem._equaldistribution)
            {
                rewardItem._percent = EditorGUILayout.FloatField("확률(%)", rewardItem._percent);
            }
            EditorGUILayout.EndHorizontal();

            _editableGachaItem._rewardlist[i] = rewardItem;

            EditorGUILayout.EndVertical();
        }

        if (removeIndex != -1)
        {
            _editableGachaItem._rewardlist.RemoveAt(removeIndex);
        }

        if (GUILayout.Button("+ 보상 아이템 추가"))
        {
            _editableGachaItem._rewardlist.Add(new St_GachaItemList());
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("적용하고 목록으로"))
        {
            if(SaveTable())
            {
                _currentView = View.List;
                _selectedGachaIndex = -1;
                _isEditingNewItem = false;
            }
        }
        if (GUILayout.Button("목록으로 돌아가기"))
        {
            if (_isEditingNewItem)
            {
                _gachaTable._gachatable.RemoveAt(_selectedGachaIndex);
            }
            _currentView = View.List;
            _selectedGachaIndex = -1;
            _isEditingNewItem = false;
        }
    }
    #endregion
}
