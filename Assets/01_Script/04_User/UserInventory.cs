
using System;
using System.Collections.Generic;
using BackEnd;
using BackEnd.BackndNewtonsoft.Json;
using UnityEngine;

[Serializable]
public struct St_UserInventory
{
    public List<St_UserInvenItemList> _userinvendata;
    public static event Action _upgrade_event;

    public Param Get_UserData()
    {
        var param = new Param();
        param.Add("_userinvendata", _userinvendata);
        return param;
    }

    public bool Load_UserData(BackendReturnObject loadresult)
    {
        if (loadresult.IsSuccess() == false)
        {
            return false;
        }

        var userdatajson = loadresult.FlattenRows()[0];
        Debug.Log($"전체 데이터: {userdatajson.ToString()}");

        // 인벤토리 아이템 데이터 로드
        if (userdatajson.ContainsKey("_userinvendata"))
        {
            var userinvendata = userdatajson["_userinvendata"];
            Debug.Log($"인벤토리 데이터 타입: {userinvendata.GetType()}");
            Debug.Log($"인벤토리 데이터 내용: {userinvendata.ToString()}");

            var maxcount = userinvendata.Count;
            Debug.Log($"아이템 개수: {maxcount}");

            for (int i = 0; i < maxcount; i++)
            {
                try
                {
                    var itemJsonData = userinvendata[i];
                    Debug.Log($"아이템 [{i}] 타입: {itemJsonData.GetType()}");
                    Debug.Log($"아이템 [{i}] 내용: {itemJsonData.ToString()}");

                    // JsonData에서 직접 값을 추출하여 구조체 생성
                    var itemData = new St_UserInvenItemList();

                    // 각 필드가 존재하는지 확인하고 값 추출
                    if (itemJsonData.ContainsKey("_itemid"))
                    {
                        itemData._itemid = (int)itemJsonData["_itemid"];
                        Debug.Log($"아이템 ID: {itemData._itemid}");
                    }

                    if (itemJsonData.ContainsKey("_itemvalue"))
                    {
                        itemData._itemvalue = (int)itemJsonData["_itemvalue"];
                        Debug.Log($"아이템 값: {itemData._itemvalue}");
                    }

                    if (itemJsonData.ContainsKey("_grade"))
                    {
                        itemData._grade = (int)itemJsonData["_grade"];
                        Debug.Log($"아이템 등급: {itemData._grade}");
                    }

                    _userinvendata.Add(itemData);
                    Debug.Log($"성공적으로 로드된 아이템: ID={itemData._itemid}, Value={itemData._itemvalue}, Grade={itemData._grade}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"인벤토리 아이템 [{i}] 데이터 로드 실패: {ex.Message}");
                    Debug.LogError($"스택 트레이스: {ex.StackTrace}");
                    return false;
                }
            }
        }
        else
        {
            Debug.Log("_userinvendata 키가 존재하지 않습니다.");
        }

        return true;
    }

    public void UpdateItemData(int itemid, int itemvalue)
    {
        var getitemresult = GetUserItemData(itemid);
        var beforevalue = getitemresult.itemdata._itemvalue;
        getitemresult.itemdata._itemvalue += itemvalue;
        _userinvendata[getitemresult.itemidx] = getitemresult.itemdata;
        BackEndLog.WriteLog(LogType.INVENTORY, $"획득한 아이템 아이디 :{itemid} / 획득 전 갯수:{beforevalue} / 획득 후 갯수:{getitemresult.itemdata._itemvalue}");
    }

    public void UpdateUpgrade(int itemid, int upgrade)
    {
        var getitemresult = GetUserItemData(itemid);
        var beforeupgrade = getitemresult.itemdata._grade;
        getitemresult.itemdata._grade = upgrade;
        _userinvendata[getitemresult.itemidx] = getitemresult.itemdata;
        _upgrade_event?.Invoke();
        BackEndLog.WriteLog(LogType.INVENTORY, $"업그레이드 아이디:{itemid} 업그레이드 전:{beforeupgrade} / 업그레이드 후:{getitemresult.itemdata._grade}");
    }

    public (St_UserInvenItemList itemdata, int itemidx) GetUserItemData(int itemid)
    {
        var itemidx = _userinvendata.FindIndex(x => x._itemid == itemid);
        if (itemidx == -1)
        {
            var itemdata = new St_UserInvenItemList();
            itemdata._itemid = itemid;
            _userinvendata.Add(itemdata);
            itemidx = _userinvendata.Count - 1;
        }
        return (_userinvendata[itemidx], itemidx);
    }
}

[Serializable]
public struct St_UserInvenItemList
{
    public int _itemid;
    public int _itemvalue;
    public int _grade;
}