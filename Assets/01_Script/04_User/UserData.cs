using System;
using System.Collections.Generic;

public class UserData
{
    static UserData _instance;
    public static UserData _userdata
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UserData();
            }
            return _instance;
        }
    }


    public St_UserChapterData _chapterdata;
    public St_UserInventory _userinventory;
    public St_UserEquitHero _userequiphero;
    public St_UserQuestData _userquestdata;

    public UserData()
    {
        PlayManager._play_chapter_next += UserChapterUpdate;
        CreateNewUserData();
    }

    ~UserData()
    {
        PlayManager._play_chapter_next -= UserChapterUpdate;
    }

    void CreateNewUserData()
    {
        _chapterdata = new St_UserChapterData();
        _chapterdata._lastchapternumber = 1;

        _userinventory = new St_UserInventory();
        _userinventory._userinvendata = new List<St_UserInvenItemList>();

        _userequiphero = new St_UserEquitHero();
        _userequiphero._equipheroid = new List<int>() { 10000, 0, 0,};
    }

    public void UserChapterUpdate()
    {
        _chapterdata._lastchapternumber++;
    }

}


[Serializable]
public struct St_UserHeroList
{
    public int _heroid;
    public int _heropoint;
}


[Serializable]
public struct St_UserChapterData
{
    public int _lastchapternumber;
}