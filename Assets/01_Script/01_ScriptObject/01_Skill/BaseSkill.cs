using System;
using UnityEngine;

public abstract class BaseSkill : ScriptableObject
{

    [Header("스킬 정보")]
    public int _level;
    public St_SkillInfo _skillInfo;
    public ESKILLKIND _skillkind;

    [Header("실행할 애니메이션")]
    public EANIMATION _eanimation = EANIMATION.NONE;
    [Header("발동시킬 트리거")]
    public ESKILLTRIGGER _eskilltrigger;
    //발동할 애니메이션
    [Header("내 위치에 나타나는 이펙트 오브젝트")]
    public GameObject[] _active_skillEffect;

    public const int MAXLEVEL = 3;

    /// <summary>
    /// <summary>
    /// 내 위치에 이펙트 생성
    /// </summary>
    /// <param name="myposition"></param>

    //스킬 발동 시 발동자 나오는 이펙트
    public void ActiveSkillEffectToTarget(Vector3 myposition)
    {
        var maxcount = _active_skillEffect.Length;
        for (int i = 0; i < maxcount; i++)
        {
            var mypositioneffect = Instantiate<GameObject>(_active_skillEffect[i], myposition, default);
        }
    }

    public void ActiveSkillPlayAnimation(BaseNPC me)
    {
        me.PlayAnimation(_eanimation);
    }
}

[Serializable]
public struct St_SkillInfo
{
    public float _cooltime;
    public float _duration;
    public int _mid;

    //이 스킬 사용 후 다음 스킬 사용까지 딜레이 시간
    public float _next_skilldelaytime;
}

public enum ESKILLKIND
{
    NONE,
    ATTACK,
    BUFF
}