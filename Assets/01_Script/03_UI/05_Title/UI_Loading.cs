using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Loading : MonoBehaviour
{
    [SerializeField] Image _gagebar;
    [SerializeField] TextMeshProUGUI _gagetext;
    [SerializeField] GameObject _loginpanel;
    float _currentgage = 0;
    float _updategage = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _gagebar.fillAmount = 0;
        _gagetext.text = "0%";
        _updategage = 0;
        _currentgage = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentgage <= 0)
        {
            return;
        }

        _updategage += Time.deltaTime;
        if (_updategage >= _currentgage)
        {
            _updategage = _currentgage;
        }
        _gagebar.fillAmount = _updategage;
        _gagetext.text = (_updategage * 100).ToString("F1") + "%";
        //로딩 완료시점
        if (_updategage < 1)
        {
            return;
        }
        _loginpanel.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void UpdateGage(float gage)
    {
        _currentgage += gage;
    }
}
