using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hero_Btn : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] TextMeshProUGUI _status;
    [SerializeField] Image _icon;

    const string STATUSDATA = "- {0}\n- {1}%\n- {2}%";
    int HeroIDX;

    public void Init(int idx)
    {
        var herolist = PlayerSpawnManager.instance.GetHeroList();
        if (idx >= herolist.Count)
        {
            this.gameObject.SetActive(false);
            return;
        }
        HeroIDX = idx;
        var userherodata = herolist[idx];
        var heroid = userherodata.GetID();
        if (heroid == 0)
        {
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);

        var heroorigindata = PlayManager.instance.GetHeroData(heroid);
        _name.text = heroorigindata._name;
        _status.text = string.Format(STATUSDATA, userherodata.GetStatus()._damge, userherodata.GetStatus()._critical, userherodata.GetStatus()._critical_damage);
        _icon.sprite = heroorigindata._icon;
    }

    public void Btn_Click()
    {
        UI_Status._heroidx = () => HeroIDX;
    }
}
