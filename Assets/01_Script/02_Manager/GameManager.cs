using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject _messagepopup;

    public void CreatePopup(string message, Action exitaction)
    {
        var popup = Instantiate(_messagepopup, null);
        popup.GetComponent<UI_MessagePopup>().Setting(message, exitaction);
    }

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        UI_Login._login_faild_event += CreatePopup;
    }

    void OnApplicationQuit()
    {
        if (BackEnd.Backend.IsInitialized == false)
        {
            return;
        }
        BackEndUserData.UpdateUserData();
    }
}