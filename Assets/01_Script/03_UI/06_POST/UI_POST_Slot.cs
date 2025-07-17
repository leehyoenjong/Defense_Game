using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_POST_Slot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] TextMeshProUGUI _explain;
    [SerializeField] TextMeshProUGUI _time;
    [SerializeField] Transform _parent;
    [SerializeField] GameObject _slot;
    BackEndPOST.UPostItem _postinfo;

    public BackEndPOST.UPostItem GetPostInfo() => _postinfo;

    public void Setting(BackEndPOST.UPostItem postinfo)
    {
        _postinfo = postinfo;
        _title.text = postinfo.title;
        _explain.text = postinfo.content;
        _time.text = postinfo.TimeRemainingString;
        SettingSlot();
    }

    void SettingSlot()
    {
        foreach (var item in _postinfo.items)
        {
            var slot = Instantiate(_slot, _parent).GetComponent<UI_ItemSlot>();
            slot.Setting(item.itemID, item.itemCount);
        }
    }

    public async void Btn_Reward()
    {
        var result = await BackEndPOST.RemovePost(_postinfo.inDate);
        if (result == false)
        {
            return;
        }
        // UPostItem에서 보상 리스트를 가져와서 한번에 처리
        var rewardList = _postinfo.GetRewardList();

        if (rewardList.Count > 0)
        {
            RewardManager.instance.CraeteReward(rewardList);
        }

        this.gameObject.SetActive(false);
    }
}