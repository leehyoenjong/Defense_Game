using TMPro;
using UnityEngine;

public class HpBarController_Text : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _hptext;
    const string HPTEXT = "{0}/{1}";
    public void Hpbar_Update(int maxhp, int curhp)
    {
        _hptext.text = string.Format(HPTEXT, curhp, maxhp);
    }
}