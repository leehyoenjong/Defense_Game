using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_QuestTable", menuName = "Table/SO_QuestTable", order = 0)]
public class SO_QuestTable : ScriptableObject
{
    List<St_QuestTable> _questlist = new List<St_QuestTable>();

    public List<St_QuestTable> GetQuestTypeList(EQUESTTYPE questtype)
    {
        return _questlist.FindAll(x => x._questtype == questtype);
    }
}

[Serializable]
public struct St_QuestTable
{
    public int _mid;
    public EQUESTTYPE _questtype;

    //오픈 조건
    public EQUESTVALUETYPE _questopentype;
    public int _questopentarget; //아이템 아이디, 몬스터 아이디 등등 여러 방면으로 사용가능
    public int _questopenvalue;

    //클리어 조건 
    public EQUESTVALUETYPE _questcleartype;
    public int _questcleartarget; //아이템 아이디, 몬스터 아이디 등등 여러 방면으로 사용가능
    public int _questclearvalue;

    //보상
    public int _rewarditemid;
    public int _rewarditemvalue;

    /// <summary>
    /// 퀘스트 오픈조건 만족하는지 체크 
    /// </summary>
    /// <returns></returns>
    public bool CheckOpenQuest()
    {
        var userdata = UserData._userdata;
        switch (_questopentype)
        {
            case EQUESTVALUETYPE.CHAPERCLEAR:
                return userdata._chapterdata._lastchapternumber > _questopentarget;
            case EQUESTVALUETYPE.TARGETQUESTCLEAR:
                return userdata._userquestdata.CheckQuestClear(_questopentarget);
            case EQUESTVALUETYPE.MONSTERKILL:
            case EQUESTVALUETYPE.TARGETMONSTERKILL:
            case EQUESTVALUETYPE.GACHA:
            case EQUESTVALUETYPE.TARGETGACHA:
            case EQUESTVALUETYPE.UPGRADE:
            case EQUESTVALUETYPE.TARGETHEROUPGRADE:
            case EQUESTVALUETYPE.QUESTCLEARCOUNT:
            case EQUESTVALUETYPE.TARGETQUESTCLEARCOUNT:
                return userdata._userquestdata.GetQuestValue(_questopentype, _questopentarget) >= _questopenvalue;
            default:
                return true;
        }
    }

    /// <summary>
    /// 클리어 여부 확인
    /// </summary>
    /// <returns></returns>
    public bool CheckClearQuest()
    {
        var userdata = UserData._userdata;
        switch (_questcleartype)
        {
            case EQUESTVALUETYPE.CHAPERCLEAR:
                return userdata._chapterdata._lastchapternumber > _questcleartarget;
            case EQUESTVALUETYPE.TARGETQUESTCLEAR:
                return userdata._userquestdata.CheckQuestClear(_questclearvalue);
            case EQUESTVALUETYPE.MONSTERKILL:
            case EQUESTVALUETYPE.TARGETMONSTERKILL:
            case EQUESTVALUETYPE.GACHA:
            case EQUESTVALUETYPE.TARGETGACHA:
            case EQUESTVALUETYPE.UPGRADE:
            case EQUESTVALUETYPE.TARGETHEROUPGRADE:
            case EQUESTVALUETYPE.QUESTCLEARCOUNT:
            case EQUESTVALUETYPE.TARGETQUESTCLEARCOUNT:
                return userdata._userquestdata.GetQuestValue(_questopentype, _questcleartarget) >= _questclearvalue;
            default:
                return true;
        }
    }
}

public enum EQUESTTYPE
{
    DAY,
    WEEK,
    ACHIEVEMENTS,
    REPEAT
}

public enum EQUESTVALUETYPE
{
    NONE,
    CHAPERCLEAR,
    MONSTERKILL,
    TARGETMONSTERKILL,
    GACHA,
    TARGETGACHA,
    UPGRADE,
    TARGETHEROUPGRADE,
    QUESTCLEARCOUNT,
    TARGETQUESTCLEAR,
    TARGETQUESTCLEARCOUNT
}