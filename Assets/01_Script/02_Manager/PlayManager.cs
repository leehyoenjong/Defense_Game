using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static PlayManager instance;

    /// <summary>
    /// 통합된 게임 상태 변경 이벤트
    /// </summary>
    public static event Action<GameStateData> _ongamestatechanged;
    [SerializeField] GameObject _gameover;

    //챕터 및 스테이지 아이디
    protected int _current_chapter_id;
    public int GetCurrentChapterID() => _current_chapter_id;
    protected int _current_stage_id;
    public int GetCurrentStageID() => _current_stage_id;

    const int MAXSTAGECOUNT = 10;//하나의 챕터에 총 10개의 스테이지가 존재 


    void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        _ongamestatechanged += HandleGameStateChange;
    }

    void OnDisable()
    {
        _ongamestatechanged -= HandleGameStateChange;
    }

    void Start()
    {
        _current_chapter_id = UserData._userdata._userchapterdata._lastchapternumber;
        _current_stage_id = 0;
        PlayGame().Forget();
    }

    /// <summary>
    /// 통합된 게임 상태 변경 핸들러
    /// </summary>
    void HandleGameStateChange(GameStateData stateData)
    {
        switch (stateData._state)
        {
            case EPLAYSTATE.STAGE_NEXT:
                StageClear();
                break;
            case EPLAYSTATE.CHAPTER_NEXT:
                ChapterUpdate();
                break;
            case EPLAYSTATE.GAMEOVER:
                CreateGameOver();
                break;
        }
    }

    async UniTaskVoid PlayGame()
    {
        await UniTask.WaitForEndOfFrame(cancellationToken: this.GetCancellationTokenOnDestroy());

        // 게임 준비 상태
        _ongamestatechanged?.Invoke(new GameStateData(EPLAYSTATE.READY));

        // 스테이지/챕터 시작
        _ongamestatechanged?.Invoke(new GameStateData(EPLAYSTATE.STAGE_START, _current_stage_id, MAXSTAGECOUNT, _current_chapter_id));

        await UniTask.WaitForSeconds(1f, cancellationToken: this.GetCancellationTokenOnDestroy());

        // 게임 플레이 시작
        _ongamestatechanged?.Invoke(new GameStateData(EPLAYSTATE.PLAY));
    }

    void StageClear()
    {
        _current_stage_id++;
        _ongamestatechanged?.Invoke(new GameStateData(EPLAYSTATE.STAGE_NEXT, _current_stage_id, MAXSTAGECOUNT, _current_chapter_id));
    }

    void ChapterUpdate()
    {
        _current_chapter_id++;
        _current_stage_id = 0;
        _ongamestatechanged?.Invoke(new GameStateData(EPLAYSTATE.CHAPTER_START, _current_stage_id, MAXSTAGECOUNT, _current_chapter_id));
    }

    void CreateGameOver()
    {
        var gameover = Instantiate(_gameover, null);

        //더 이상 챕터 정보가 없다면 클리어
        var currentchapterdata = DataManager.instance.GetChapterData(_current_chapter_id);
        bool isclear = currentchapterdata._stagedata == null || currentchapterdata._stagedata.Count <= 0;
        gameover.GetComponent<UI_GameOver>().Init(_current_chapter_id, _current_stage_id, isclear);
    }

    /// <summary>
    /// 외부에서 게임 상태 변경을 요청할 때 사용하는 메서드들 (하위 호환성)
    /// </summary>
    public static void TriggerStageNext() => _ongamestatechanged?.Invoke(new GameStateData(EPLAYSTATE.STAGE_NEXT));
    public static void TriggerChapterNext() => _ongamestatechanged?.Invoke(new GameStateData(EPLAYSTATE.CHAPTER_NEXT));
    public static void TriggerGameOver() => _ongamestatechanged?.Invoke(new GameStateData(EPLAYSTATE.GAMEOVER));
}