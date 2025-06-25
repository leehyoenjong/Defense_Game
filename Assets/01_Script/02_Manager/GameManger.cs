using UnityEngine;

public class GameManger : MonoBehaviour
{
    public static UserData _userdata;

    void Awake()
    {
        //TODO: 추후 로그인 할 때 데이터 삽입하기
        _userdata = new UserData();
    }
}