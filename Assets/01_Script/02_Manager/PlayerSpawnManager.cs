using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] SO_PlayerPrefab _playerprefablist;
    [SerializeField] Transform[] _playerpoint;
    [SerializeField] List<St_UserPlayerList> _playeridlist;//TODO: 추후 유저 데이터로 저장할 것

    void OnEnable()
    {
        PlayManager._play_ready_event += CreatePlayer;
    }

    void OnDisable()
    {
        PlayManager._play_ready_event -= CreatePlayer;
    }

    void CreatePlayer()
    {
        var maxcount = _playeridlist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            var player = _playerprefablist.GetPlayerList(_playeridlist[i]._playerid);
            if (player._player_id == 0)
            {
                return;
            }

            var createpoint = _playerpoint[_playeridlist[i]._playerpoint];
            var createplayer = Instantiate<GameObject>(player._playerobject);
            createplayer.GetComponent<Player_Base>().OnSpawn(createpoint.position);
        }
    }
}

[Serializable]
public struct St_UserPlayerList
{
    public int _playerid;
    public int _playerpoint;
}