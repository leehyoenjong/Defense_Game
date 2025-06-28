using System;
using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hero_Btn : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] TextMeshProUGUI _status;
    [SerializeField] Image _icon;

    const string STATUSDATA = "- {0}\n- {1}%\n- {2}%";
    Player_Base _heroclass;
    public static event Action<int> _hero_click_event;

    public void Init(int heroid)
    {
        //현재 필드에 있는 영웅들 리스트를 가져와 아이디 매칭 
        var herolist = PlayerSpawnManager.instance.GetHeroList();
        var heroclass = herolist.Find(x => x.GetID() == heroid);
        if (heroclass == null)
        {
            this.gameObject.SetActive(false);
            return;
        }

        _heroclass = heroclass;
        this.gameObject.SetActive(true);

        //영웅의 기본 데이터를 가져와 이름, 아이콘을 매칭하고 스테이터스는 필드에 있는 거에서 매칭하기
        var hero_origindata = PlayManager.instance.GetHeroData(_heroclass.GetID());
        _name.text = hero_origindata._name;
        _icon.sprite = hero_origindata._icon;
        _status.text = string.Format(STATUSDATA, _heroclass.GetStatus()._damge, _heroclass.GetStatus()._critical, _heroclass.GetStatus()._critical_damage);
    }

    public void Btn_Click()
    {
        UI_Status._heroclass = () => _heroclass;
        _hero_click_event?.Invoke(_heroclass.GetID());
    }
}
