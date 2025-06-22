using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static event Action _play_event;
    public static event Action _play_ready_event;
    public static Action _player_end; //TODO: 모든 스테이지 클리어 처리 필요
    public static Action _play_stageclear; //TODO: 스테이지 클리어 처리 필요 
    public static PlayManager instance;

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
    }

    void OnDisable()
    {
        _play_stageclear -= StageClear;
    }

    void Start()
    {
        _current_chapter_id = 0;
        _current_stage_id = 0;
        PlayGame().Forget();
    }

    async UniTaskVoid PlayGame()
    {
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
}