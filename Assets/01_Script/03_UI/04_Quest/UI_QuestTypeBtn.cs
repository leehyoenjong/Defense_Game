using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UI_QuestTypeBtn : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _questtype;
    [SerializeField] GameObject _alarm;
    [SerializeField] UnityEvent<EQUESTTYPE> _clickevent;
    EQUESTTYPE _questtypes;


    public void Setting(EQUESTTYPE types)
    {
        _questtypes = types;
        _questtype.text = types.ToString();
    }

    public void Btn_Click()
    {
        _clickevent.Invoke(_questtypes);
    }
}