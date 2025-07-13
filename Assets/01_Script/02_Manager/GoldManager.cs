using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static event Action<int> _gold_get_event;

    int _current_gold;

    void Start()
    {
        Monster_Base._monsterdie += AddGold;
    }

    void OnDisable()
    {
        Monster_Base._monsterdie -= AddGold;
    }

    void AddGold(St_MonsterTable monsterinfo)
    {
        _current_gold += monsterinfo._diegold;
        _gold_get_event?.Invoke(_current_gold);
    }
}