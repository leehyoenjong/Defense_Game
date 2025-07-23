using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillChose_Slot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _name, _explain;
    [SerializeField] Image _skillicon;

    BaseNPC _chosehero;
    BaseSkill _choseskill;
    Action _exit;

    public void Setting(BaseNPC chosehero, BaseSkill choseskill, Action exit)
    {
        this.gameObject.SetActive(true);
        _chosehero = chosehero;
        _choseskill = choseskill;
        _exit = exit;

        _name.text = _choseskill._skillInfo._name;
        _explain.text = _choseskill._skillInfo._explain;
        _skillicon.sprite = _choseskill._skillInfo._skillicon;
    }


    public void Btn_Click()
    {
        _chosehero.AddActiveSkill(_choseskill);
        _exit.Invoke();
    }
}
