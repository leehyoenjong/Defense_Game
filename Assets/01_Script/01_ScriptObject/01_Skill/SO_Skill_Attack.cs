using UnityEngine;

public class SO_Skill_Attack : BaseSkill
{
    //타겟에게 나타나는 이펙트 오브젝트
    [SerializeField] GameObject[] _target_attackeffect;

    /// <summary>
    /// 스킬 실행
    /// </summary>
    /// <param name="me"></param>
    /// <param name="target"></param>
    public virtual void ActiveSkill(BaseNPC me, BaseNPC target)
    {
        //이펙트 생성
        ActiveSkillEffectToTarget(me.transform.position);
        TargetToEffect(target.transform.position);

        //데미지 주기
        me.Target_To_Attack(target);
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