using UnityEditor;
using UnityEngine;

public class ToolHome : EditorWindow
{
    [MenuItem("Tools/Tool Home")]
    public static void ShowWindow()
    {
        GetWindow<ToolHome>("Tool Home");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tools", EditorStyles.boldLabel);
        GUILayout.Space(20);

        if (GUILayout.Button("Hero Database Editor", GUILayout.Height(30)))
        {
            HeroDatabaseEditor.ShowWindow();
            this.Close();
        }

        if (GUILayout.Button("Monster Database Editor", GUILayout.Height(30)))
        {
            MonsterDatabaseEditor.ShowWindow();
            this.Close();
        }

        if (GUILayout.Button("Level Design Tool", GUILayout.Height(30)))
        {
            LevelDesignTool.ShowWindow();
            this.Close();
        }

        if (GUILayout.Button("Hero Placement Editor", GUILayout.Height(30)))
        {
            HeroPlacementEditor.ShowWindow();
            this.Close();
        }

        if (GUILayout.Button("Item Database Editor", GUILayout.Height(30)))
        {
            ItemDatabaseEditor.ShowWindow();
            this.Close();
        }

        if (GUILayout.Button("Shop Database Editor", GUILayout.Height(30)))
        {
            ShopDatabaseEditor.ShowWindow();
            this.Close();
        }

        if (GUILayout.Button("Gacha Database Editor", GUILayout.Height(30)))
        {
            GachaDatabaseEditor.ShowWindow();
            this.Close();
        }

        if (GUILayout.Button("Quest Database Editor", GUILayout.Height(30)))
        {
            QuestDatabaseEditor.ShowWindow();
            this.Close();
        }

        if (GUILayout.Button("Upgrade Database Editor", GUILayout.Height(30)))
        {
            UpgradeDatabaseEditor.ShowWindow();
            this.Close();
        }

        if (GUILayout.Button("Status Database Editor", GUILayout.Height(30)))
        {
            StatusDatabaseEditor.ShowWindow();
            this.Close();
        }

        if (GUILayout.Button("Protect Object Database Editor", GUILayout.Height(30)))
        {
            ProtectObjectDatabaseEditor.ShowWindow();
            this.Close();
        }
    }
}

