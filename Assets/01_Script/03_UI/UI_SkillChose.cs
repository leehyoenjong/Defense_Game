using System.Collections.Generic;
using UnityEngine;

public class UI_SkillChose : MonoBehaviour
{
    [SerializeField] UI_SkillChose_Slot[] _skillchose_slots;
    List<BaseSkill> _choseskilllist = new List<BaseSkill>();

    void OnEnable()
    {
        SettingSlot();
        Time.timeScale = 0;
    }

    void OnDisable()
    {
        Time.timeScale = 1;
    }

    void SettingSlot()
    {
        _choseskilllist.Clear();
        var maxcount = _skillchose_slots.Length;
        for (int i = 0; i < maxcount; i++)
        {
            _skillchose_slots[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < maxcount; i++)
        {
            var chosedata = ChoseData();
            if (chosedata.chosehero == null || chosedata.choseskill._skillInfo._mid == 0)
            {
                continue;
            }
            _choseskilllist.Add(chosedata.choseskill);
            _skillchose_slots[i].Setting(chosedata.chosehero, chosedata.choseskill, () => this.gameObject.SetActive(false));
        }

        this.gameObject.SetActive(_choseskilllist.Count > 0);
    }

    (Hero_Base chosehero, BaseSkill choseskill) ChoseData()
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

            var choseskill = chosehero._so_npc.ChoseLevelLvSkill(chosehero, _choseskilllist);
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
