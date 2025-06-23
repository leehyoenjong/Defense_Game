using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_NPC", menuName = "SO_NPC", order = 0)]
public class SO_NPC : ScriptableObject
{
    public St_Status _status;
    public SO_Skill_Attack[] _skill_Attack;
    public SO_Skill_Buff[] _skill_buff;
    public int _diegold;
}


[Serializable]
public struct St_Status
{
    public int _hp;
    public int _damge;
    public int _armor;
    public float _critical;// 0~1
    public float _critical_damage; // 0~1
}