using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_MonsterTable", menuName = "Table/SO_MonsterTable", order = 0)]
public class SO_MonsterTable : ScriptableObject
{
    [SerializeField] List<St_MonsterTable> _monsterlist;
    public List<St_MonsterTable> GetMonsterList() => _monsterlist;
    public St_MonsterTable GetMonsterInfo(int heroid)
    {
        var monsterdata = _monsterlist.Find(x => x._npc._mid == heroid);
        return monsterdata;
    }
}

[Serializable]
public struct St_MonsterTable
{
    public SO_NPC _npc;

    [Header("사망시 골드")]
    public int _diegold;
}