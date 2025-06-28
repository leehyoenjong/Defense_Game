using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static event Action _play_event;
    public static event Action _play_ready_event;
    public static Action _play_stage_allclear; //TODO: 모든 스테이지 클리어 처리 필요
    public static Action _play_stageclear; //TODO: 스테이지 클리어 처리 필요 
    public static Action _play_gameover; //TODO: 스테이지 실패 처리 
    public static PlayManager instance;

    [SerializeField] SO_PlayerPrefab _playerprefablist;
    public St_PlayerList GetHeroData(int heroid) => _playerprefablist.GetHeroList(heroid);
    [SerializeField] SO_ChapterData _chapterdata;
    [SerializeField] GameObject _gameover;
    public St_ChapterData GetCurrentChapterData() => _chapterdata.GetChapterData(_current_chapter_id);

    //챕터 및 스테이지 아이디
    public int _current_chapter_id;
    public int _current_stage_id;

    const int MAXSTAGECOUNT = 10;//하나의 챕터에 총 10개의 스테이지가 존재 


    void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        _play_stageclear += StageClear;
        _play_stageclear += CreateGameOver;
        _play_gameover += CreateGameOver;
    }

    void OnDisable()
    {
        _play_stageclear -= StageClear;
        _play_stageclear -= CreateGameOver;
        _play_gameover -= CreateGameOver;
    }

    void Start()
    {
        //TODO: 선택한 챕터 정보를 넘겨받을 수 있도록 해야함
        _current_chapter_id = 0;
        _current_stage_id = 0;
        PlayGame().Forget();
    }

    async UniTaskVoid PlayGame()
    {
        await UniTask.WaitForEndOfFrame();
        _play_ready_event?.Invoke();
        await UniTask.WaitForSeconds(1f, cancellationToken: this.GetCancellationTokenOnDestroy());
        _play_event?.Invoke();
    }

    void StageClear()
    {
        _current_stage_id++;

        if (_current_stage_id >= MAXSTAGECOUNT)
        {
            _current_stage_id = 0;
            _current_chapter_id++;
        }
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