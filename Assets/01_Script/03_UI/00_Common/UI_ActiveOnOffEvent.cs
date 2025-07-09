using UnityEngine;
using UnityEngine.Events;

public class UI_ActiveOnOffEvent : MonoBehaviour
{
    [SerializeField] UnityEvent _disable;
    [SerializeField] UnityEvent _enable;

    void OnDisable()
    {
        _disable?.Invoke();
    }

    void OnEnable()
    {
        _enable?.Invoke();
    }
}