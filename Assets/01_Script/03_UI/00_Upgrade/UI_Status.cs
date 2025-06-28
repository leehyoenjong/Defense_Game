using System;
using UnityEngine;

public class UI_Status : MonoBehaviour
{
    [SerializeField] UI_Btn_Status_Upgrade[] _btnstatusupgrades;
    public static Func<Player_Base> _heroclass;
    public static event Action _status_disable_event;

    void OnEnable()
    {
        var maxcount = _btnstatusupgrades.Length;

        for (int i = 0; i < maxcount; i++)
        {
            _btnstatusupgrades[i].Init(_heroclass.Invoke());
        }
    }

    void OnDisable()
    {
        _status_disable_event?.Invoke();
    }
}