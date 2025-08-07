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
    BaseNPC _heroclass;
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

    public void Init(BaseNPC heroclass)
    {
        this._heroclass = heroclass;
        LevelAndValueSetting();
    }

    void LevelAndValueSetting()
    {
        var heroStatus = _heroclass.GetStatus();
        var upgradeLevel = StatusUpgradeManager.instance.GetStatusUpgrade(_heroclass.GetID(), _estatusupgradekind);

        float statusValue = 0f;
        string statusText = "";

        // _estatusupgradekind에 따라 해당하는 status 값 가져오기
        switch (_estatusupgradekind)
        {
            case ESTATUSUPGRADE.ATTACKPER:
                statusValue = heroStatus._damge;
                statusText = _estatusupgradekind + ": " + statusValue.ToString("F1");
                break;
            case ESTATUSUPGRADE.CRITICALPER:
                statusValue = heroStatus._critical * 100; // 퍼센트로 표시
                statusText = _estatusupgradekind + ": " + statusValue.ToString("F1") + "%";
                break;
            case ESTATUSUPGRADE.CRITICALDAMAGE:
                statusValue = heroStatus._critical_damage * 100; // 퍼센트로 표시
                statusText = _estatusupgradekind + ": " + statusValue.ToString("F1") + "%";
                break;
            case ESTATUSUPGRADE.PROTECTMAXHPPER:
                statusValue = heroStatus._hp;
                statusText = _estatusupgradekind + ": " + statusValue.ToString("F1");
                break;
            case ESTATUSUPGRADE.PROTECTARMOR:
                statusValue = heroStatus._armor;
                statusText = _estatusupgradekind + ": " + statusValue.ToString("F1");
                break;
        }

        statusText += $"\nCOST {StatusUpgradeManager.instance.GetSellValue(_heroclass.GetID(), _estatusupgradekind)}";
        _statusvalue.text = statusText;
        _level.text = "Lv. " + upgradeLevel.level.ToString();
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