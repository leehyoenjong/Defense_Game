using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_StageData", menuName = "SO_StageData", order = 0)]
public class SO_StageData : ScriptableObject
{
    public int _stageid;
    public List<St_Stage> _monsterlist;
}

[Serializable]
public struct St_Stage
{
    public int _monsterid;
    public int _count;
    public float _delaytime;

    public St_MonsterTable GetMonsterInfo() => DataManager.instance.GetMonsterInfo(_monsterid);
}