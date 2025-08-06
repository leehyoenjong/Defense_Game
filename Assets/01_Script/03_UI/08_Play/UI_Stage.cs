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
                _stagenumber.text = string.Format(STAGENUMBER, gamestate._currentstage, gamestate._maxstagecount);
                break;
        }
    }
}
