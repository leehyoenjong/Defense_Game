using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BackEnd;
using BackEnd.BackndNewtonsoft.Json;
using UnityEngine;

[Serializable]
public struct St_UserQuestData
{
    public List<St_UserQuestList> _questvaluelist;//퀘스트 값 리스트 
    public List<int> _questclearid;//퀘스트 클리어 리스트

    public Param Get_UserData()
    {
        var param = new Param();
        param.Add("_questvaluelist", _questvaluelist);
        param.Add("_questclearid", _questclearid);
        return param;
    }

    public bool Load_UserData(BackendReturnObject loadresult)
    {
        if (loadresult.IsSuccess() == false)
        {
            return false;
        }

        var userdatajson = loadresult.FlattenRows();

        // 퀘스트 클리어 ID 리스트 로드
        if (userdatajson.ContainsKey("_questclearid"))
        {
            var questclearid = userdatajson["_questclearid"];
            var maxcount = questclearid.Count;
            for (int i = 0; i < maxcount; i++)
            {
                if (int.TryParse(questclearid[i].ToString(), out var clearid) == false)
                {
                    continue;
                }
                _questclearid.Add(clearid);
            }
        }

        // 퀘스트 값 리스트 로드
        if (userdatajson.ContainsKey("_questvaluelist"))
        {
            var questvaluelist = userdatajson["_questvaluelist"];
            var maxcount = questvaluelist.Count;
            for (int i = 0; i < maxcount; i++)
            {
                try
                {
                    var questData = JsonConvert.DeserializeObject<St_UserQuestList>(questvaluelist[i].ToString());
                    _questvaluelist.Add(questData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"퀘스트 값 리스트 로드 실패: {ex.Message}");
                    continue;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 퀘스트 값 가져오기 
    /// </summary>
    public int GetQuestValue(EQUESTTYPE questtype, EQUESTVALUETYPE cleartype, int targetid)
    {
        if (cleartype == EQUESTVALUETYPE.TARGETQUESTCLEAR)
        {
            var values = UserData._userdata._userquestdata.CheckQuestClear(targetid) ? 1 : 0;
            return values;
        }
        else if (cleartype == EQUESTVALUETYPE.CHAPTERCLEAR)
        {
            return UserData._userdata._userchapterdata._lastchapternumber;
        }

        St_UserQuestList userquest = default;

        if (targetid > 0)
        {
            userquest = _questvaluelist.FirstOrDefault(x => x._questvaluetype == cleartype && x._questtype == questtype && x._targetid == targetid);
        }
        else
        {
            userquest = _questvaluelist.FirstOrDefault(x => x._questvaluetype == cleartype && x._questtype == questtype);
        }



        return userquest._totalvalue;
    }

    /// <summary>
    /// 퀘스트 클리어 여부
    /// </summary>
    public bool CheckQuestClear(int questid)
    {
        return _questclearid.Contains(questid);
    }

    /// <summary>
    /// 퀘스트 클리어 처리 
    /// </summary>
    public void ClearQuestUpdate(int questid, int clearcount = 1)
    {
        var questinfo = DataManager.instance.GetQuestInfo(questid);
        if (questinfo._mid == 0)
        {
            //TODO: 퀘스트 아이디가 없으면 에러
            Debug.LogError("퀘스트 아이디에 문제있음!");
            return;
        }

        //반복 퀘스트는 완료로 남기지 않는다.
        if (questinfo._questtype == EQUESTTYPE.REPEAT)
        {
            UserData._userdata._userquestdata.QuestClearUpdateValue(questinfo, clearcount);
            return;
        }

        if (CheckQuestClear(questid))
        {
            //TODO: 같은 퀘스트를 여러번 완료하는 건 에러다
            Debug.LogError("여러번 클리어함! / 퀘스트 아이디에 문제있음!");
        }
        _questclearid.Add(questid);
    }

    public void QuestClearUpdateValue(St_QuestTable questinfo, int clearcount)
    {
        switch (questinfo._questcleartype)
        {
            case EQUESTVALUETYPE.MONSTERKILL:
            case EQUESTVALUETYPE.TARGETMONSTERKILL:
            case EQUESTVALUETYPE.GACHA:
            case EQUESTVALUETYPE.TARGETGACHA:
            case EQUESTVALUETYPE.UPGRADE:
            case EQUESTVALUETYPE.TARGETHEROUPGRADE:
            case EQUESTVALUETYPE.QUESTCLEARCOUNT:
            case EQUESTVALUETYPE.TARGETTYPEQUESTCLEARCOUNT:
                break;
            case EQUESTVALUETYPE.CHAPTERCLEAR:
            case EQUESTVALUETYPE.TARGETQUESTCLEAR:
            //TODO: 챕터와 클리어는 별도 데이터가 있기 때문에 하지 않음
            default:
                return;
        }

        var questindex = _questvaluelist.FindIndex(x => x._questvaluetype == questinfo._questcleartype && x._questtype == questinfo._questtype && x._targetid == questinfo._questcleartarget);
        if (questindex == -1)
        {
            return;
        }

        var userquestinfo = _questvaluelist[questindex];
        userquestinfo._totalvalue += -questinfo._questclearvalue * clearcount;
        _questvaluelist[questindex] = userquestinfo;
    }

    /// <summary>
    /// 퀘스트 값 저장
    /// </summary>
    public void UpdateQuestValue(EQUESTVALUETYPE questvaluetype, int targetid, int targetvalue)
    {
        switch (questvaluetype)
        {
            case EQUESTVALUETYPE.MONSTERKILL:
            case EQUESTVALUETYPE.TARGETMONSTERKILL:
            case EQUESTVALUETYPE.GACHA:
            case EQUESTVALUETYPE.TARGETGACHA:
            case EQUESTVALUETYPE.UPGRADE:
            case EQUESTVALUETYPE.TARGETHEROUPGRADE:
            case EQUESTVALUETYPE.QUESTCLEARCOUNT:
            case EQUESTVALUETYPE.TARGETTYPEQUESTCLEARCOUNT:
                break;
            case EQUESTVALUETYPE.CHAPTERCLEAR:
            case EQUESTVALUETYPE.TARGETQUESTCLEAR:
            //TODO: 챕터와 클리어는 별도 데이터가 있기 때문에 하지 않음
            default:
                return;
        }

        //동일한 _questvaluetype의 모든  _questtype에 업데이트 해주어야하기 때문에 반복문을 통해 업데이트 값 추가 해주기
        var maxcount = (int)EQUESTTYPE.MAX;
        for (int i = 0; i < maxcount; i++)
        {
            St_UserQuestList userquest = default;
            var targetquesttype = (EQUESTTYPE)i;
            var questindex = _questvaluelist.FindIndex(x => x._questvaluetype == questvaluetype && x._questtype == targetquesttype && x._targetid == targetid);
            if (questindex == -1)
            {
                userquest._questtype = targetquesttype;
                userquest._questvaluetype = questvaluetype;
                userquest._targetid = targetid;
                userquest._totalvalue = targetvalue;
                _questvaluelist.Add(userquest);
                continue;
            }
            userquest = _questvaluelist[questindex];
            userquest._totalvalue += targetvalue;
            _questvaluelist[questindex] = userquest;
        }
    }
}

[Serializable]
public struct St_UserQuestList
{
    public EQUESTTYPE _questtype;
    public EQUESTVALUETYPE _questvaluetype;
    public int _targetid;
    public int _totalvalue;
}