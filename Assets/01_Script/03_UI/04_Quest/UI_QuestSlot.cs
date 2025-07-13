using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuestSlot : MonoBehaviour
{
    [SerializeField] UI_ItemSlot _itemslot;
    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] TextMeshProUGUI _explain;
    [SerializeField] TextMeshProUGUI _questvalue;
    [SerializeField] GameObject _clearpanel;
    [SerializeField] GameObject[] _btn;
    [SerializeField] Image _gage;

    St_QuestTable _questinfo;

    public static event Action<St_QuestTable> _quest_clear_event;

    public void Setting(St_QuestTable questinfo)
    {
        var isclear = UserData._userdata._userquestdata.CheckQuestClear(questinfo._mid);

        if (questinfo._isclearactiveoff && isclear)
        {
            this.gameObject.SetActive(false);
            return;
        }

        _questinfo = questinfo;

        //보상
        _itemslot.Setting(questinfo._rewarditemid, questinfo._rewarditemvalue);

        //소개
        _title.text = questinfo._title;
        _explain.text = questinfo._explain;

        //게이지
        var questgagevalue = questinfo.GetQuestClearAndUserValue();
        _gage.fillAmount = (float)questgagevalue.uservalue / questgagevalue.questvalue;
        _questvalue.text = $"{questgagevalue.uservalue}/{questgagevalue.questvalue}";

        //버튼
        _btn[0].SetActive(_gage.fillAmount < 1);
        _btn[1].SetActive(_gage.fillAmount >= 1);

        _clearpanel.SetActive(isclear);
    }

    public void Btn_Clear()
    {
        if (_questinfo.CheckClearQuest() == false)
        {
            return;
        }

        var clearcount = 1;
        if (_questinfo._questtype == EQUESTTYPE.REPEAT)
        {
            var questgagevalue = _questinfo.GetQuestClearAndUserValue();
            clearcount = questgagevalue.uservalue / questgagevalue.questvalue;
        }

        UserData._userdata._userquestdata.ClearQuestUpdate(_questinfo._mid, clearcount);
        RewardManager.instance.CreateReward(_questinfo._rewarditemid, _questinfo._rewarditemvalue * clearcount);
        _quest_clear_event?.Invoke(_questinfo);
        Setting(_questinfo);
    }

    public void Btn_Move()
    {
        switch (_questinfo._questcleartype)
        {
            case EQUESTVALUETYPE.QUESTCLEARCOUNT:
            case EQUESTVALUETYPE.TARGETTYPEQUESTCLEARCOUNT:
            case EQUESTVALUETYPE.TARGETQUESTCLEAR:
                return;
        }

        _questinfo.QuestMove();
    }
}