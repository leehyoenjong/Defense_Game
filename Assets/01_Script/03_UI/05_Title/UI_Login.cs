using System;
using BackEnd;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Login : MonoBehaviour
{
    public static event Action<string, Action> _login_faild_event;
    [SerializeField] BackEndLogin _login;
    [SerializeField] BackEndUserData _userdata;

    public void Btn_GuestLogin()
    {
        var loginresult = _login.ActiveLogin_Guset();
        Debug.Log($"로그인 정보 :{loginresult.ToString()}");
        LoginResult(loginresult);
    }

    public void Btn_AppleLogin()
    {
        var loginresult = _login.ActiveLogin_Apple();
        LoginResult(loginresult);
    }

    async void LoginResult(BackendReturnObject loginresult)
    {
        if (GameManager.instance._is_local_mode)
        {
            UserData.Create();
            _ = SceneManager.LoadSceneAsync("01_LOBBY");
            return;
        }

        var loginstage = (BackEndLoginState)loginresult.StatusCode;
        UserData.Create();

        switch (loginstage)
        {
            case BackEndLoginState.NEW_USER_SUCCESS:
                _userdata.NewUserDataInit();
                _ = SceneManager.LoadSceneAsync("01_LOBBY");
                break;
            case BackEndLoginState.SUCCESS:
                var isloadresult = await _userdata.LoadUserData();
                if (isloadresult.Item1 == false)
                {
                    _login_faild_event?.Invoke(isloadresult.Item2, Application.Quit);
                    return;
                }
                _ = SceneManager.LoadSceneAsync("01_LOBBY");
                break;
            case BackEndLoginState.DEVICE_NULL:
            case BackEndLoginState.BAD_LOGIN:
            case BackEndLoginState.NONE_CONNECT_USER:
                _login_faild_event?.Invoke(loginresult.Message, Application.Quit);
                return;
        }
    }
}