using System;
using BackEnd;

[Serializable]
public struct St_UserChapterData
{
    public int _lastchapternumber;

    public Param Get_UserData()
    {
        var param = new Param();
        param.Add("_lastchapternumber", _lastchapternumber);
        return param;
    }

    public bool Load_UserData(BackendReturnObject loadresult)
    {
        if (loadresult.IsSuccess() == false)
        {
            return false;
        }

        var userdatajson = loadresult.FlattenRows()[0];

        // 마지막 챕터 번호 로드
        if (userdatajson.ContainsKey("_lastchapternumber"))
        {
            if (int.TryParse(userdatajson["_lastchapternumber"].ToString(), out var chapternumber))
            {
                _lastchapternumber = chapternumber;
            }
        }
        return true;
    }

    public void UserChapterUpdate(GameStateData gamestate)
    {
        if (gamestate._state != EPLAYSTATE.CHAPTER_NEXT)
        {
            return;
        }

        var beforechapternum = _lastchapternumber;
        _lastchapternumber++;
        BackEndLog.WriteLog(LogType.CHAPTER, $"스테이지 변경 전:{beforechapternum} / 변경 후 :{_lastchapternumber}");
    }
}