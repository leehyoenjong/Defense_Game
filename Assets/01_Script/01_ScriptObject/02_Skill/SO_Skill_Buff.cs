using UnityEngine;

public class SO_Skill_Buff : BaseSkill
{
    [Header("버프 줄 스테이터스")]
    public St_Status _add_status; //적용 시킬 스테이터스

    /// <summary>
    /// 스킬 실행
    /// </summary>
    /// <param name="me"></param>
    /// <param name="buffskillactivetrigger"></param>
    public virtual void ActiveSkill(BaseNPC me, ESKILLTRIGGER buffskillactivetrigger)
    {
        if (buffskillactivetrigger != _eskilltrigger)
        {
            return;
        }

        //사용자 애니메이션 실행
        ActiveSkillPlayAnimation(me);

        // 스킬 타입에 따른 실행
        switch (_eusetype)
        {
            case EUSETYPE.NOW:
                ExecuteImmediateBuff(me);
                break;
            case EUSETYPE.OBJECT_ENTER:
                ExecuteObjectEnterBuff(me);
                break;
        }
    }

    /// <summary>
    /// 즉시 발동 버프 실행
    /// </summary>
    private void ExecuteImmediateBuff(BaseNPC me)
    {
        // 타겟 위치 계산 (아군에게 버프를 주는 경우 고려)
        var targetlist = FilterTargetList(me);
        if (targetlist == null || targetlist.Count <= 0)
        {
            return;
        }

        //내 위치에 이펙트 생성
        ActiveSkillEffectToTarget(me.transform.position);

        foreach (var target in targetlist)
        {
            TargetBuff(me, target);
        }
    }

    /// <summary>
    /// 오브젝트 접촉 버프 실행
    /// </summary>
    private void ExecuteObjectEnterBuff(BaseNPC me)
    {
        //내 위치에 이펙트 생성
        ActiveSkillEffectToTarget(me.transform.position);

        // 타겟 위치 계산 (아군에게 버프를 주는 경우 고려)
        var targetlist = FilterTargetList(me);
        if (targetlist == null || targetlist.Count <= 0)
        {
            return;
        }

        // 스킬 오브젝트 생성 (충돌 감지 컴포넌트 포함)
        CreateSkillObjects(me, TargetBuff, targetlist[0].transform.position);
    }

    /// <summary>
    /// 타겟에게 버프 적용
    /// </summary>
    /// <param name="me">스킬 사용자</param>
    /// <param name="target">버프를 받을 대상</param>
    public virtual void TargetBuff(BaseNPC me, BaseNPC target)
    {
        // 타겟에게 버프 적용
        target.AddStatus(_add_status);

        // 타겟 위치에 이펙트 생성
        ActiveSkillEffectToTarget(target.transform.position);
    }

    /// <summary>
    /// SkillController에서 UniTask를 이용해 종료 되었을때 해당 함수를 부르도록 해뒀음
    /// </summary>
    /// <param name="me"></param>
    public virtual void DisableSkill(BaseNPC me)
    {
        //나에게 적용된 스테이터스 값 제거 
        me.RemoveStatus(_add_status);
    }
}

public enum ESKILLTRIGGER
{
    SPAWN,
    AREAENTER,
}

