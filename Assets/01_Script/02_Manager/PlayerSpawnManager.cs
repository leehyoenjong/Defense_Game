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
        var userherodata = UserData._userdata._userequiphero;
        var maxcount = userherodata.GetEquipHeroList().Count;
        for (int i = 0; i < maxcount; i++)
        {
            var heroitemid = userherodata.GetEquipHeroList()[i];
            if (heroitemid <= 0)
            {
                continue;
            }

            if (DataManager.instance.GetItemTable().GetItemdata().TryGetValue(heroitemid, out var itemdata) == false)
            {
                continue;
            }

            var player = DataManager.instance.GetHeroData(itemdata._connecttableid);
            if (player._player_id == 0)
            {
                continue;
            }

            var createpoint = _playerpoint[i];
            var createhero = Instantiate<GameObject>(player._playerobject);

            var hero = createhero.GetComponent<Player_Base>();
            hero.IDSetting(itemdata._connecttableid);
            hero.OnSpawn(createpoint.position);
            _herolist.Add(hero);
        }
    }
}
