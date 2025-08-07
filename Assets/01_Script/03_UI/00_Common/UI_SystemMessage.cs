using TMPro;
using UnityEngine;

public class UI_SystemMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _message;

    public void Setting(string message)
    {
        _message.text = message;
        Destroy(this.gameObject, 1f);
    }
}