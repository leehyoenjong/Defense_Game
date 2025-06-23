using System;
using TMPro;
using UnityEngine;

public class UI_Btn_Status_Upgrade : MonoBehaviour
{
    [SerializeField] ESTATUSUPGRADE _estatusupgradekind;

    [SerializeField] TextMeshProUGUI _level;

    [SerializeField] TextMeshProUGUI _statusvalue;

    public static event Action<ESTATUSUPGRADE> _statusupgrade_event;
    void Start()
    {
        LevelAndValueSetting();
    }

    public void Btn_StatusUpgrade()
    {
        _statusupgrade_event?.Invoke(_estatusupgradekind);
        LevelAndValueSetting();
    }

    void LevelAndValueSetting()
    {
        var statusresult = StatusUpgradeManager.instance.GetStatusUpgrade(_estatusupgradekind);
        _statusvalue.text = _estatusupgradekind + ": " + statusresult.values.ToString("F1");
        _level.text = "Lv. " + statusresult.level.ToString();
    }


}