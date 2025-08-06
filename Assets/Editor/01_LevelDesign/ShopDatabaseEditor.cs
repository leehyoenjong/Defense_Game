using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopDatabaseEditor : EditorWindow
{
    private enum View { List, Detail }
    private View _currentView = View.List;

    private SO_Shop_Table _shopTable;
    private SO_Item_Table _itemTable;
    private Vector2 _scrollPosition;

    private int _selectedShopItemIndex = -1;
    private St_ShopTable _editableShopItem;
    private bool _isEditingNewItem = false;

    [MenuItem("Tools/Tool List/Shop Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<ShopDatabaseEditor>("Shop Database");
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

        if (_shopTable == null || _itemTable == null)
        {
            EditorGUILayout.HelpBox("필요한 테이블(SO_Shop_Table, SO_Item_Table)을 찾을 수 없습니다.", MessageType.Error);
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
            EditorUtility.SetDirty(_shopTable);
        }
    }

    private void LoadTables()
    {
        string[] shopTableGuid = AssetDatabase.FindAssets("t:SO_Shop_Table");
        if (shopTableGuid.Length > 0)
        {
            _shopTable = AssetDatabase.LoadAssetAtPath<SO_Shop_Table>(AssetDatabase.GUIDToAssetPath(shopTableGuid[0]));
        }

        string[] itemTableGuid = AssetDatabase.FindAssets("t:SO_Item_Table");
        if (itemTableGuid.Length > 0)
        {
            _itemTable = AssetDatabase.LoadAssetAtPath<SO_Item_Table>(AssetDatabase.GUIDToAssetPath(itemTableGuid[0]));
        }
    }

    private bool SaveTable()
    {
        if (_selectedShopItemIndex != -1)
        {
            _shopTable._shoplist[_selectedShopItemIndex] = _editableShopItem;
        }

        var duplicateGroups = _shopTable._shoplist
            .GroupBy(item => item._shopid)
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicateGroups.Any())
        {
            string errorMessage = "중복된 상점 ID가 있습니다:\n";
            foreach (var group in duplicateGroups)
            {
                string itemNames = string.Join(", ", group.Select(item => $"'{item._title}'"));
                errorMessage += $"- ID {group.Key}: {itemNames}\n";
            }
            EditorUtility.DisplayDialog("ID 중복 오류", errorMessage, "확인");
            return false;
        }
        
        EditorUtility.SetDirty(_shopTable);
        AssetDatabase.SaveAssets();
        
        typeof(SO_Shop_Table).GetField("_shoplist_dic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_shopTable, new Dictionary<int, St_ShopTable>());

        ShowNotification(new GUIContent("상점 테이블이 저장되었습니다."));
        return true;
    }

    #region List View
    private void DrawListView()
    {
        EditorGUILayout.LabelField("상점 상품 목록", EditorStyles.boldLabel);

        if (GUILayout.Button("신규 상품 추가"))
        {
            AddNewShopItem();
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        int removeIndex = -1;
        for (int i = 0; i < _shopTable._shoplist.Count; i++)
        {
            St_ShopTable shopItem = _shopTable._shoplist[i];

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(shopItem._title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Shop ID: {shopItem._shopid}");
            
            string priceItemName = shopItem._priceitemid == -1 ? "광고" : _itemTable.SearchItemData(shopItem._priceitemid)._itemname ?? "알 수 없음";
            EditorGUILayout.LabelField($"가격: {shopItem.GetPriceText()} ({priceItemName})");
            
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("상세정보", GUILayout.Width(80), GUILayout.Height(60)))
            {
                _selectedShopItemIndex = i;
                _editableShopItem = shopItem;
                _currentView = View.Detail;
            }

            if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(60)))
            {
                if (EditorUtility.DisplayDialog("상품 삭제 확인", $"'{shopItem._title}' 상품을 목록에서 정말 삭제하시겠습니까?", "삭제", "취소"))
                {
                    removeIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex != -1)
        {
            _shopTable._shoplist.RemoveAt(removeIndex);
        }

        EditorGUILayout.EndScrollView();
    }

    private void AddNewShopItem()
    {
        St_ShopTable newItem = new St_ShopTable();
        int newId = 1;
        if (_shopTable._shoplist.Any())
        {
            newId = _shopTable._shoplist.Max(item => item._shopid) + 1;
        }
        newItem._shopid = newId;
        newItem._title = $"New Shop Item {newId}";
        newItem._sellitemlist = new List<St_RewardItemList>();

        _shopTable._shoplist.Add(newItem);
        _selectedShopItemIndex = _shopTable._shoplist.Count - 1;
        _editableShopItem = newItem;
        _currentView = View.Detail;
        _isEditingNewItem = true;
    }
    #endregion

    #region Detail View
    private void DrawDetailView()
    {
        if (_selectedShopItemIndex == -1)
        {
            EditorGUILayout.HelpBox("표시할 상품이 선택되지 않았습니다.", MessageType.Warning);
            if (GUILayout.Button("목록으로 돌아가기")) _currentView = View.List;
            return;
        }

        EditorGUILayout.LabelField($"상품 상세정보: {_editableShopItem._title}", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _editableShopItem._shopid = EditorGUILayout.IntField("상점 ID", _editableShopItem._shopid);
        _editableShopItem._title = EditorGUILayout.TextField("상품 제목", _editableShopItem._title);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("가격 정보", EditorStyles.boldLabel);

        string priceItemName = "알 수 없음";
        if (_editableShopItem._priceitemid == -1)
        {
            priceItemName = "광고";
        }
        else if (_editableShopItem._priceitemid > 0)
        {
            priceItemName = _itemTable.SearchItemData(_editableShopItem._priceitemid)._itemname ?? "ID 없음";
        }

        if (EditorGUILayout.DropdownButton(new GUIContent(priceItemName), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("광고"), _editableShopItem._priceitemid == -1, () => { _editableShopItem._priceitemid = -1; });
            
            foreach (var itemEntry in _itemTable._itemlist)
            {
                menu.AddItem(new GUIContent($"{itemEntry._itemname} (ID: {itemEntry._itemid})"), itemEntry._itemid == _editableShopItem._priceitemid, () => 
                {
                    _editableShopItem._priceitemid = itemEntry._itemid;
                });
            }
            menu.ShowAsContext();
        }

        if (_editableShopItem._priceitemid != -1)
        {
            _editableShopItem._price = EditorGUILayout.IntField("가격", _editableShopItem._price);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("판매 아이템 목록", EditorStyles.boldLabel);

        if (_editableShopItem._sellitemlist == null)
        {
            _editableShopItem._sellitemlist = new List<St_RewardItemList>();
        }

        int removeItemIndex = -1;
        for (int i = 0; i < _editableShopItem._sellitemlist.Count; i++)
        {
            St_RewardItemList rewardItem = _editableShopItem._sellitemlist[i];
            
            EditorGUILayout.BeginHorizontal();

            // Item ID Dropdown
            int currentItemId = rewardItem._itemid;
            string currentItemName = _itemTable.SearchItemData(currentItemId)._itemname ?? (currentItemId == 0 ? "없음" : "ID를 찾을 수 없음");
            
            if (EditorGUILayout.DropdownButton(new GUIContent(currentItemName), FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("없음"), currentItemId == 0, () => 
                {
                    var item = _editableShopItem._sellitemlist[i];
                    item._itemid = 0;
                    _editableShopItem._sellitemlist[i] = item;
                });

                foreach (var itemEntry in _itemTable._itemlist)
                {
                    menu.AddItem(new GUIContent($"{itemEntry._itemname} (ID: {itemEntry._itemid})"), itemEntry._itemid == currentItemId, () => 
                    {
                        var item = _editableShopItem._sellitemlist[i];
                        item._itemid = itemEntry._itemid;
                        _editableShopItem._sellitemlist[i] = item;
                    });
                }
                menu.ShowAsContext();
            }

            // Item Value
            rewardItem._itemvalue = EditorGUILayout.IntField("수량", rewardItem._itemvalue, GUILayout.MaxWidth(200));
            _editableShopItem._sellitemlist[i] = rewardItem;

            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                removeItemIndex = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (removeItemIndex != -1)
        {
            _editableShopItem._sellitemlist.RemoveAt(removeItemIndex);
        }

        if (GUILayout.Button("+ 판매 아이템 추가"))
        {
            _editableShopItem._sellitemlist.Add(new St_RewardItemList());
        }


        EditorGUILayout.Space(20);
        
        if (GUILayout.Button("적용하고 목록으로"))
        {
            if(SaveTable())
            {
                _currentView = View.List;
                _selectedShopItemIndex = -1;
                _isEditingNewItem = false;
            }
        }
        if (GUILayout.Button("목록으로 돌아가기"))
        {
            if (_isEditingNewItem)
            {
                _shopTable._shoplist.RemoveAt(_selectedShopItemIndex);
            }
            _currentView = View.List;
            _selectedShopItemIndex = -1;
            _isEditingNewItem = false;
        }
    }
    
    #endregion
}
