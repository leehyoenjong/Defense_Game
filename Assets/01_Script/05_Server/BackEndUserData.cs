using BackEnd;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BackEndUserData : MonoBehaviour
{
    /// <summary>
    /// 첫 유저 접속시 데이터 추가
    /// </summary>
    public void NewUserDataInit()
    {
        Backend.PlayerData.InsertData("USERQUEST", UserData._userdata._userquestdata.Get_UserData());
        Backend.PlayerData.InsertData("USERINVENTORY", UserData._userdata._userinventory.Get_UserData());
        Backend.PlayerData.InsertData("USEREQUIP", UserData._userdata._userequiphero.Get_UserData());
        Backend.PlayerData.InsertData("USERCHAPTER", UserData._userdata._userchapterdata.Get_UserData());
    }


    public async UniTask<(bool, string)> LoadUserData()
    {
        bool[] loadcomplted = new bool[4];//유저 데이터 불러오기 결과
        bool[] loadfinished = new bool[4];//로드 완료 여부

        Backend.PlayerData.GetMyData("USERQUEST", callback =>
        {
            loadcomplted[0] = UserData._userdata._userquestdata.Load_UserData(callback);
            loadfinished[0] = true;
        });
        Backend.PlayerData.GetMyData("USERINVENTORY", callback =>
        {
            loadcomplted[1] = UserData._userdata._userinventory.Load_UserData(callback);
            loadfinished[1] = true;
        });
        Backend.PlayerData.GetMyData("USEREQUIP", callback =>
        {
            loadcomplted[2] = UserData._userdata._userequiphero.Load_UserData(callback);
            loadfinished[2] = true;
        });
        Backend.PlayerData.GetMyData("USERCHAPTER", callback =>
        {
            loadcomplted[3] = UserData._userdata._userchapterdata.Load_UserData(callback);
            loadfinished[3] = true;
        });

        // 모든 콜백이 완료될 때까지 기다리기
        await UniTask.WaitUntil(() => loadfinished[0] && loadfinished[1] && loadfinished[2] && loadfinished[3]);

        // 모든 로드가 성공했는지 확인
        for (int i = 0; i < loadcomplted.Length; i++)
        {
            if (!loadcomplted[i])
            {
                Debug.LogError($"사용자 데이터 로드 실패: {i}번 데이터");
                return (false, $"사용자 데이터 로드 실패: {i}번 데이터");
            }
        }

        Debug.Log("모든 사용자 데이터 로드 완료");
        return (true, "모든 사용자 데이터 로드 완료");
    }

    public static void UpdateUserData()
    {
        Backend.PlayerData.UpdateMyLatestData("USERQUEST", UserData._userdata._userquestdata.Get_UserData());
        Backend.PlayerData.UpdateMyLatestData("USERINVENTORY", UserData._userdata._userinventory.Get_UserData());
        Backend.PlayerData.UpdateMyLatestData("USEREQUIP", UserData._userdata._userequiphero.Get_UserData());
        Backend.PlayerData.UpdateMyLatestData("USERCHAPTER", UserData._userdata._userchapterdata.Get_UserData());
    }
}