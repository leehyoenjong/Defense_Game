using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Status_Table", menuName = "Table/SO_Status_Table", order = 0)]
public class SO_Status_Table : ScriptableObject
{
    public List<St_StatusTable> _statuslist;

    public List<St_Status> GetStatusData(int statusid)
    {
        return _statuslist.Find(x => x._mid == statusid)._statuslist;
    }
}


[Serializable]
public struct St_StatusTable
{
    public int _mid;
    public List<St_Status> _statuslist;
}

[Serializable]
public struct St_Status
{
    public int _grade;
    public int _hp;
    public int _damge;
    public int _armor;
    public float _critical;// 0~1
    public float _critical_damage; // 0~1
}