using UnityEngine;

public class UI_Hero : MonoBehaviour
{
    [SerializeField] UI_Hero_Btn[] _herobtn;
    [SerializeField] UI_Status _uistatus;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayManager._play_ready_event += HeroButtonSetting;
    }

    void OnDisable()
    {
        PlayManager._play_ready_event -= HeroButtonSetting;
    }

    void HeroButtonSetting()
    {
        var maxcount = _herobtn.Length;
        var userheroidlist = UserData._userdata._userequiphero._equipheroid;
        for (int i = 0; i < maxcount; i++)
        {
            _herobtn[i].Init(userheroidlist[i]);
        }
    }
}
