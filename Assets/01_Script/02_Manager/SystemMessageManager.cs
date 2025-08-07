using UnityEngine;

public class SystemMessageManager : MonoBehaviour
{
    public static SystemMessageManager instance;
    [SerializeField] GameObject _message;

    void Awake()
    {
        instance = this;
    }

    public void CreateSystemMessage(string message)
    {
        Instantiate(_message, null).GetComponent<UI_SystemMessage>().Setting(message);
    }
}
