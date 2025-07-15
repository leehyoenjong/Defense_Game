using UnityEngine;
using UnityEngine.Events;

public class UI_BtnEvent : MonoBehaviour
{
    [SerializeField] UnityEvent _click_event;

    public void Btn_Click()
    {
        _click_event?.Invoke();
    }
}