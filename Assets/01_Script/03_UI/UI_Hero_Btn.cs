using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hero_Btn : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] TextMeshProUGUI _status;
    [SerializeField] Image _icon;

    const string STATUSDATA = "ATTACK             - {0}\nCRITICALPER       - {1}%\nCRITICALDAMAGE  - {2}%";

    public void Init(int heroid)
    {
        if (heroid == 0)
        {
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);

        var herodata = PlayManager.instance.GetHeroData(heroid);
        _name.text = herodata._name;
        _icon.sprite = herodata._icon;
    }
}
