using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager instance;
    [SerializeField] Transform[] _playerpoint;
    List<Player_Base> _herolist = new List<Player_Base>();
    public List<Player_Base> GetHeroList() => _herolist;

    void Awake()
    {
        instance = this;
    }

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
        var userherodata = UserData._userdata._userherodata;
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

            var hero = createhero.GetComponent<Player_Base>();
            hero.OnSpawn(createpoint.position);
            hero.IDSetting(userherodata[i]._heroid);
            _herolist.Add(hero);
        }
    }
}
