using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject _messagepopup;
    public bool _is_local_mode;

    public void CreatePopup(string message, Action exitaction)
    {
        var popup = Instantiate(_messagepopup, null);
        popup.GetComponent<UI_MessagePopup>().Setting(message, exitaction);
    }

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        UI_Login._login_faild_event += CreatePopup;
    }

    void OnApplicationQuit()
    {
        BaseSkill._skill_target_dictionary_list.Clear();
        if (BackEnd.Backend.IsInitialized == false)
        {
            return;
        }
        BackEndUserData.UpdateUserData();
    }
}