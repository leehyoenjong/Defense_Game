using BackEnd;
using UnityEngine;

public class BackEndManager : MonoBehaviour
{
    private void Awake()
    {
        var bro = Backend.Initialize(); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값
        if (bro.IsSuccess() == false)
        {
            GameManager.instance.CreatePopup("Back End Init Faild RePlay Plz", Application.Quit);
            return;
        }
        Debug.Log("초기화 성공 : " + bro); // 성공일 경우 statusCode 204 Success
    }
}