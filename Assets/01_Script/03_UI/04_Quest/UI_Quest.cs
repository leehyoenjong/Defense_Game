using System.Collections.Generic;
using UnityEngine;

public class UI_Quest : MonoBehaviour
{
    [SerializeField] GameObject _slot;
    [SerializeField] Transform _parent;
    [SerializeField] UI_QuestTypeBtn[] _questtypebtn;

    List<UI_QuestSlot> _questslotlis = new List<UI_QuestSlot>();

    void OnEnable()
    {
        SettingQuestTypeBtn();
        SettingSlot(EQUESTTYPE.REPEAT);
    }

    void SettingQuestTypeBtn()
    {
        for (int i = 0; i < _questtypebtn.Length; i++)
        {
            _questtypebtn[i].Setting((EQUESTTYPE)i);
        }
    }

    public void SettingSlot(EQUESTTYPE types)
    {
        var questlist = DataManager.instance.GetQuestTable().GetQuestTypeList(types);
        if (questlist.Count <= 0)
        {
            //TODO: 퀘스트가 없을 순 없음!
            Debug.LogError("퀘스트 문제가 있음!");
            Destroy(this.gameObject);
            return;
        }

        var maxcount = _questslotlis.Count;
        for (int i = 0; i < maxcount; i++)
        {
            _questslotlis[i].gameObject.SetActive(false);
        }

        maxcount = questlist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            if (questlist[i]._mid == 0)
            {
                //TODO: 퀘스트가 없으면 안됩니다,
                Debug.LogError("퀘스트에 문제가 있습니다!");
                continue;
            }

            UI_QuestSlot slot = null;

            if (_questslotlis.Count > i)
            {
                slot = _questslotlis[i];
                slot.gameObject.SetActive(true);
            }
            else
            {
                slot = Instantiate(_slot, _parent).GetComponent<UI_QuestSlot>();
                _questslotlis.Add(slot);
            }

            slot.Setting(questlist[i]);
        }
    }
}