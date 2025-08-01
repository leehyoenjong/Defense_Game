using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Status_Table", menuName = "Table/SO_Status_Table", order = 0)]
public class SO_Status_Table : ScriptableObject
{
    [Header("ㅡㅡㅡ리스트는 다르지만 같은 MID를 사용하면 안됨ㅡㅡㅡ")]

    [Header("영웅 스테이터스")]
    public List<St_StatusTable> _statuslist;

    [Header("보호 오브젝트 스테이터스")]
    public List<St_StatusTable> _statuslist_object;
    [Header("몬스터 스테이터스")]
    public List<St_StatusTable> _statuslist_monster;

    List<St_StatusTable> _statuslist_total;

    public List<St_Status> GetStatusData(int statusid)
    {
        if (_statuslist_total == null || _statuslist_total.Count <= 0)
        {
            _statuslist_total.AddRange(_statuslist);
            _statuslist_total.AddRange(_statuslist_object);
            _statuslist_total.AddRange(_statuslist_monster);
        }

        return _statuslist_total.Find(x => x._mid == statusid)._statuslist;
    }
}


[Serializable]
public struct St_StatusTable
{
    [Space(5f)]
    [Header("식별 정보")]
    public string customName;  // 사용자가 직접 입력할 이름
    [Space(10f)]
    public int _mid;
    [Space(5f)]
    public List<St_Status> _statuslist;
}

[Serializable]
public struct St_Status
{
    [Space(5f)]
    public int _grade;
    public int _hp;
    public int _damge;
    public int _armor;
    public float _critical;// 0~1
    public float _critical_damage; // 0~1
}