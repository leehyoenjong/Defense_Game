using BackEnd;
using UnityEngine;

public class BackEndLog : MonoBehaviour
{
    public static void WriteLog(LogType logtype, string message)
    {
        Param param = new Param();
        param.Add(logtype.ToString(), message);
        Debug.Log($"로그 실행");
        Backend.GameLog.InsertLogV2(logtype.ToString(), param, (callback) =>
        {
            Debug.Log($"로그 여부 :{callback.IsSuccess()}");
        });
    }
}

public enum LogType
{
    INVENTORY,
    QUEST,
    EQUIP,
    CHAPTER,
    POST
}