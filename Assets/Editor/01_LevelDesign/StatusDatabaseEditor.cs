using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StatusDatabaseEditor : EditorWindow
{
    private enum View { List, Detail }
    private enum StatusListType { Hero, ProtectObject, Monster }

    private View _currentView = View.List;
    private SO_Status_Table _statusTable;
    private Vector2 _scrollPosition;

    // List view foldouts
    private bool _heroFoldout = true;
    private bool _objectFoldout = true;
    private bool _monsterFoldout = true;

    // Detail view state
    private St_StatusTable _editableStatus;
    private int _selectedStatusIndex = -1;
    private StatusListType _selectedListType;
    private bool _isEditingNew = false;
    private Dictionary<int, bool> _gradeFoldouts = new Dictionary<int, bool>();

    [MenuItem("Tools/Tool List/Status Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<StatusDatabaseEditor>("Status Database");
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

        if (_statusTable == null)
        {
            EditorGUILayout.HelpBox("SO_Status_Table 에셋을 찾을 수 없습니다.", MessageType.Error);
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
            EditorUtility.SetDirty(_statusTable);
        }
    }

    private void LoadTable()
    {
        string[] guid = AssetDatabase.FindAssets("t:SO_Status_Table");
        if (guid.Length > 0)
        {
            _statusTable = AssetDatabase.LoadAssetAtPath<SO_Status_Table>(AssetDatabase.GUIDToAssetPath(guid[0]));
        }
    }

    private bool SaveChanges()
    {
        var allStatusEntries = _statusTable._statuslist
            .Concat(_statusTable._statuslist_object)
            .Concat(_statusTable._statuslist_monster);

        var duplicateGroups = allStatusEntries
            .GroupBy(s => s._mid)
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicateGroups.Any())
        {
            string errorMessage = "중복된 스테이터스 ID (_mid)가 있습니다:\n";
            foreach (var group in duplicateGroups)
            {
                string names = string.Join(", ", group.Select(item => $"'{item.customName}'"));
                errorMessage += $"- ID {group.Key}: {names}\n";
            }
            EditorUtility.DisplayDialog("ID 중복 오류", errorMessage, "확인");
            return false;
        }

        EditorUtility.SetDirty(_statusTable);
        AssetDatabase.SaveAssets();
        _statusTable.GetStatusData(0); // Invalidate cache
        ShowNotification(new GUIContent("스테이터스 테이블이 저장되었습니다."));
        return true;
    }

    #region List View
    private void DrawListView()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        _heroFoldout = EditorGUILayout.Foldout(_heroFoldout, "영웅 스테이터스", true, EditorStyles.foldoutHeader);
        if (_heroFoldout) DrawStatusList(ref _statusTable._statuslist, StatusListType.Hero);

        _objectFoldout = EditorGUILayout.Foldout(_objectFoldout, "보호 오브젝트 스테이터스", true, EditorStyles.foldoutHeader);
        if (_objectFoldout) DrawStatusList(ref _statusTable._statuslist_object, StatusListType.ProtectObject);

        _monsterFoldout = EditorGUILayout.Foldout(_monsterFoldout, "몬스터 스테이터스", true, EditorStyles.foldoutHeader);
        if (_monsterFoldout) DrawStatusList(ref _statusTable._statuslist_monster, StatusListType.Monster);

        EditorGUILayout.EndScrollView();
    }

    private void DrawStatusList(ref List<St_StatusTable> statusList, StatusListType type)
    {
        int removeIndex = -1;
        for (int i = 0; i < statusList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"ID: {statusList[i]._mid}", GUILayout.Width(70));
            EditorGUILayout.LabelField(statusList[i].customName);
            if (GUILayout.Button("수정", GUILayout.Width(50)))
            {
                _selectedStatusIndex = i;
                _selectedListType = type;
                _editableStatus = statusList[i];
                _isEditingNew = false;
                _currentView = View.Detail;
                _gradeFoldouts.Clear();
            }
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                if (EditorUtility.DisplayDialog("스테이터스 삭제 확인", $"'{statusList[i].customName}' (ID: {statusList[i]._mid})를 정말 삭제하시겠습니까?", "삭제", "취소"))
                {
                    removeIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex != -1)
        {
            statusList.RemoveAt(removeIndex);
        }

        if (GUILayout.Button("+ " + type.ToString() + " 스테이터스 추가"))
        {
            AddNewStatus(ref statusList, type);
        }
        EditorGUILayout.Space();
    }

    private void AddNewStatus(ref List<St_StatusTable> statusList, StatusListType type)
    {
        var allMids = _statusTable._statuslist.Select(s => s._mid)
            .Concat(_statusTable._statuslist_object.Select(s => s._mid))
            .Concat(_statusTable._statuslist_monster.Select(s => s._mid));
        
        int newId = allMids.Any() ? allMids.Max() + 1 : 1;

        St_StatusTable newStatus = new St_StatusTable
        {
            _mid = newId,
            customName = $"New {type} Status",
            _statuslist = new List<St_Status>()
        };

        statusList.Add(newStatus);
        _selectedStatusIndex = statusList.Count - 1;
        _selectedListType = type;
        _editableStatus = newStatus;
        _isEditingNew = true;
        _currentView = View.Detail;
        _gradeFoldouts.Clear();
    }
    #endregion

    #region Detail View
    private void DrawDetailView()
    {
        EditorGUILayout.LabelField($"스테이터스 상세정보: {_editableStatus.customName}", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _editableStatus.customName = EditorGUILayout.TextField("이름 (Custom Name)", _editableStatus.customName);
        _editableStatus._mid = EditorGUILayout.IntField("스테이터스 ID", _editableStatus._mid);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("등급별 스탯", EditorStyles.boldLabel);

        if (_editableStatus._statuslist == null) _editableStatus._statuslist = new List<St_Status>();

        int removeGradeIndex = -1;
        for(int i = 0; i < _editableStatus._statuslist.Count; i++)
        {
            St_Status status = _editableStatus._statuslist[i];
            
            if (!_gradeFoldouts.ContainsKey(status._grade))
            {
                _gradeFoldouts[status._grade] = true;
            }

            EditorGUILayout.BeginHorizontal();
            _gradeFoldouts[status._grade] = EditorGUILayout.Foldout(_gradeFoldouts[status._grade], $"등급 {status._grade}", true, EditorStyles.foldoutHeader);
            if (GUILayout.Button("-", GUILayout.Width(25))) removeGradeIndex = i;
            EditorGUILayout.EndHorizontal();

            if (_gradeFoldouts[status._grade])
            {
                EditorGUI.indentLevel++;
                status._grade = EditorGUILayout.IntField("Grade", status._grade);
                status._hp = EditorGUILayout.IntField("HP", status._hp);
                status._damge = EditorGUILayout.IntField("Damage", status._damge);
                status._armor = EditorGUILayout.IntField("Armor", status._armor);
                status._critical = EditorGUILayout.Slider("Critical", status._critical, 0f, 1f);
                status._critical_damage = EditorGUILayout.FloatField("Critical Damage", status._critical_damage);
                _editableStatus._statuslist[i] = status;
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(2);
        }

        if (removeGradeIndex != -1)
        {
            _editableStatus._statuslist.RemoveAt(removeGradeIndex);
        }

        if (GUILayout.Button("+ 등급 추가"))
        {
            int newGrade = _editableStatus._statuslist.Any() ? _editableStatus._statuslist.Max(s => s._grade) + 1 : 1;
            _editableStatus._statuslist.Add(new St_Status { _grade = newGrade });
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("적용하고 목록으로"))
        {
            GetTargetList()[_selectedStatusIndex] = _editableStatus;
            if (SaveChanges())
            {
                _currentView = View.List;
                _selectedStatusIndex = -1;
            }
        }
        if (GUILayout.Button("목록으로 돌아가기"))
        {
            if (_isEditingNew)
            {
                GetTargetList().RemoveAt(_selectedStatusIndex);
            }
            _currentView = View.List;
            _selectedStatusIndex = -1;
        }
    }

    private List<St_StatusTable> GetTargetList()
    {
        switch (_selectedListType)
        {
            case StatusListType.Hero: return _statusTable._statuslist;
            case StatusListType.ProtectObject: return _statusTable._statuslist_object;
            case StatusListType.Monster: return _statusTable._statuslist_monster;
            default: return null;
        }
    }
    #endregion
}
