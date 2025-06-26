using System;
using TMPro;
using UnityEngine;

public class UI_Btn_Status_Upgrade : MonoBehaviour
{
    [SerializeField] ESTATUSUPGRADE _estatusupgradekind;

    [SerializeField] TextMeshProUGUI _level;

    [SerializeField] TextMeshProUGUI _statusvalue;

    public static event Action<int, ESTATUSUPGRADE> _statusupgrade_event;
    int HeroListIDX;

    public void Init(int idx)
    {
        HeroListIDX = idx;
        LevelAndValueSetting();
    }

    public void Btn_StatusUpgrade()
    {
        _statusupgrade_event?.Invoke(HeroListIDX, _estatusupgradekind);
        LevelAndValueSetting();
    }

    void LevelAndValueSetting()
    {
        var statusresult = StatusUpgradeManager.instance.GetStatusUpgrade(HeroListIDX, _estatusupgradekind);
        _statusvalue.text = _estatusupgradekind + ": " + statusresult.values.ToString("F1");
        _level.text = "Lv. " + statusresult.level.ToString();
    }


}