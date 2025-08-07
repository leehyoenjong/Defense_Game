using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UpgradeDatabaseEditor : EditorWindow
{
    private enum View { List, Detail }
    private View _currentView = View.List;

    private SO_Upgrade_Table _upgradeTable;
    private SO_Item_Table _itemTable;
    private Vector2 _scrollPosition;

    private int _selectedUpgradeIndex = -1;
    private St_UpgradeTable _editableUpgradeItem;
    private bool _isEditingNewItem = false;

    [MenuItem("Tools/Tool List/Upgrade Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<UpgradeDatabaseEditor>("Upgrade Database");
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

        if (_upgradeTable == null || _itemTable == null)
        {
            EditorGUILayout.HelpBox("필요한 테이블(SO_Upgrade_Table, SO_Item_Table)을 찾을 수 없습니다.", MessageType.Error);
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
            EditorUtility.SetDirty(_upgradeTable);
        }
    }

    private void LoadTables()
    {
        string[] upgradeTableGuid = AssetDatabase.FindAssets("t:SO_Upgrade_Table");
        if (upgradeTableGuid.Length > 0)
        {
            _upgradeTable = AssetDatabase.LoadAssetAtPath<SO_Upgrade_Table>(AssetDatabase.GUIDToAssetPath(upgradeTableGuid[0]));
        }

        string[] itemTableGuid = AssetDatabase.FindAssets("t:SO_Item_Table");
        if (itemTableGuid.Length > 0)
        {
            _itemTable = AssetDatabase.LoadAssetAtPath<SO_Item_Table>(AssetDatabase.GUIDToAssetPath(itemTableGuid[0]));
        }
    }

    private bool SaveTable()
    {
        if (_selectedUpgradeIndex != -1 && !_isEditingNewItem)
        {
            _upgradeTable._upgradetable[_selectedUpgradeIndex] = _editableUpgradeItem;
        }

        var duplicateGrades = _upgradeTable._upgradetable
            .GroupBy(item => item._grade)
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicateGrades.Any())
        {
            string errorMessage = "중복된 등급(Grade)이 있습니다:\n";
            foreach (var group in duplicateGrades)
            {
                errorMessage += $"- Grade {group.Key}\n";
            }
            EditorUtility.DisplayDialog("등급 중복 오류", errorMessage, "확인");
            return false;
        }
        
        // Sort by grade before saving
        _upgradeTable._upgradetable = _upgradeTable._upgradetable.OrderBy(item => item._grade).ToList();

        EditorUtility.SetDirty(_upgradeTable);
        AssetDatabase.SaveAssets();

        ShowNotification(new GUIContent("강화 테이블이 저장되었습니다."));
        return true;
    }

    #region List View
    private void DrawListView()
    {
        EditorGUILayout.LabelField("강화 단계 목록", EditorStyles.boldLabel);

        if (GUILayout.Button("신규 강화 단계 추가"))
        {
            AddNewUpgrade();
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        int removeIndex = -1;
        var sortedList = _upgradeTable._upgradetable.OrderBy(item => item._grade).ToList();
        
        for (int i = 0; i < sortedList.Count; i++)
        {
            St_UpgradeTable upgradeItem = sortedList[i];
            int originalIndex = _upgradeTable._upgradetable.IndexOf(upgradeItem);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"Grade: {upgradeItem._grade}", GUILayout.Width(100));
            
            string priceItemName = _itemTable.SearchItemData(upgradeItem._priceitemid)._itemname ?? "ID 없음";
            EditorGUILayout.LabelField($"필요 아이템: {priceItemName}");
            EditorGUILayout.LabelField($"필요 수량: {upgradeItem._price}");

            if (GUILayout.Button("수정", GUILayout.Width(80)))
            {
                _selectedUpgradeIndex = originalIndex;
                _editableUpgradeItem = upgradeItem;
                _currentView = View.Detail;
                _isEditingNewItem = false;
            }

            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                if (EditorUtility.DisplayDialog("강화 단계 삭제 확인", $"Grade '{upgradeItem._grade}' 단계를 목록에서 정말 삭제하시겠습니까?", "삭제", "취소"))
                {
                    removeIndex = originalIndex;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex != -1)
        {
            _upgradeTable._upgradetable.RemoveAt(removeIndex);
            SaveTable();
        }

        EditorGUILayout.EndScrollView();
    }

    private void AddNewUpgrade()
    {
        St_UpgradeTable newItem = new St_UpgradeTable();
        int newGrade = 1;
        if (_upgradeTable._upgradetable.Any())
        {
            newGrade = _upgradeTable._upgradetable.Max(item => item._grade) + 1;
        }
        newItem._grade = newGrade;

        _upgradeTable._upgradetable.Add(newItem);
        _selectedUpgradeIndex = _upgradeTable._upgradetable.Count - 1;
        _editableUpgradeItem = newItem;
        _currentView = View.Detail;
        _isEditingNewItem = true;
    }
    #endregion

    #region Detail View
    private void DrawDetailView()
    {
        if (_selectedUpgradeIndex == -1)
        {
            EditorGUILayout.HelpBox("표시할 강화 단계가 선택되지 않았습니다.", MessageType.Warning);
            if (GUILayout.Button("목록으로 돌아가기")) _currentView = View.List;
            return;
        }

        EditorGUILayout.LabelField($"강화 단계 상세정보: Grade {_editableUpgradeItem._grade}", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _editableUpgradeItem._grade = EditorGUILayout.IntField("등급 (Grade)", _editableUpgradeItem._grade);
        _editableUpgradeItem._price = EditorGUILayout.IntField("필요 수량", _editableUpgradeItem._price);
        
        // Price Item Dropdown
        string priceItemName = "아이템 선택";
        if (_editableUpgradeItem._priceitemid > 0)
        {
            priceItemName = _itemTable.SearchItemData(_editableUpgradeItem._priceitemid)._itemname ?? "ID 없음";
        }
        
        if (EditorGUILayout.DropdownButton(new GUIContent(priceItemName), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("없음"), _editableUpgradeItem._priceitemid == 0, () => { _editableUpgradeItem._priceitemid = 0; });
            
            foreach (var itemEntry in _itemTable._itemlist)
            {
                menu.AddItem(new GUIContent($"{itemEntry._itemname} (ID: {itemEntry._itemid})"), itemEntry._itemid == _editableUpgradeItem._priceitemid, () => 
                {
                    _editableUpgradeItem._priceitemid = itemEntry._itemid;
                });
            }
            menu.ShowAsContext();
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("적용하고 목록으로"))
        {
            if(SaveTable())
            {
                _currentView = View.List;
                _selectedUpgradeIndex = -1;
                _isEditingNewItem = false;
            }
        }
        if (GUILayout.Button("목록으로 돌아가기"))
        {
            if (_isEditingNewItem)
            {
                _upgradeTable._upgradetable.RemoveAt(_selectedUpgradeIndex);
            }
            _currentView = View.List;
            _selectedUpgradeIndex = -1;
            _isEditingNewItem = false;
        }
    }
    #endregion
}
