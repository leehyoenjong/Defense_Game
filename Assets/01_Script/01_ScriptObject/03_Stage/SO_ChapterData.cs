using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_ChapterData", menuName = "SO_ChapterData", order = 0)]
public class SO_ChapterData : ScriptableObject
{
    [SerializeField] List<St_ChapterData> _chapterdata;

    public St_ChapterData GetChapterData(int chapterid)
    {
        var chapterdata = _chapterdata.Find(x => x._chapterid == chapterid);
        return chapterdata;
    }
}

[Serializable]
public struct St_ChapterData
{
    public int _chapterid;
    public List<SO_StageData> _stagedata;
}