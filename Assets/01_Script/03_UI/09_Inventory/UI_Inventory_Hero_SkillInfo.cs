using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Hero_SkillInfo : MonoBehaviour
{
    [SerializeField] Image _icon;
    [SerializeField] TextMeshProUGUI _explain;
    [SerializeField] TextMeshProUGUI _cooltime;
    const string EXPLAIN = "{0}\n{1}";

    public void Setting(BaseSkill heroskill)
    {
        _icon.sprite = heroskill._skillInfo._skillicon;
        _explain.text = string.Format(EXPLAIN, heroskill._skillInfo._name, heroskill._skillInfo._explain);
        _cooltime.text = heroskill._skillInfo._cooltime.ToString("F1");
    }
}