using UnityEngine;
using UnityEngine.Events;

public class UI_ActiveOnOffUIBtn : MonoBehaviour
{
    [SerializeField] GameObject _active;
    [SerializeField] UnityEvent _disable;

    public void Btn_Click()
    {
        _active.SetActive(!_active.activeSelf);
    }

    void OnDisable()
    {
        _disable?.Invoke();
    }
}