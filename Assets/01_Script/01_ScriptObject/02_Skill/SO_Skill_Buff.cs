using UnityEngine;

public class SO_Skill_Buff : BaseSkill
{
    [Header("버프 줄 스테이터스")]
    public St_Status _add_status; //상승 시킬 스테이터스

    /// <summary>
    /// 스킬 실행
    /// </summary>
    /// <param name="me"></param>
    /// <param name="target"></param>
    public virtual void ActiveSkill(BaseNPC me, ESKILLTRIGGER buffskillactivetrigger)
    {
        if (buffskillactivetrigger != _eskilltrigger)
        {
            return;
        }

        //나에게 스테이터스 값 적용
        me.AddStatus(_add_status);

        //사용자 애니메이션 실행
        ActiveSkillPlayAnimation(me);

        //내 위치에 이펙트 생성
        ActiveSkillEffectToTarget(me.transform.position);
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

