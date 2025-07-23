using UnityEngine;

public class SO_Skill_Attack : BaseSkill
{
    //타겟에게 나타나는 이펙트 오브젝트
    [Header("타겟 위치에 나타나는 이펙트 오브젝트")]
    [SerializeField] GameObject[] _target_attackeffect;

    [Header("공격 데미지 퍼센트")]
    [SerializeField] float _skilldamagepercent;

    public int SkillDamage(int totaldamage)
    {
        return Mathf.CeilToInt(totaldamage * _skilldamagepercent);
    }

    /// <summary>
    /// 스킬 실행
    /// </summary>
    /// <param name="me"></param>
    public virtual bool ActiveSkill(BaseNPC me)
    {
        // 사용자 애니메이션 실행
        ActiveSkillPlayAnimation(me);

        // 스킬 타입에 따른 실행
        switch (_eusetype)
        {
            case EUSETYPE.NOW:
                return ExecuteImmediateSkill(me);
            case EUSETYPE.OBJECT_ENTER:
                return ExecuteObjectEnterSkill(me);
            default:
                return false;
        }
    }

    /// <summary>
    /// 즉시 발동 스킬 실행
    /// </summary>
    private bool ExecuteImmediateSkill(BaseNPC me)
    {
        // 타겟 리스트 가져오기
        var targetlist = FilterTargetList(me);
        if (targetlist == null || targetlist.Count <= 0)
        {
            return false;
        }

        // 범위를 이용한 타겟 필터링
        var finalTargets = FilterTargetsByArea(me, targetlist);
        if (finalTargets.Count <= 0)
        {
            return false;
        }

        //사용자 스킬 이펙트 실행 
        ActiveSkillEffectToTarget(me.transform.position);

        var maxcount = finalTargets.Count;
        for (int i = 0; i < maxcount; i++)
        {
            //이펙트 생성
            TargetAttack(me, finalTargets[i]);
        }
        return true;
    }

    /// <summary>
    /// 오브젝트 접촉 스킬 실행
    /// </summary>
    private bool ExecuteObjectEnterSkill(BaseNPC me)
    {
        //사용자 스킬 이펙트 실행 
        ActiveSkillEffectToTarget(me.transform.position);

        // 타겟 위치 계산 (가장 가까운 적 방향으로)
        var targetlist = FilterTargetList(me);
        if (targetlist == null || targetlist.Count <= 0)
        {
            return false;
        }

        // 스킬 오브젝트 생성 (충돌 감지 컴포넌트 포함)
        CreateSkillObjects(me, TargetAttack, targetlist[0].transform.position);
        return true;
    }

    public virtual void TargetAttack(BaseNPC me, BaseNPC target)
    {
        var totaldamage = SkillDamage(me.TotalDamage());
        me.Target_To_Attack(target, totaldamage);
        TargetToEffect(target.transform.position);
    }

    /// <summary>
    /// 타겟 위치에 생성되는 이펙트
    /// </summary>
    public virtual void TargetToEffect(Vector3 targetposition)
    {
        var maxcount = _target_attackeffect.Length;
        for (int i = 0; i < maxcount; i++)
        {
            var targettoeffect = Instantiate<GameObject>(_target_attackeffect[i], targetposition, default);
        }
    }
}