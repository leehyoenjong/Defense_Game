using UnityEngine;

public class UI_Hero : MonoBehaviour
{
    [SerializeField] UI_Hero_Btn[] _herobtn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HeroButtonSetting();
    }

    void HeroButtonSetting()
    {
        var userherolist = GameManger._userdata._userherodata;
        var maxcount = _herobtn.Length;
        for (int i = 0; i < maxcount; i++)
        {
            _herobtn[i].Init(userherolist[i]._heroid);
        }
    }
}
