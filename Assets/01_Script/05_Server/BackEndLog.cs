using BackEnd;
using UnityEngine;

public class BackEndLog : MonoBehaviour
{
    public static void WriteLog(LogType logtype, string message)
    {
        Param param = new Param();
        param.Add(message);
        Backend.GameLog.InsertLogV2(logtype.ToString(), param, (callback) => { });
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