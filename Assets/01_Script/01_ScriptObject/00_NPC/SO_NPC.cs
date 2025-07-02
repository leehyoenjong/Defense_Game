using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_NPC", menuName = "SO_NPC", order = 0)]
public class SO_NPC : ScriptableObject
{
    public St_Status _status;
    public SO_Skill_Attack[] _skill_Attack;
    public SO_Skill_Buff[] _skill_buff;
    public BaseSkill[] _skill_chose_list;
    public int _diegold;

    public BaseSkill ChoseLevelLvSkill(BaseNPC me)
    {
        var maxskill = me.GetActiveAttackSkill().FindAll(x => x._level == BaseSkill.MAXLEVEL);

        var choseskilllist = new List<BaseSkill>();
        choseskilllist.AddRange(_skill_chose_list);
        choseskilllist.AddRange(_skill_Attack);
        choseskilllist.AddRange(_skill_buff);

        //choseskilllist에서 maxskill의 _mid에 해당하는 거 제거
        if (maxskill.Count > 0)
        {
            var maxskillMids = maxskill.Select(x => x._skillInfo._mid).ToList();
            choseskilllist.RemoveAll(x => maxskillMids.Contains(x._skillInfo._mid));
        }

        if (choseskilllist.Count == 0)
        {
            return null; // 선택할 수 있는 스킬이 없는 경우
        }

        var choseidx = UnityEngine.Random.Range(0, choseskilllist.Count);
        BaseSkill choseskill = choseskilllist[choseidx];

        return choseskill;
    }
}


[Serializable]
public struct St_Status
{
    public int _hp;
    public int _damge;
    public int _armor;
    public float _critical;// 0~1
    public float _critical_damage; // 0~1
}