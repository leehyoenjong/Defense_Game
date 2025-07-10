using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Pause : MonoBehaviour
{
    void OnEnable()
    {
        Time.timeScale = 0;
    }

    void OnDisable()
    {
        Time.timeScale = 1;
    }

    public void Btn_Continue()
    {
        Destroy(this.gameObject);
    }

    public void Btn_Quit()
    {
        //TODO:추후에 씬 매니저로 이동 변경 예정
        SceneManager.LoadScene("00_LOBBY");
    }
}
