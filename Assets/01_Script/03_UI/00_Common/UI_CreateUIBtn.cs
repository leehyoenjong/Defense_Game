using UnityEngine;
using UnityEngine.Events;

public class UI_CreateUIBtn : MonoBehaviour
{
    [SerializeField] GameObject _popup;
    [SerializeField] UnityEvent<GameObject> _createevent;
    public void Btn_Create()
    {
        var popup = Instantiate(_popup, null);
        _createevent?.Invoke(popup);
    }
}