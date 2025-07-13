using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManger : MonoBehaviour
{

    void Awake()
    {
        UserData.Create();
        DontDestroyOnLoad(this.gameObject);
        SceneManager.LoadSceneAsync("01_LOBBY");
    }
}