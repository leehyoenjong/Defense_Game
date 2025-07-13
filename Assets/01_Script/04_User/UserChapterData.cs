using System;

[Serializable]
public struct St_UserChapterData
{
    public int _lastchapternumber;

    public void UserChapterUpdate()
    {
        _lastchapternumber++;
    }
}