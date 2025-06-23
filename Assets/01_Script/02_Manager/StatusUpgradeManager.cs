using System.Collections.Generic;
using UnityEngine;

public class StatusUpgradeManager : MonoBehaviour
{
    public static StatusUpgradeManager instance;

    Dictionary<ESTATUSUPGRADE, int> _statusupgrade = new Dictionary<ESTATUSUPGRADE, int>();

    [SerializeField]
    List<float> _maxlevelvalue = new List<float>()
    {
        0,
        1000,//공격력 퍼센트
        1,   //크리티컬 퍼센트
        1,   //크리티컬 데미지 퍼센트
        1000,//보호 오브젝트 최대 HP 퍼센트
        100  //보호 오브젝트 최대 방어력(상수)
    };


    const int MAXCOINVALUE = 100000;//최대 강화 총 비용
    const int MAXLEVEL = 100;       //최대 레벨 

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UI_Btn_Status_Upgrade._statusupgrade_event += StatusUpgrade;
    }

    public (int level, float values) GetStatusUpgrade(ESTATUSUPGRADE estatusupgrade)
    {
        if (_statusupgrade.ContainsKey(estatusupgrade) == false)
        {
            _statusupgrade.Add(estatusupgrade, 0);
        }
        var curlevel = _statusupgrade[estatusupgrade];

        if (curlevel <= 0)
        {
            return (0, 0);
        }

        //최대와 현재 레벨과 최대 상승값을 이용해 일정한 값이 오르도록 수식작성
        float percentPerLevel = _maxlevelvalue[(int)estatusupgrade] / (MAXLEVEL - 1);
        return (curlevel, percentPerLevel * (curlevel - 1));
    }

    void StatusUpgrade(ESTATUSUPGRADE estatusupgrade)
    {
        if (_statusupgrade.ContainsKey(estatusupgrade) == false)
        {
            _statusupgrade.Add(estatusupgrade, 0);
        }

        var nextlevel = _statusupgrade[estatusupgrade] + 1;
        if (UpgradeCointSetting(nextlevel) == false)
        {
            //TODO: 돈 없다는 팝업 띄우기
            return;
        }
        _statusupgrade[estatusupgrade]++;
    }

    //TODO: 추후 데이터 테이블을 이용해서 비용 할 수 있도록 개선 필요
    bool UpgradeCointSetting(int nextlevel)
    {
        var nextlevelcoinvalue = MAXCOINVALUE / MAXLEVEL;
        var currentupgradecoinvalue = nextlevelcoinvalue * nextlevel;

        //TODO: 유저 돈 가져와서 처리할 것 
        var usercoin = 0;
        if (currentupgradecoinvalue > usercoin)
        {
            return false;
        }

        usercoin -= currentupgradecoinvalue;
        return true;
    }
}

public enum ESTATUSUPGRADE
{
    NONE,
    ATTACKPER,
    CIRITICALPER,
    CIRITICALDAMAGE,
    PROTECTMAXHPPER,
    PROTECTARMOR
}