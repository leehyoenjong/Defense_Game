using TMPro;
using UnityEngine;

public class UI_Stage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _stagenumber;

    string STAGENUMBER = "STAGE {0}/{1}";

    void Start()
    {
        PlayManager._ongamestatechanged += StageNumberSetting;
        PlayManager._ongamestatechanged += StageNumberSetting;
    }
    void OnDisable()
    {
        PlayManager._ongamestatechanged -= StageNumberSetting;
        PlayManager._ongamestatechanged -= StageNumberSetting;
    }

    void StageNumberSetting(GameStateData gamestate)
    {
        if (gamestate._state != EPLAYSTATE.CHAPTER_NEXT && gamestate._state != EPLAYSTATE.CHAPTER_START)
        {
            return;
        }

        _stagenumber.text = string.Format(STAGENUMBER, gamestate._currentstage, gamestate._maxstagecount);
    }
}
