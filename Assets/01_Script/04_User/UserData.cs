using System;
using System.Collections.Generic;

public class UserData
{
    public St_UserHeroList[] _userherodata;

    public UserData()
    {
        _userherodata = new St_UserHeroList[5];
        _userherodata[0]._heroid = 1;
        _userherodata[0]._heropoint = 1;
    }
}


[Serializable]
public struct St_UserHeroList
{
    public int _heroid;
    public int _heropoint;
}