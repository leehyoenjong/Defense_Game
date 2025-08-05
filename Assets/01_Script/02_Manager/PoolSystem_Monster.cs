using System.Collections.Generic;
using UnityEngine;

public class PoolSystem_Monster : MonoBehaviour
{
    public static PoolSystem_Monster instance;
    Dictionary<int, Queue<Monster_Base>> _mon = new Dictionary<int, Queue<Monster_Base>>();

    void Awake()
    {
        instance = this;
        Monster_Base._monster_die_animation_exit += ReleseMonster;
    }

    public Monster_Base GetMonsterObject(int monid)
    {
        var createmoninfo = DataManager.instance.GetMonsterInfo(monid);
        if (createmoninfo._npc == null)
        {
            return null;
        }

        if (_mon.ContainsKey(monid) == false)
        {
            _mon.Add(monid, new Queue<Monster_Base>());
        }

        if (_mon[monid].Count > 0)
        {
            return _mon[monid].Dequeue();
        }

        var mon = Instantiate(createmoninfo._npc._mybodyobject, null).GetComponent<Monster_Base>();
        return mon;
    }

    void ReleseMonster(Monster_Base mon)
    {
        if (_mon.ContainsKey(mon.GetID()) == false)
        {
            _mon.Add(mon.GetID(), new Queue<Monster_Base>());
        }

        _mon[mon.GetID()].Enqueue(mon);
        mon.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        _mon.Clear();
        instance = null;
    }
}