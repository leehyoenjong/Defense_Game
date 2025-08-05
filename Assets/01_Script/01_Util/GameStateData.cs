
public enum EPLAYSTATE
{
    READY,
    PLAY,
    STAGE_NEXT,
    STAGE_START,
    CHAPTER_NEXT,
    CHAPTER_START,
    GAMEOVER,
}

/// <summary>
/// 게임 상태 변경 시 전달되는 데이터
/// </summary>
[System.Serializable]
public struct GameStateData
{
    public EPLAYSTATE _state;
    public int _currentstage;
    public int _maxstagecount;
    public int _currentchapter;

    public GameStateData(EPLAYSTATE state, int currentStage = 0, int maxStageCount = 0, int currentChapter = 0)
    {
        _state = state;
        _currentstage = currentStage;
        _maxstagecount = maxStageCount;
        _currentchapter = currentChapter;
    }
}
