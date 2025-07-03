using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static event Action _play_event;
    public static event Action _play_ready_event;
    public static Action _play_stage_allclear; //TODO: 모든 스테이지 클리어 처리 필요
    public static Action _play_stage_next; //TODO: 스테이지 클리어 처리 필요 
    public static Action<int, int> _play_stage_and_chapter_next; //TODO: 스테이지 클리어 처리 필요 
    public static Action<int, int> _play_stage_and_chapter_start; //TODO: 스테이지 클리어 처리 필요 
    public static Action _play_chapter_next; //TODO: 모든 챕터 생성 완료 
    public static Action _play_gameover; //TODO: 스테이지 실패 처리 
    public static PlayManager instance;

    [SerializeField] SO_PlayerPrefab _playerprefablist;
    public St_PlayerList GetHeroData(int heroid) => _playerprefablist.GetHeroList(heroid);
    [SerializeField] SO_ChapterData _chapterdata;
    [SerializeField] GameObject _gameover;
    public St_ChapterData GetCurrentChapterData() => _chapterdata.GetChapterData(_current_chapter_id);
    public SO_StageData GetCurrentStageData() => GetCurrentChapterData()._stagedata.Find(x => x._stageid == _current_stage_id);


    //챕터 및 스테이지 아이디
    protected int _current_chapter_id;
    protected int _current_stage_id;

    const int MAXSTAGECOUNT = 10;//하나의 챕터에 총 10개의 스테이지가 존재 


    void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        _play_stage_next += StageClear;
        _play_chapter_next += ChapterUpdate;
        _play_gameover += CreateGameOver;
    }

    void OnDisable()
    {
        _play_stage_next -= StageClear;
        _play_chapter_next -= ChapterUpdate;
        _play_gameover -= CreateGameOver;
    }

    void Start()
    {
        _current_chapter_id = UserData._userdata._chapterdata._lastchapternumber;
        _current_stage_id = 0;
        PlayGame().Forget();
    }

    async UniTaskVoid PlayGame()
    {
        await UniTask.WaitForEndOfFrame();
        _play_ready_event?.Invoke();
        _play_stage_and_chapter_start?.Invoke(_current_stage_id, MAXSTAGECOUNT);
        await UniTask.WaitForSeconds(1f, cancellationToken: this.GetCancellationTokenOnDestroy());
        _play_event?.Invoke();
    }

    void StageClear()
    {
        _current_stage_id++;
        _play_stage_and_chapter_next?.Invoke(_current_stage_id, MAXSTAGECOUNT);
    }

    void ChapterUpdate()
    {
        _current_chapter_id++;
        _current_stage_id = 0;
        _play_stage_and_chapter_next?.Invoke(_current_stage_id, MAXSTAGECOUNT);
    }

    void CreateGameOver()
    {
        var gameover = Instantiate(_gameover, null);

        //더 이상 챕터 정보가 없다면 클리어
        var currentchapterdata = _chapterdata.GetChapterData(_current_chapter_id);
        bool isclear = currentchapterdata._stagedata == null || currentchapterdata._stagedata.Count <= 0;
        gameover.GetComponent<UI_GameOver>().Init(_current_chapter_id, _current_stage_id, isclear);
    }
}