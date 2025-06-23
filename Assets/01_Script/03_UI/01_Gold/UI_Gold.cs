using TMPro;
using UnityEngine;

public class UI_Gold : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _gold;

    void Start()
    {
        GoldManager._gold_get_event += GoldSetting;
    }

    void OnDisable()
    {
        GoldManager._gold_get_event -= GoldSetting;
    }

    void GoldSetting(int gold)
    {
        _gold.text = gold.ToString();
    }
}