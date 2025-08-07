using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_ProtectTable", menuName = "Table/SO_ProtectTable", order = 0)]
public class SO_ProtectTable : ScriptableObject
{
    [SerializeField] List<St_ProtectObject> _protectlist;
    public List<St_ProtectObject> GetProtectList() => _protectlist;
    public St_ProtectObject GetProtectInfo(int protectid)
    {
        var protectdata = _protectlist.Find(x => x._npc._mid == protectid);
        return protectdata;
    }
}

[Serializable]
public struct St_ProtectObject
{
    public SO_NPC _npc;
}