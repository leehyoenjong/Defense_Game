using TMPro;
using UnityEngine;

public class UI_Stage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _stagenumber;

    string STAGENUMBER = "STAGE {0}/{1}";

    void Start()
    {
        PlayManager._play_stage_and_chapter_next += StageNumberSetting;
        PlayManager._play_stage_and_chapter_start += StageNumberSetting;
    }
    void OnDisable()
    {
        PlayManager._play_stage_and_chapter_next -= StageNumberSetting;
        PlayManager._play_stage_and_chapter_start -= StageNumberSetting;
    }

    void StageNumberSetting(int stageid, int stagemax)
    {
        _stagenumber.text = string.Format(STAGENUMBER, stageid, stagemax);
    }
}
