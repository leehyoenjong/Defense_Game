using UnityEditor;
using UnityEngine;

public class LevelDesignHome : EditorWindow
{
    [MenuItem("Tools/Level Design Home")]
    public static void ShowWindow()
    {
        GetWindow<LevelDesignHome>("Level Design Home");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Design Tools", EditorStyles.boldLabel);
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
    }
}

