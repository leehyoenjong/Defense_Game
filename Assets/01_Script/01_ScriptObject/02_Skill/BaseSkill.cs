using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseSkill : ScriptableObject
{

    [Header("스킬 정보")]
    public St_SkillInfo _skillInfo;
    public ESKILLKIND _skillkind;

    [Header("실행할 애니메이션")]
    public EANIMATION _eanimation = EANIMATION.NONE;
    [Header("발동시킬 트리거")]
    public ESKILLTRIGGER _eskilltrigger;
    //발동할 애니메이션
    [Header("내 위치에 나타나는 이펙트 오브젝트")]
    [SerializeField] protected List<GameObject> _active_skillEffect;


    [Header("ㅡㅡㅡㅡ발동 타입ㅡㅡㅡㅡ")]
    public EUSETYPE _eusetype;

    [Header("닿으면 데미지 닳는 오브젝트 (OBJECT 사용하는 모드에서만 사용)")]
    [ConditionalField("_eusetype", true, (int)EUSETYPE.OBJECT_ENTER, (int)EUSETYPE.OBJECT_VIEW)]
    public List<GameObject> _enter_hit_object;

    [Header("ㅡㅡㅡㅡ타겟 종류ㅡㅡㅡㅡ")]
    public ETARGETKIND _etargetkind;
    [Header("ㅡㅡㅡㅡ타겟 필터 타입ㅡㅡㅡㅡ")]
    public ETARGETFILTERTYPE _etargetfiltertype;
    [Header("ㅡㅡㅡㅡ범위 타입ㅡㅡㅡㅡ")]
    public ESKILLAREA _eskillarea;

    [Header("ㅡㅡㅡㅡ범위 설정ㅡㅡㅡㅡ")]

    //원형 범위
    [ConditionalField(ConditionalFieldAttribute.ConditionOperator.And, true, "_eskillarea", (int)ESKILLAREA.CIRCLE, "_eusetype", (int)EUSETYPE.OBJECT_ENTER)]
    public float _circleRadius = 1f;

    //박스 범위
    [ConditionalField(ConditionalFieldAttribute.ConditionOperator.And, true, "_eskillarea", (int)ESKILLAREA.BOX, "_eusetype", (int)EUSETYPE.OBJECT_ENTER)]
    public float _boxWidth = 1f;

    [ConditionalField(ConditionalFieldAttribute.ConditionOperator.And, true, "_eskillarea", (int)ESKILLAREA.BOX, "_eusetype", (int)EUSETYPE.OBJECT_ENTER)]
    public float _boxHeight = 1f;
    public static Dictionary<ETARGETKIND, List<BaseNPC>> _skill_target_dictionary_list = new Dictionary<ETARGETKIND, List<BaseNPC>>();
    public const int MAXLEVEL = 3;

    /// <summary>
    /// <summary>
    /// 내 위치에 이펙트 생성
    /// </summary>
    /// <param name="myposition"></param>

    //스킬 발동 시 발동자 나오는 이펙트
    public void ActiveSkillEffectToTarget(Vector3 myposition)
    {
        var maxcount = _active_skillEffect.Count;
        for (int i = 0; i < maxcount; i++)
        {
            var mypositioneffect = Instantiate<GameObject>(_active_skillEffect[i], myposition, default);
        }
    }

    /// <summary>
    /// OBJECT_ENTER 타입 스킬 효과 생성 (스킬 컴포넌트 포함)
    /// </summary>
    /// <param name="caster">스킬 사용자</param>
    /// <param name="spawnPosition">생성 위치</param>
    /// <param name="direction">진행 방향 (optional)</param>
    /// <param name="targetPosition">타겟 위치 (optional, 유도탄/포물선용)</param>
    public virtual void CreateSkillObjects(BaseNPC caster, Action<BaseNPC, BaseNPC> action, Vector3 targetposition)
    {
        if (_eusetype != EUSETYPE.OBJECT_ENTER && _eusetype != EUSETYPE.OBJECT_VIEW)
            return;

        // 타겟 리스트 가져오기 (유도 미사일용)
        var targetList = FilterTargetList(caster);
        var filteredTargets = FilterTargetsByArea(caster, targetList);

        // _enter_hit_object 배열의 오브젝트들을 생성
        var maxCount = _enter_hit_object.Count;
        for (int i = 0; i < maxCount; i++)
        {
            if (_enter_hit_object[i] == null)
                continue;

            //TODO: 생성위치는 일단 생성자에게 하고 해당 오브젝트에서 어디로 생성될지 지정하도록 할 예정
            var skillObject = Instantiate(_enter_hit_object[i], caster.transform.position, Quaternion.identity);

            // SkillEffectController 설정
            var effectController = skillObject.GetComponent<SkillEffectController>();
            if (effectController == null)
            {
                effectController = skillObject.AddComponent<SkillEffectController>();
            }
            effectController.Initialize(caster, action);

            // Movement Controller 설정
            var movementController = skillObject.GetComponent<SkillMovementController>();
            if (movementController != null)
            {
                // 움직임 타입에 따라 다른 설정 적용
                switch (movementController._movementType)
                {
                    case EMOVEMENTTYPE.HOMING:
                    case EMOVEMENTTYPE.NOW:
                        // 유도 타입: 타겟 설정
                        if (filteredTargets != null && filteredTargets.Count > 0)
                        {
                            movementController.SetTargets(filteredTargets[0]);
                        }
                        break;

                    case EMOVEMENTTYPE.PARABOLA:
                        // 포물선 타입: 타겟 위치 설정
                        if (targetposition != Vector3.zero)
                        {
                            movementController.SetTargetPosition(targetposition);
                        }
                        else if (filteredTargets != null && filteredTargets.Count > 0)
                        {
                            movementController.SetTargetPosition(filteredTargets[0].transform.position);
                        }
                        break;

                    case EMOVEMENTTYPE.ROTATE:
                        // 회전 타입: 중심점 설정 (caster를 중심으로)
                        movementController.SetRotationCenter(caster.transform);
                        break;

                    default:
                        // 직진 등 기본 움직임은 별도 설정 불필요
                        break;
                }
            }
        }
    }

    public void ActiveSkillPlayAnimation(BaseNPC me)
    {
        me.PlayAnimation(_eanimation);
    }

    public virtual List<BaseNPC> FilterTargetList(BaseNPC me)
    {
        if (_skill_target_dictionary_list.TryGetValue(_etargetkind, out var targetlist) == false)
        {
            return null;
        }
        if (targetlist.Count <= 0)
        {
            return null;
        }

        if (_etargetkind == ETARGETKIND.ME)
        {
            return new List<BaseNPC> { me };
        }

        // 죽은 대상 제외
        var alivelist = targetlist.Where(x => x.CheckDie() == false).ToList();
        if (alivelist.Count <= 0)
        {
            return new List<BaseNPC>();
        }

        switch (_etargetfiltertype)
        {
            case ETARGETFILTERTYPE.NONE:
                return alivelist;

            case ETARGETFILTERTYPE.POS_NEAR_HERO:
            case ETARGETFILTERTYPE.POS_NEAR_MONSTER:
                // 가장 가까운 대상
                return alivelist
                    .OrderBy(x => Vector3.Distance(me.transform.position, x.transform.position))
                    .Take(1)
                    .ToList();

            case ETARGETFILTERTYPE.POS_FAR_HERO:
            case ETARGETFILTERTYPE.POS_FAR_MONSTER:
                // 가장 먼 대상
                return alivelist
                    .OrderByDescending(x => Vector3.Distance(me.transform.position, x.transform.position))
                    .Take(1)
                    .ToList();

            case ETARGETFILTERTYPE.MOST_CURRENT_HP_HERO:
            case ETARGETFILTERTYPE.MOST_CURRENT_HP_MONSTER:
                return alivelist
                    .OrderByDescending(x => x.GetCurrentHP())
                    .Take(1)
                    .ToList();

            case ETARGETFILTERTYPE.MOST_SMALL_CURRENT_HP_HERO:
            case ETARGETFILTERTYPE.MOST_SMALL_CURRENT_HP_MONSTER:
                return alivelist
                    .OrderBy(x => x.GetCurrentHP())
                    .Take(1)
                    .ToList();

            case ETARGETFILTERTYPE.MOST_MAXHP_HERO:
            case ETARGETFILTERTYPE.MOST_MAXHP_MONSTER:
                return alivelist
                    .OrderByDescending(x => x.GetMaxHP())
                    .Take(1)
                    .ToList();

            case ETARGETFILTERTYPE.MOST_SMALL_MAXHP_HERO:
            case ETARGETFILTERTYPE.MOST_SMALL_MAXHP_MONSTER:
                return alivelist
                    .OrderBy(x => x.GetMaxHP())
                    .Take(1)
                    .ToList();

            case ETARGETFILTERTYPE.MOST_POWER_HERO:
            case ETARGETFILTERTYPE.MOST_POWER_MONSTER:
                return alivelist
                    .OrderByDescending(x => x.TotalDamage())
                    .Take(1)
                    .ToList();

        }
        return null;
    }

    /// <summary>
    /// 범위 내 타겟 필터링 (새로운 메서드 추가)
    /// </summary>
    /// <param name="me">스킬 사용자</param>
    /// <param name="targetList">전체 타겟 리스트</param>
    /// <returns>범위 내 타겟 리스트</returns>
    public virtual List<BaseNPC> FilterTargetsByArea(BaseNPC me, List<BaseNPC> targetList)
    {
        if (targetList == null || targetList.Count <= 0)
        {
            return new List<BaseNPC>();
        }

        switch (_eskillarea)
        {
            case ESKILLAREA.ONE:
                // 한 마리만 - 이미 FilterTargetList에서 처리됨
                return targetList.Take(1).ToList();

            case ESKILLAREA.ALL:
                // 전체 - 모든 타겟
                return targetList;

            case ESKILLAREA.CIRCLE:
                return targetList.Where(target =>
                    Vector3.Distance(me.transform.position, target.transform.position) <= _circleRadius
                ).ToList();

            case ESKILLAREA.BOX:
                return targetList.Where(target =>
                {
                    Vector3 distance = target.transform.position - me.transform.position;
                    return Mathf.Abs(distance.x) <= _boxWidth * 0.5f &&
                           Mathf.Abs(distance.y) <= _boxHeight * 0.5f;
                }).ToList();

            default:
                return targetList;
        }
    }
}

[Serializable]
public struct St_SkillInfo
{
    public string _name;
    public string _explain;
    public float _cooltime;
    public float _duration;
    public int _mid;
    public int _level;
    public Sprite _skillicon;

    //이 스킬 사용 후 다음 스킬 사용까지 딜레이 시간
    public float _next_skilldelaytime;
}

