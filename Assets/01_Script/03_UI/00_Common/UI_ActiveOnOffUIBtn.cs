using UnityEngine;
using UnityEngine.Events;

public class UI_ActiveOnOffUIBtn : MonoBehaviour
{
    [SerializeField] GameObject _active;

    public void Btn_Click()
    {
        _active.SetActive(!_active.activeSelf);
    }
}