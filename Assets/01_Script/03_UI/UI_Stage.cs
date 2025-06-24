using TMPro;
using UnityEngine;

public class UI_Stage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _stagenumber;

    string STAGENUMBER = "STAGE {0}/{1}";

    void Start()
    {
        StageNumberSetting();
        PlayManager._play_stageclear += StageNumberSetting;
    }
    void OnDisable()
    {
        PlayManager._play_stageclear -= StageNumberSetting;
    }

    void StageNumberSetting()
    {
        var currentchapterdata = PlayManager.instance.GetCurrentChapterData();
        var _current_stage_id = PlayManager.instance._current_stage_id;
        var maxstage = currentchapterdata._stagedata.Count;
        _stagenumber.text = string.Format(STAGENUMBER, _current_stage_id, maxstage);
    }
}
