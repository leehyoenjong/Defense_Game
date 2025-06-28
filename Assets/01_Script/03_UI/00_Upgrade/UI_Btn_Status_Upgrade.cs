using System;
using TMPro;
using UnityEngine;

public class UI_Btn_Status_Upgrade : MonoBehaviour
{
    [SerializeField] ESTATUSUPGRADE _estatusupgradekind;

    [SerializeField] TextMeshProUGUI _level;

    [SerializeField] TextMeshProUGUI _statusvalue;

    public static event Action<int, ESTATUSUPGRADE> _statusupgrade_event;
    const float ACTIVEDEALYTIME = 0.2f;//꾸욱 누를때 딜레이
    const float POINTUPDEALYTIME = 1f;//첫 딜레이
    Player_Base _heroclass;
    bool _ispointdown;
    float _delaytime;

    void Update()
    {
        if (_ispointdown == false)
        {
            return;
        }

        _delaytime -= Time.deltaTime;

        if (_delaytime > 0)
        {
            return;
        }

        _delaytime = ACTIVEDEALYTIME;
        ActiveUpgarde();
    }

    public void Init(Player_Base heroclass)
    {
        this._heroclass = heroclass;
        LevelAndValueSetting();
    }

    void LevelAndValueSetting()
    {
        var statusresult = StatusUpgradeManager.instance.GetStatusUpgrade(_heroclass.GetID(), _estatusupgradekind);
        _statusvalue.text = _estatusupgradekind + ": " + statusresult.values.ToString("F1");
        _level.text = "Lv. " + statusresult.level.ToString();
    }

    public void Btn_EvetnTrigger_PointDown()
    {
        _delaytime = POINTUPDEALYTIME;
        _ispointdown = true;
        ActiveUpgarde();
    }

    public void Btn_EventTrigger_PointUp()
    {
        _ispointdown = false;
    }

    void ActiveUpgarde()
    {
        _statusupgrade_event?.Invoke(_heroclass.GetID(), _estatusupgradekind);
        LevelAndValueSetting();
    }
}