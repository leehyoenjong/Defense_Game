using System;
using TMPro;
using UnityEngine;

public class UI_MessagePopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _message;
    event Action _exit;
    public void Setting(string message, Action exitaction)
    {
        _message.text = message;
        _exit = exitaction;
    }

    public void Btn_Exit()
    {
        _exit?.Invoke();
    }
}