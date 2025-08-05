using UnityEngine;

public class UI_Hero : MonoBehaviour
{
    [SerializeField] UI_Hero_Btn[] _herobtn;
    [SerializeField] UI_Status _uistatus;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayManager._ongamestatechanged += HeroButtonSetting;
    }

    void OnDisable()
    {
        PlayManager._ongamestatechanged -= HeroButtonSetting;
    }

    void HeroButtonSetting(GameStateData gamestate)
    {
        if (gamestate._state != EPLAYSTATE.READY)
        {
            return;
        }

        var maxcount = _herobtn.Length;
        var userheroidlist = UserData._userdata._userequiphero._equipheroid;
        for (int i = 0; i < maxcount; i++)
        {
            var itemdata = DataManager.instance.GetItemTable().SearchItemData(userheroidlist[i]);
            _herobtn[i].Init(itemdata._connecttableid);
        }
    }
}
