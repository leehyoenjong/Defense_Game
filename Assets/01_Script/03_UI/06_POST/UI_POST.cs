using System.Collections.Generic;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UI_POST : MonoBehaviour
{
    [SerializeField] GameObject _none_post;
    [SerializeField] GameObject _slot;
    [SerializeField] Transform _parent;


    List<UI_POST_Slot> _postslotlist = new List<UI_POST_Slot>();

    void OnEnable()
    {
        SettingSlot().Forget();
    }

    async UniTaskVoid SettingSlot()
    {
        var postlist = await BackEndPOST.GetPOSTList(10);
        var maxcount = postlist.Count;
        foreach (var item in _postslotlist)
        {
            item.gameObject.SetActive(false);
        }

        if (maxcount <= 0)
        {
            _none_post.SetActive(true);
            return;
        }
        _none_post.SetActive(false);

        for (int i = 0; i < maxcount; i++)
        {
            UI_POST_Slot slot = null;
            if (i >= _postslotlist.Count)
            {
                slot = Instantiate(_slot, _parent).GetComponent<UI_POST_Slot>();
                _postslotlist.Add(slot);
            }
            else
            {
                slot = _postslotlist[i];
                slot.gameObject.SetActive(true);
            }

            slot.Setting(postlist[i]);
        }
    }


    public async void Btn_AllReward()
    {
        var result = await BackEndPOST.RemoveAllPost();
        if (result == false)
        {
            return;
        }

        // 모든 활성화된 우편 슬롯에서 보상 리스트를 가져와서 하나로 합치기
        var allRewardList = new List<St_RewardItemList>();
        var activeSlots = _postslotlist.Where(x => x.gameObject.activeSelf);

        foreach (var slot in activeSlots)
        {
            var postRewards = slot.GetPostInfo().GetRewardList();
            allRewardList.AddRange(postRewards);
        }

        // 모든 보상을 한번에 처리
        if (allRewardList.Count > 0)
        {
            RewardManager.instance.CraeteReward(allRewardList);
        }

        // 모든 슬롯 비활성화
        foreach (var slot in activeSlots)
        {
            slot.gameObject.SetActive(false);
        }
    }
}
