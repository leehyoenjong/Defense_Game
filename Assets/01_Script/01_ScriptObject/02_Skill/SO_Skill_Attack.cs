using UnityEngine;

public class SO_Skill_Attack : BaseSkill
{
    //타겟에게 나타나는 이펙트 오브젝트
    [Header("타겟 위치에 나타나는 이펙트 오브젝트")]
    [SerializeField] GameObject[] _target_attackeffect;

    [Header("공격 데미지 퍼센트")]
    [SerializeField] float _skilldamagepercent;

    [Header("ㅡㅡㅡㅡ공격 타입ㅡㅡㅡㅡ")]
    [SerializeField] EATTACKTYPE _attack_type;

    [Header("ㅡㅡㅡㅡ범위ㅡㅡㅡㅡ")]
    [SerializeField] EATTACKAREA _attack_area;

    [Header("ㅡㅡㅡㅡ타겟ㅡㅡㅡㅡ")]
    [SerializeField] EATTACKTARGETKIND _attack_target_kind;

    public int SkillDamage(int totaldamage)
    {
        return Mathf.CeilToInt(totaldamage * _skilldamagepercent);
    }

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

        //사용자 애니메이션 실행
        ActiveSkillPlayAnimation(me);

        //데미지 주기
        var totaldamage = SkillDamage(me.TotalDamage());
        me.Target_To_Attack(target, totaldamage);
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

    public virtual BaseNPC GetTarget()
    {
        return null;
    }
}