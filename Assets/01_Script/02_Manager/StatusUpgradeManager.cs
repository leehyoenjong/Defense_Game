using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusUpgradeManager : MonoBehaviour
{
    public static StatusUpgradeManager instance;

    Dictionary<int, Dictionary<ESTATUSUPGRADE, int>> _statusupgrade = new Dictionary<int, Dictionary<ESTATUSUPGRADE, int>>();
    public static event Action<int> _statusupgrade_event;

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
        _statusupgrade.Clear();
    }

    void OnEnable()
    {
        UI_Btn_Status_Upgrade._statusupgrade_event += StatusLvUpgrade;
    }

    void OnDisable()
    {
        UI_Btn_Status_Upgrade._statusupgrade_event -= StatusLvUpgrade;
    }


    public (int level, float values) GetStatusUpgrade(int heroid, ESTATUSUPGRADE estatusupgrade)
    {
        if (_statusupgrade.ContainsKey(heroid) == false)
        {
            _statusupgrade.Add(heroid, new Dictionary<ESTATUSUPGRADE, int>());
        }
        if (_statusupgrade[heroid].ContainsKey(estatusupgrade) == false)
        {
            _statusupgrade[heroid].Add(estatusupgrade, 0);
        }

        var curlevel = _statusupgrade[heroid][estatusupgrade];

        if (curlevel <= 0)
        {
            return (0, 0);
        }

        //최대와 현재 레벨과 최대 상승값을 이용해 일정한 값이 오르도록 수식작성
        float percentPerLevel = _maxlevelvalue[(int)estatusupgrade] / (MAXLEVEL - 1);
        return (curlevel, percentPerLevel * (curlevel - 1));
    }

    public (int level, float values) GetStatusBeforeUpgrade(int heroid, ESTATUSUPGRADE estatusupgrade)
    {
        if (_statusupgrade.ContainsKey(heroid) == false)
        {
            _statusupgrade.Add(heroid, new Dictionary<ESTATUSUPGRADE, int>());
        }
        if (_statusupgrade[heroid].ContainsKey(estatusupgrade) == false)
        {
            _statusupgrade[heroid].Add(estatusupgrade, 0);
        }

        var curlevel = _statusupgrade[heroid][estatusupgrade];
        curlevel--;

        if (curlevel <= 0)
        {
            return (0, 0);
        }

        //최대와 현재 레벨과 최대 상승값을 이용해 일정한 값이 오르도록 수식작성
        float percentPerLevel = _maxlevelvalue[(int)estatusupgrade] / (MAXLEVEL - 1);
        return (curlevel, percentPerLevel * (curlevel - 1));
    }

    void StatusLvUpgrade(int heroid, ESTATUSUPGRADE estatusupgrade)
    {
        if (_statusupgrade.ContainsKey(heroid) == false)
        {
            _statusupgrade.Add(heroid, new Dictionary<ESTATUSUPGRADE, int>());
        }
        if (_statusupgrade[heroid].ContainsKey(estatusupgrade) == false)
        {
            _statusupgrade[heroid].Add(estatusupgrade, 0);
        }

        var nextlevel = _statusupgrade[heroid][estatusupgrade] + 1;
        if (UpgradeCointSetting(nextlevel) == false)
        {
            //TODO: 돈 없다는 팝업 띄우기
            return;
        }
        _statusupgrade[heroid][estatusupgrade]++;
        _statusupgrade_event?.Invoke(heroid);
    }

    //TODO: 추후 데이터 테이블을 이용해서 비용 할 수 있도록 개선 필요
    bool UpgradeCointSetting(int nextlevel)
    {
        return true;
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

    /// <summary>
    /// 업그레이드 차이값을 계산하여 반환 (새로운 값 - 이전 값)
    /// </summary>
    public St_Status GetStatusUpgradeDifference(int heroid)
    {
        var heroobejct = PlayerSpawnManager.instance.GetHeroList().Find(x => x.GetID() == heroid);
        var baseStatus = heroobejct.GetStatus();

        var differenceStatus = new St_Status();

        // 이전과 현재 업그레이드 값 가져오기
        var beforeAttack = GetStatusBeforeUpgrade(heroid, ESTATUSUPGRADE.ATTACKPER);
        var currentAttack = GetStatusUpgrade(heroid, ESTATUSUPGRADE.ATTACKPER);

        var beforeCritical = GetStatusBeforeUpgrade(heroid, ESTATUSUPGRADE.CRITICALPER);
        var currentCritical = GetStatusUpgrade(heroid, ESTATUSUPGRADE.CRITICALPER);

        var beforeCriticalDamage = GetStatusBeforeUpgrade(heroid, ESTATUSUPGRADE.CRITICALDAMAGE);
        var currentCriticalDamage = GetStatusUpgrade(heroid, ESTATUSUPGRADE.CRITICALDAMAGE);

        // 공격력 차이 계산
        int beforeAttackValue = beforeAttack.values > 0 ?
            Mathf.FloorToInt(baseStatus._damge * (beforeAttack.values / 100f)) : 0;
        int currentAttackValue = currentAttack.values > 0 ?
            Mathf.FloorToInt(baseStatus._damge * (currentAttack.values / 100f)) : 0;
        differenceStatus._damge = currentAttackValue - beforeAttackValue;

        // 크리티컬 확률 차이 계산
        float beforeCriticalValue = beforeCritical.values > 0 ? (beforeCritical.values / 100f) : 0;
        float currentCriticalValue = currentCritical.values > 0 ? (currentCritical.values / 100f) : 0;
        differenceStatus._critical = currentCriticalValue - beforeCriticalValue;

        // 크리티컬 데미지 차이 계산
        float beforeCriticalDamageValue = beforeCriticalDamage.values > 0 ? (beforeCriticalDamage.values / 100f) : 0;
        float currentCriticalDamageValue = currentCriticalDamage.values > 0 ? (currentCriticalDamage.values / 100f) : 0;
        differenceStatus._critical_damage = currentCriticalDamageValue - beforeCriticalDamageValue;

        return differenceStatus;
    }

    /// <summary>
    /// 현재 업그레이드 값을 St_Status로 반환
    /// </summary>
    public St_Status GetStatusUpgradeAsStatus(int heroid)
    {
        var heroobejct = PlayerSpawnManager.instance.GetHeroList().Find(x => x.GetID() == heroid);
        var baseStatus = heroobejct.GetStatus();

        var upgradeStatus = new St_Status();

        // 각 업그레이드 타입별로 처리
        var attackUpgrade = GetStatusUpgrade(heroid, ESTATUSUPGRADE.ATTACKPER);
        var criticalUpgrade = GetStatusUpgrade(heroid, ESTATUSUPGRADE.CRITICALPER);
        var criticalDamageUpgrade = GetStatusUpgrade(heroid, ESTATUSUPGRADE.CRITICALDAMAGE);

        // 공격력: 기본값에 퍼센트 적용
        if (attackUpgrade.values > 0)
        {
            upgradeStatus._damge = Mathf.FloorToInt(baseStatus._damge * (attackUpgrade.values / 100f));
        }

        // 크리티컬 확률: 퍼센트 값 그대로 적용
        if (criticalUpgrade.values > 0)
        {
            upgradeStatus._critical = criticalUpgrade.values / 100f;
        }

        // 크리티컬 데미지: 퍼센트 값 그대로 적용
        if (criticalDamageUpgrade.values > 0)
        {
            upgradeStatus._critical_damage = criticalDamageUpgrade.values / 100f;
        }

        return upgradeStatus;
    }

    /// <summary>
    /// 이전 업그레이드 값을 St_Status로 반환
    /// </summary>
    public St_Status GetStatusBeforeUpgradeAsStatus(int heroid)
    {
        // PlayManager에서 기본 헤로 데이터 가져오기
        var heroData = DataManager.instance.GetHeroData(heroid);
        var heroObject = heroData._npc._mybodyobject;
        var baseNPC = heroObject.GetComponent<BaseNPC>();
        var baseStatus = baseNPC.GetStatus();

        var beforeUpgradeStatus = new St_Status();

        // 각 업그레이드 타입별로 처리
        var beforeAttack = GetStatusBeforeUpgrade(heroid, ESTATUSUPGRADE.ATTACKPER);
        var beforeCritical = GetStatusBeforeUpgrade(heroid, ESTATUSUPGRADE.CRITICALPER);
        var beforeCriticalDamage = GetStatusBeforeUpgrade(heroid, ESTATUSUPGRADE.CRITICALDAMAGE);

        // 공격력: 기본값에 퍼센트 적용
        if (beforeAttack.values > 0)
        {
            beforeUpgradeStatus._damge = Mathf.FloorToInt(baseStatus._damge * (beforeAttack.values / 100f));
        }

        // 크리티컬 확률: 퍼센트 값 그대로 적용
        if (beforeCritical.values > 0)
        {
            beforeUpgradeStatus._critical = beforeCritical.values / 100f;
        }

        // 크리티컬 데미지: 퍼센트 값 그대로 적용
        if (beforeCriticalDamage.values > 0)
        {
            beforeUpgradeStatus._critical_damage = beforeCriticalDamage.values / 100f;
        }

        return beforeUpgradeStatus;
    }
}
public enum ESTATUSUPGRADE
{
    NONE,
    ATTACKPER,
    CRITICALPER,
    CRITICALDAMAGE,
    PROTECTMAXHPPER,
    PROTECTARMOR
}