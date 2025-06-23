using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static event Action<int> _gold_get_event;
    public static Action<int> _gold_add_event;
    int _current_gold;

    void Start()
    {
        _gold_add_event = AddGold;
    }

    void OnDisable()
    {
        _gold_add_event = null;
    }

    void AddGold(int addgold)
    {
        _current_gold += addgold;
        _gold_get_event?.Invoke(_current_gold);
    }
}