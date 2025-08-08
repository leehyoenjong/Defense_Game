using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager instance;

    [SerializeField] GameObject _rewardpopup;

    void Awake()
    {
        instance = this;
    }

    public void CreateReward(St_RewardItemList itemList)
    {
        UserData._userdata._userinventory.UpdateItemData(itemList._itemid, itemList._itemvalue);

        var createpopup = Instantiate(_rewardpopup, null);
        createpopup.GetComponent<UI_RewardPopup>().Setting(itemList);
    }

    public void CreateReward(List<St_RewardItemList> itemList)
    {
        var maxcount = itemList.Count;
        for (int i = 0; i < maxcount; i++)
        {
            UserData._userdata._userinventory.UpdateItemData(itemList[i]._itemid, itemList[i]._itemvalue);
        }

        var createpopup = Instantiate(_rewardpopup, null);
        createpopup.GetComponent<UI_RewardPopup>().Setting(itemList);
    }

    public void CreateReward(int itemid, int itemvalue, bool _nowindow = true)
    {
        UserData._userdata._userinventory.UpdateItemData(itemid, itemvalue);

        if (_nowindow == false)
        {
            return;
        }
        var createpopup = Instantiate(_rewardpopup, null);
        createpopup.GetComponent<UI_RewardPopup>().Setting(itemid, itemvalue);
    }
}