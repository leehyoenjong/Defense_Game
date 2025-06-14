using UnityEngine;
using UnityEngine.UI;


public class HpbarController : MonoBehaviour
{
    [SerializeField] Image _hpgage;
    float _current_hpper = 1;

    private void Start()
    {
        _hpgage.fillAmount = _current_hpper;
    }

    public void Hpbar_Update(int maxhp, int curhp)
    {
        _current_hpper = (float)curhp / (float)maxhp;
    }

    private void Update()
    {
        _hpgage.fillAmount = Mathf.Lerp(_hpgage.fillAmount, _current_hpper, Time.deltaTime * 5f);
    }
}