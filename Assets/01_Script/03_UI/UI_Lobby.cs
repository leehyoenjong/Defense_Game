using TMPro;
using UnityEngine;

public class UI_Lobby : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _lastchapter;

    const string CHAPTERTITLE = "LAST CHAPTER - {0}";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lastchapter.text = string.Format(CHAPTERTITLE, UserData._userdata._chapterdata._lastchapternumber);
    }
}
