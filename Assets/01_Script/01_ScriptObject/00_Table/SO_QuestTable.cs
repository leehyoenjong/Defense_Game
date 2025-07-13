using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SO_QuestTable", menuName = "Table/SO_QuestTable", order = 0)]
public class SO_QuestTable : ScriptableObject
{
    [Header("반복 퀘스트")]
    public List<St_QuestTable> _repeatquestlist = new List<St_QuestTable>();

    [Header("일일 퀘스트")]
    public List<St_QuestTable> _dayquestlist = new List<St_QuestTable>();

    [Header("주간 퀘스트")]
    public List<St_QuestTable> _weekquestlist = new List<St_QuestTable>();

    [Header("업적 퀘스트")]
    public List<St_QuestTable> _achievementsquestlist = new List<St_QuestTable>();

    List<St_QuestTable> _alllist = new List<St_QuestTable>();

    public List<St_QuestTable> GetQuestTypeList(EQUESTTYPE questtype)
    {
        switch (questtype)
        {
            case EQUESTTYPE.REPEAT:
                return _repeatquestlist;
            case EQUESTTYPE.DAY:
                return _dayquestlist;
            case EQUESTTYPE.WEEK:
                return _weekquestlist;
            case EQUESTTYPE.ACHIEVEMENTS:
                return _achievementsquestlist;
        }
        return null;
    }

    public St_QuestTable GetQuestInfo(int questid)
    {
        _alllist.Clear();
        if (_alllist.Count <= 0)
        {
            _alllist.AddRange(_repeatquestlist);
            _alllist.AddRange(_dayquestlist);
            _alllist.AddRange(_weekquestlist);
            _alllist.AddRange(_achievementsquestlist);
        }
        return _alllist.First(x => x._mid == questid);
    }
}

[Serializable]
public struct St_QuestTable
{
    public int _mid;
    public EQUESTTYPE _questtype;

    //퀘스트 소개
    public string _title;
    public string _explain;
    public bool _isclearactiveoff;//퀘스트 완료 시 슬롯이 안보이게 할 것인가? true면 안보이게 flase면 보이게 

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
            case EQUESTVALUETYPE.CHAPTERCLEAR:
                return userdata._userchapterdata._lastchapternumber > _questopentarget;
            case EQUESTVALUETYPE.TARGETQUESTCLEAR:
                return userdata._userquestdata.CheckQuestClear(_questopentarget);
            case EQUESTVALUETYPE.MONSTERKILL:
            case EQUESTVALUETYPE.TARGETMONSTERKILL:
            case EQUESTVALUETYPE.GACHA:
            case EQUESTVALUETYPE.TARGETGACHA:
            case EQUESTVALUETYPE.UPGRADE:
            case EQUESTVALUETYPE.TARGETHEROUPGRADE:
            case EQUESTVALUETYPE.QUESTCLEARCOUNT:
            case EQUESTVALUETYPE.TARGETTYPEQUESTCLEARCOUNT:
                return userdata._userquestdata.GetQuestValue(_questtype, _questopentype, _questopentarget) >= _questopenvalue;
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
            case EQUESTVALUETYPE.CHAPTERCLEAR:
                return userdata._userchapterdata._lastchapternumber > _questcleartarget;
            case EQUESTVALUETYPE.TARGETQUESTCLEAR:
                return userdata._userquestdata.CheckQuestClear(_questclearvalue);
            case EQUESTVALUETYPE.MONSTERKILL:
            case EQUESTVALUETYPE.TARGETMONSTERKILL:
            case EQUESTVALUETYPE.GACHA:
            case EQUESTVALUETYPE.TARGETGACHA:
            case EQUESTVALUETYPE.UPGRADE:
            case EQUESTVALUETYPE.TARGETHEROUPGRADE:
            case EQUESTVALUETYPE.QUESTCLEARCOUNT:
            case EQUESTVALUETYPE.TARGETTYPEQUESTCLEARCOUNT:
                return userdata._userquestdata.GetQuestValue(_questtype, _questcleartype, _questcleartarget) >= _questclearvalue;
            default:
                return true;
        }
    }

    public (int uservalue, int questvalue) GetQuestClearAndUserValue()
    {
        var userdata = UserData._userdata;
        switch (_questcleartype)
        {
            case EQUESTVALUETYPE.CHAPTERCLEAR:
                return (userdata._userchapterdata._lastchapternumber, _questcleartarget);
            case EQUESTVALUETYPE.TARGETQUESTCLEAR:
                var isclear = userdata._userquestdata.CheckQuestClear(_questclearvalue);
                return (isclear ? 1 : 0, 1);
            case EQUESTVALUETYPE.MONSTERKILL:
            case EQUESTVALUETYPE.TARGETMONSTERKILL:
            case EQUESTVALUETYPE.GACHA:
            case EQUESTVALUETYPE.TARGETGACHA:
            case EQUESTVALUETYPE.UPGRADE:
            case EQUESTVALUETYPE.TARGETHEROUPGRADE:
            case EQUESTVALUETYPE.QUESTCLEARCOUNT:
            case EQUESTVALUETYPE.TARGETTYPEQUESTCLEARCOUNT:
                return (userdata._userquestdata.GetQuestValue(_questtype, _questcleartype, _questcleartarget), _questclearvalue);
            default:
                return (0, 0);
        }
    }

    public void QuestMove()
    {
        switch (_questcleartype)
        {
            case EQUESTVALUETYPE.CHAPTERCLEAR:
            case EQUESTVALUETYPE.MONSTERKILL:
            case EQUESTVALUETYPE.TARGETMONSTERKILL:
                SceneManager.LoadSceneAsync("01_LOBBY");
                break;
            case EQUESTVALUETYPE.GACHA:
            case EQUESTVALUETYPE.TARGETGACHA:
                var shsopbtn = GameObject.Find("Btn_Shop");
                shsopbtn?.GetComponent<Button>().onClick.Invoke();
                break;
            case EQUESTVALUETYPE.UPGRADE:
            case EQUESTVALUETYPE.TARGETHEROUPGRADE:
                var invenbtn = GameObject.Find("Btn_Inven_Hero");
                invenbtn?.GetComponent<Button>().onClick.Invoke();
                break;
            case EQUESTVALUETYPE.TARGETQUESTCLEAR:
            case EQUESTVALUETYPE.QUESTCLEARCOUNT:
            case EQUESTVALUETYPE.TARGETTYPEQUESTCLEARCOUNT:
            default:
                break;
        }
    }
}

public enum EQUESTTYPE
{
    REPEAT,
    DAY,
    WEEK,
    ACHIEVEMENTS,
    MAX,//마지막 번호를 확인하기 위함
}

public enum EQUESTVALUETYPE
{
    NONE,
    CHAPTERCLEAR,
    MONSTERKILL,
    TARGETMONSTERKILL,
    GACHA,
    TARGETGACHA,
    UPGRADE,
    TARGETHEROUPGRADE,
    QUESTCLEARCOUNT,
    TARGETTYPEQUESTCLEARCOUNT,
    TARGETQUESTCLEAR,
}