using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] Transform[] _playerpoint;

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
        var userherodata = GameManger._userdata._userherodata;
        var maxcount = userherodata.Length;
        for (int i = 0; i < maxcount; i++)
        {
            var player = PlayManager.instance.GetHeroData(userherodata[i]._heroid);
            if (player._player_id == 0)
            {
                return;
            }

            var createpoint = _playerpoint[userherodata[i]._heropoint];
            var createhero = Instantiate<GameObject>(player._playerobject);
            createhero.GetComponent<Player_Base>().OnSpawn(createpoint.position);
        }
    }
}
