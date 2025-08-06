using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemDatabaseEditor : EditorWindow
{
    private enum View { List, Detail }
    private View _currentView = View.List;

    private SO_Item_Table _itemTable;
    private Vector2 _scrollPosition;
    
    private int _selectedItemIndex = -1;
    private St_ItemTable _editableItem;
    private bool _isEditingNewItem = false;

    [MenuItem("Tools/Tool List/Item Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<ItemDatabaseEditor>("Item Database");
    }

    private void OnEnable()
    {
        LoadTable();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Home", GUILayout.Width(60)))
        {
            ToolHome.ShowWindow();
            this.Close();
        }

        if (_itemTable == null)
        {
            EditorGUILayout.HelpBox("SO_Item_Table을 찾을 수 없습니다. 프로젝트에 해당 에셋이 있는지 확인해주세요.", MessageType.Error);
            if (GUILayout.Button("테이블 다시 불러오기")) LoadTable();
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
            EditorUtility.SetDirty(_itemTable);
        }
    }

    private void LoadTable()
    {
        string[] tableGuid = AssetDatabase.FindAssets("t:SO_Item_Table");
        if (tableGuid.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(tableGuid[0]);
            _itemTable = AssetDatabase.LoadAssetAtPath<SO_Item_Table>(path);
        }
        else
        {
            Debug.LogError("SO_Item_Table 에셋을 찾을 수 없습니다.");
        }
    }

    private bool SaveTable()
    {
        if (_selectedItemIndex != -1)
        {
            _itemTable._itemlist[_selectedItemIndex] = _editableItem;
        }

        var duplicateGroups = _itemTable._itemlist
            .GroupBy(item => item._itemid)
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicateGroups.Any())
        {
            string errorMessage = "중복된 아이템 ID가 있습니다:\n";
            foreach (var group in duplicateGroups)
            {
                string itemNames = string.Join(", ", group.Select(item => $"'{item._itemname}'"));
                errorMessage += $"- ID {group.Key}: {itemNames}\n";
            }
            EditorUtility.DisplayDialog("ID 중복 오류", errorMessage, "확인");
            return false;
        }
        
        EditorUtility.SetDirty(_itemTable);
        AssetDatabase.SaveAssets();
        
        typeof(SO_Item_Table).GetField("_itemlist_dic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_itemTable, new Dictionary<int, St_ItemTable>());
        
        ShowNotification(new GUIContent("아이템 테이블이 저장되었습니다."));
        return true;
    }

    #region List View
    private void DrawListView()
    {
        EditorGUILayout.LabelField("아이템 목록", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("신규 아이템 추가"))
        {
            AddNewItem();
        }
        if (GUILayout.Button("테이블 저장"))
        {
            SaveTable();
        }
        EditorGUILayout.EndHorizontal();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        int removeIndex = -1;
        for (int i = 0; i < _itemTable._itemlist.Count; i++)
        {
            St_ItemTable item = _itemTable._itemlist[i];
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            GUILayout.Box(item._itemicon != null ? item._itemicon.texture : Texture2D.grayTexture, GUILayout.Width(60), GUILayout.Height(60));
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(item._itemname, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"ID: {item._itemid} | 종류: {item._itemkind}");
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("상세정보", GUILayout.Width(80), GUILayout.Height(60)))
            {
                _selectedItemIndex = i;
                _editableItem = item; // Create a copy for editing
                _currentView = View.Detail;
            }

            if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(60)))
            {
                if (EditorUtility.DisplayDialog("아이템 삭제 확인", $"'{item._itemname}' 아이템을 목록에서 정말 삭제하시겠습니까?", "삭제", "취소"))
                {
                    removeIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex != -1)
        {
            _itemTable._itemlist.RemoveAt(removeIndex);
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void AddNewItem()
    {
        St_ItemTable newItem = new St_ItemTable();
        int newId = 1;
        if (_itemTable._itemlist.Any())
        {
            newId = _itemTable._itemlist.Max(item => item._itemid) + 1;
        }
        newItem._itemid = newId;
        newItem._itemname = $"New Item {newId}";
        
        _itemTable._itemlist.Add(newItem);

        _selectedItemIndex = _itemTable._itemlist.Count - 1;
        _editableItem = newItem;
        _currentView = View.Detail;
        _isEditingNewItem = true;
    }
    #endregion
    
    #region Detail View
    private void DrawDetailView()
    {
        if (_selectedItemIndex == -1)
        {
            EditorGUILayout.HelpBox("표시할 아이템이 선택되지 않았습니다.", MessageType.Warning);
            if (GUILayout.Button("목록으로 돌아가기")) _currentView = View.List;
            return;
        }
        
        EditorGUILayout.LabelField($"아이템 상세정보: {_editableItem._itemname}", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("아이콘", GUILayout.Width(EditorGUIUtility.labelWidth));
        _editableItem._itemicon = (Sprite)EditorGUILayout.ObjectField(_editableItem._itemicon, typeof(Sprite), false, GUILayout.Width(100), GUILayout.Height(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        
        _editableItem._itemid = EditorGUILayout.IntField("아이템 ID", _editableItem._itemid);
        _editableItem._itemname = EditorGUILayout.TextField("이름", _editableItem._itemname);
        
        EditorGUILayout.LabelField("설명");
        _editableItem._itemexplain = EditorGUILayout.TextArea(_editableItem._itemexplain, GUILayout.Height(40));

        _editableItem._itemkind = (EITEMKIND)EditorGUILayout.EnumPopup("아이템 종류", _editableItem._itemkind);
        _editableItem._connecttableid = EditorGUILayout.IntField("연결 테이블 ID", _editableItem._connecttableid);

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("적용하고 목록으로"))
        {
            SaveChangesAndReturnToList();
        }
        if (GUILayout.Button("목록으로 돌아가기"))
        {
            if (_isEditingNewItem)
            {
                _itemTable._itemlist.RemoveAt(_selectedItemIndex);
            }
            _currentView = View.List;
            _selectedItemIndex = -1;
            _isEditingNewItem = false;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SaveChangesAndReturnToList()
    {
        if (SaveTable())
        {
            _currentView = View.List;
            _selectedItemIndex = -1;
        }
    }
    #endregion
}
