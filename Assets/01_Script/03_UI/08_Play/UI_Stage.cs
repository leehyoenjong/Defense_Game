using TMPro;
using UnityEngine;

public class UI_Stage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _stagenumber;

    string STAGENUMBER = "STAGE {0}/{1}";

    void Start()
    {
        PlayManager._ongamestatechanged += StageNumberSetting;
    }
    void OnDisable()
    {
        PlayManager._ongamestatechanged -= StageNumberSetting;
    }

    void StageNumberSetting(GameStateData gamestate)
    {
        switch (gamestate._state)
        {
            case EPLAYSTATE.STAGE_NEXT:
            case EPLAYSTATE.STAGE_START:
            case EPLAYSTATE.CHAPTER_NEXT:
            case EPLAYSTATE.CHAPTER_START:

                Debug.Log($"gamestate: {gamestate._state} currentstage: {gamestate._currentstage}, maxstagecount: {gamestate._maxstagecount}");
                var currentstagenum = gamestate._currentstage + 1;
                _stagenumber.text = string.Format(STAGENUMBER, currentstagenum, gamestate._maxstagecount);
                break;
        }
    }
}
