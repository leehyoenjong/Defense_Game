using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillChose_Slot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _name, _explain;
    [SerializeField] Image _skillicon;

    Player_Base _chosehero;
    BaseSkill _choseskill;

    public void Setting(Player_Base chosehero, BaseSkill choseskill)
    {
        this.gameObject.SetActive(true);
        _chosehero = chosehero;
        _choseskill = choseskill;
    }


    public void Btn_Click()
    {
        _chosehero.AddActiveSkill(_choseskill);
    }
}
