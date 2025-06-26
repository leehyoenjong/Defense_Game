using System;
using UnityEngine;

public class UI_Status : MonoBehaviour
{
    [SerializeField] UI_Btn_Status_Upgrade[] _btnstatusupgrades;
    public static Func<int> _heroidx;

    void OnEnable()
    {
        var maxcount = _btnstatusupgrades.Length;

        for (int i = 0; i < maxcount; i++)
        {
            _btnstatusupgrades[i].Init(_heroidx.Invoke());
        }
    }
}