using BackEnd;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BackEndLogin : MonoBehaviour
{
    public BackendReturnObject ActiveLogin_Guset()
    {
        return Backend.BMember.GuestLogin("게스트 로그인 시도");
    }

    public BackendReturnObject ActiveLogin_Apple()
    {
        return Backend.BMember.AuthorizeFederation("idToken", FederationType.Apple, "애플 로그인 시도");
    }
}

public enum BackEndLoginState
{
    SUCCESS= 200,
    NEW_USER_SUCCESS = 201,
    DEVICE_NULL = 400,
    BAD_LOGIN,
    NONE_CONNECT_USER = 403

}