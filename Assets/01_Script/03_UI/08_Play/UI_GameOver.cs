using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_GameOver : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _stage;
    [SerializeField] TextMeshProUGUI _title;

    const string STAGEINFOTEXT = "STAGE {0} - {1}";

    public void Init(int lastchapter, int laststage, bool isclaer)
    {
        _stage.text = string.Format(STAGEINFOTEXT, lastchapter, laststage);
        _title.text = isclaer ? "CLEAR!" : "GAME OVER";
        Time.timeScale = 0;
    }

    void OnDisable()
    {
        Time.timeScale = 1;
    }
}
