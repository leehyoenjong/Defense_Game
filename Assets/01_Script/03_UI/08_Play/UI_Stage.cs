using TMPro;
using UnityEngine;

public class UI_Stage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _chapternumber;
    [SerializeField] TextMeshProUGUI _stagenumber;

    string STAGENUMBER = "STAGE {0}/{1}";
    string CHAPTERNUMBER = "CHAPTER {0}";

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
            case EPLAYSTATE.STAGE_START:
            case EPLAYSTATE.STAGE_NEXT:
            case EPLAYSTATE.CHAPTER_NEXT:
            case EPLAYSTATE.CHAPTER_START:
                _chapternumber.text = string.Format(CHAPTERNUMBER, PlayManager.instance.GetCurrentChapterID());
                var stageid = PlayManager.instance.GetCurrentStageID() + 1;
                _stagenumber.text = string.Format(STAGENUMBER, stageid, gamestate._maxstagecount);
                break;
        }
    }
}
