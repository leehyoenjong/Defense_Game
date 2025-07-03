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

    public BaseSkill ChoseLevelLvSkill(BaseNPC me, List<BaseSkill> alreadychoseskilllist)
    {
        var activeSkills = me.GetActiveAttackSkill();
        var maxskill = activeSkills.FindAll(x => x._skillInfo._level == BaseSkill.MAXLEVEL);

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

        // activeSkills와 choseskilllist에서 _mid가 중복되는 경우 처리
        var activeSkillMids = activeSkills.Select(x => x._skillInfo._mid).ToList();
        var filteredChoseSkillList = new List<BaseSkill>();


        // _mid별로 그룹화
        var groupedByMid = choseskilllist.GroupBy(x => x._skillInfo._mid);

        foreach (var group in groupedByMid)
        {
            var mid = group.Key;
            var skillsInGroup = group.ToList();

            //이미 선택한 스킬이라면 제외
            if (alreadychoseskilllist.FindIndex(x => x._skillInfo._mid == mid) != -1)
            {
                continue;
            }

            if (activeSkillMids.Contains(mid))
            {
                // 중복되는 _mid인 경우: 현재 활성 스킬의 다음 레벨만 남기기
                var activeSkill = activeSkills.First(x => x._skillInfo._mid == mid);
                var nextLevel = activeSkill._skillInfo._level + 1;
                var nextLevelSkill = skillsInGroup.FirstOrDefault(x => x._skillInfo._level == nextLevel);

                if (nextLevelSkill != null)
                {
                    filteredChoseSkillList.Add(nextLevelSkill);
                }
            }
            else
            {
                // 중복되지 않는 _mid인 경우: 가장 낮은 레벨만 남기기
                var lowestLevelSkill = skillsInGroup.OrderBy(x => x._skillInfo._level).First();
                filteredChoseSkillList.Add(lowestLevelSkill);
            }
        }

        if (filteredChoseSkillList.Count == 0)
        {
            return null; // 선택할 수 있는 스킬이 없는 경우
        }

        var choseidx = UnityEngine.Random.Range(0, filteredChoseSkillList.Count);
        BaseSkill choseskill = filteredChoseSkillList[choseidx];

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