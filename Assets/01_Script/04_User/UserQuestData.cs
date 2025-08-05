using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BackEnd;
using UnityEngine;
using LitJson;

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
        if (!ValidateLoadResult(loadresult))
            return false;

        var userData = ExtractUserData(loadresult);

        LoadQuestClearIds(userData);
        LoadQuestValueList(userData);

        Debug.Log($"퀘스트 데이터 로드 완료 - 클리어 퀘스트: {_questclearid.Count}개, 진행 퀘스트: {_questvaluelist.Count}개");
        return true;
    }

    /// <summary>
    /// 로드 결과 검증
    /// </summary>
    bool ValidateLoadResult(BackendReturnObject loadresult)
    {
        if (loadresult.IsSuccess())
            return true;

        Debug.LogError("퀘스트 데이터 로드 실패");
        return false;
    }

    /// <summary>
    /// JSON 데이터 추출
    /// </summary>
    JsonData ExtractUserData(BackendReturnObject loadresult)
    {
        var userData = loadresult.FlattenRows()[0];
        Debug.Log($"퀘스트 전체 데이터 로드 시작");
        return userData;
    }

    /// <summary>
    /// 퀘스트 클리어 ID 리스트 로드
    /// </summary>
    void LoadQuestClearIds(JsonData userData)
    {
        if (!userData.ContainsKey("_questclearid"))
        {
            Debug.Log("클리어 퀘스트 데이터 없음");
            return;
        }

        var questClearIds = userData["_questclearid"];
        var loadedCount = 0;

        for (int i = 0; i < questClearIds.Count; i++)
        {
            if (TryParseQuestClearId(questClearIds[i], i, out var clearId))
            {
                _questclearid.Add(clearId);
                loadedCount++;
            }
        }

        Debug.Log($"클리어 퀘스트 로드 완료: {loadedCount}/{questClearIds.Count}개");
    }

    /// <summary>
    /// 퀘스트 클리어 ID 파싱
    /// </summary>
    bool TryParseQuestClearId(JsonData jsonData, int index, out int clearId)
    {
        clearId = 0;
        if (int.TryParse(jsonData.ToString(), out clearId))
            return true;

        Debug.LogWarning($"퀘스트 클리어 ID [{index}] 파싱 실패: {jsonData}");
        return false;
    }

    /// <summary>
    /// 퀘스트 값 리스트 로드
    /// </summary>
    void LoadQuestValueList(JsonData userData)
    {
        if (!userData.ContainsKey("_questvaluelist"))
        {
            Debug.Log("진행 퀘스트 데이터 없음");
            return;
        }

        var questValueList = userData["_questvaluelist"];
        var loadedCount = 0;

        for (int i = 0; i < questValueList.Count; i++)
        {
            if (TryParseQuestValue(questValueList[i], i, out var questData))
            {
                _questvaluelist.Add(questData);
                loadedCount++;
            }
        }

        Debug.Log($"진행 퀘스트 로드 완료: {loadedCount}/{questValueList.Count}개");
    }

    /// <summary>
    /// 퀘스트 값 데이터 파싱
    /// </summary>
    bool TryParseQuestValue(JsonData jsonData, int index, out St_UserQuestList questData)
    {
        questData = new St_UserQuestList();

        try
        {
            // 각 필드 파싱
            if (jsonData.ContainsKey("_questtype"))
                questData._questtype = (EQUESTTYPE)(int)jsonData["_questtype"];

            if (jsonData.ContainsKey("_questvaluetype"))
                questData._questvaluetype = (EQUESTVALUETYPE)(int)jsonData["_questvaluetype"];

            if (jsonData.ContainsKey("_targetid"))
                questData._targetid = (int)jsonData["_targetid"];

            if (jsonData.ContainsKey("_totalvalue"))
                questData._totalvalue = (int)jsonData["_totalvalue"];

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"퀘스트 값 [{index}] 파싱 실패: {ex.Message}");
            return false;
        }
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
            BackEndLog.WriteLog(LogType.QUEST, $"클리어 반복 퀘스트 번호:{questid}");
            return;
        }

        if (CheckQuestClear(questid))
        {
            //TODO: 같은 퀘스트를 여러번 완료하는 건 에러다
            Debug.LogError("여러번 클리어함! / 퀘스트 아이디에 문제있음!");
        }
        _questclearid.Add(questid);
        BackEndLog.WriteLog(LogType.QUEST, $"클리어 퀘스트 번호:{questid}");
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
                BackEndLog.WriteLog(LogType.QUEST, $"퀘스트 타입:{targetquesttype.ToString()} / 퀘스트 아이템 아이디: {targetid} / 획득 후:{userquest._totalvalue}");
                continue;
            }
            userquest = _questvaluelist[questindex];
            var beforevalue = userquest._totalvalue;
            userquest._totalvalue += targetvalue;
            _questvaluelist[questindex] = userquest;
            BackEndLog.WriteLog(LogType.QUEST, $"퀘스트 타입:{targetquesttype.ToString()} / 퀘스트 아이템 아이디: {targetid} / 획득 전:{beforevalue} / 획득 후:{userquest._totalvalue}");
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