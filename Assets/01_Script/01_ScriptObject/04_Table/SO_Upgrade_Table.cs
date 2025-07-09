using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Upgrade_Table", menuName = "Table/SO_Upgrade_Table", order = 0)]
public class SO_Upgrade_Table : ScriptableObject
{
    public List<St_UpgradeTable> _upgradetable = new List<St_UpgradeTable>();
    const int MAXGRADE = 5;

    public St_UpgradeTable GetUpgradeData(int currentgrade)
    {
        int nextgrade = currentgrade + 1;
        if (nextgrade >= MAXGRADE)
        {
            return default;
        }
        return _upgradetable.Find(x => x._grade == nextgrade);
    }
}

[Serializable]
public struct St_UpgradeTable
{
    public int _grade;
    public int _priceitemid;
    public int _price;
}