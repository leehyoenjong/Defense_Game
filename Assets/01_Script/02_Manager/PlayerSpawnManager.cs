using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager instance;
    [SerializeField] Transform[] _playerpoint;
    List<BaseNPC> _herolist = new List<BaseNPC>();
    public List<BaseNPC> GetHeroList() => _herolist;

    void Awake()
    {
        instance = this;
    }

    void OnEnable()
    {
        PlayManager._ongamestatechanged += CreatePlayer;
        BaseSkill._skill_target_dictionary_list.Add(ETARGETKIND.HERO, _herolist);
    }

    void OnDisable()
    {
        PlayManager._ongamestatechanged -= CreatePlayer;
        BaseSkill._skill_target_dictionary_list.Remove(ETARGETKIND.HERO);
    }

    void CreatePlayer(GameStateData stateData)
    {
        if (stateData._state != EPLAYSTATE.READY)
        {
            return;
        }

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
            if (player._npc._mid == 0)
            {
                continue;
            }

            var createpoint = _playerpoint[i];
            var createhero = Instantiate<GameObject>(player._npc._mybodyobject);

            var hero = createhero.GetComponent<Hero_Base>();
            hero.IDSetting(itemdata._connecttableid);
            hero.OnSpawn(createpoint.position);
            _herolist.Add(hero);
        }
    }
}
