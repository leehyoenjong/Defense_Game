using System;
using System.Collections.Generic;

public struct St_UserQuestData
{
    public List<St_UserQuestList> _questlist;
    public List<int> _questclearid;

    public int GetQuestValue(EQUESTVALUETYPE cleartype, int targetid)
    {
        return _questlist.Find(x => x._cleartype == cleartype && x._targetid == targetid)._totalvalue;
    }

    public bool CheckQuestClear(int questid)
    {
        return _questclearid.Contains(questid);
    }
}

[Serializable]
public struct St_UserQuestList
{
    //조건
    public EQUESTVALUETYPE _cleartype;
    public int _targetid;

    //획득한 수
    public int _totalvalue;
}