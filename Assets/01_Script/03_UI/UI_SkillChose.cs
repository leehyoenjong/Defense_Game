using System.Collections.Generic;
using UnityEngine;

public class UI_SkillChose : MonoBehaviour
{
    [SerializeField] UI_SkillChose_Slot[] _skillchose_slots;

    void Start()
    {
        SettingSlot();
    }


    void SettingSlot()
    {
        var maxcount = _skillchose_slots.Length;
        for (int i = 0; i < maxcount; i++)
        {
            _skillchose_slots[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < maxcount; i++)
        {
            var chosehero = ChoseHero();
            if (chosehero.chosehero == null)
            {
                return;
            }

            _skillchose_slots[i].Setting(chosehero.chosehero, chosehero.choseskill);
        }
    }

    (Player_Base chosehero, BaseSkill choseskill) ChoseHero()
    {
        var herolist = PlayerSpawnManager.instance.GetHeroList();
        var choseidxlist = new List<int>();

        // herolist의 전체 인덱스 값을 choseidxlist에 추가
        for (int i = 0; i < herolist.Count; i++)
        {
            choseidxlist.Add(i);
        }

        // choseidxlist가 비어있지 않을 때까지 반복
        while (choseidxlist.Count > 0)
        {
            var randomIdx = UnityEngine.Random.Range(0, choseidxlist.Count);
            var heroidx = choseidxlist[randomIdx];
            var chosehero = herolist[heroidx];

            var choseskill = chosehero._so_npc.ChoseLevelLvSkill(chosehero);
            if (choseskill != null)
            {
                return (chosehero, choseskill);
            }

            // choseskill이 null이면 해당 인덱스를 제외
            choseidxlist.RemoveAt(randomIdx);
        }

        return (null, null);
    }
}
