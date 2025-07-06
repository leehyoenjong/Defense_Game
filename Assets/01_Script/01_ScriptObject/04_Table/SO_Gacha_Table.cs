using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Gacha_Table", menuName = "Table/SO_Gacha_Table", order = 0)]
public class SO_Gacha_Table : ScriptableObject
{
    public List<St_GachaTable> _gachatable = new List<St_GachaTable>();
    Dictionary<int, St_GachaTable> _gachatable_dic = new Dictionary<int, St_GachaTable>();
    public Dictionary<int, St_GachaTable> GetGachaTable()
    {
        if (_gachatable_dic.Count <= 0)
        {
            var maxcount = _gachatable.Count;
            for (int i = 0; i < maxcount; i++)
            {
                _gachatable_dic.Add(_gachatable[i]._gachaid, _gachatable[i]);
            }
        }
        return _gachatable_dic;
    }

    /// <summary>
    /// 가챠 상품데이터 가져오기 
    /// </summary>
    /// <returns></returns>
    public List<St_GachaItemList> GetGachaData(int gachaid)
    {
        if (GetGachaTable().TryGetValue(gachaid, out var gachadata) == false)
        {
            return default;
        }

        //균등이 아닐 경우 개별 퍼센트가 존재하기 때문에 바로 리턴
        if (gachadata._equaldistribution == false)
        {
            return gachadata._rewardlist;
        }

        //100%에서 아이템의 갯수만큼 나눠 퍼센트를 적용한 후 리턴
        var gachaitemlist = gachadata._rewardlist.ToList();

        var maxcount = gachaitemlist.Count;
        var percent = 100f / maxcount;

        for (int i = 0; i < maxcount; i++)
        {
            var gachaitemdata = gachaitemlist[i];
            gachaitemdata._percent = percent;
            gachaitemlist[i] = gachaitemdata;
        }

        return gachaitemlist;
    }

    public St_GachaItemList OpenGacha(int gachaid)
    {
        var gachaitemlist = GetGachaData(gachaid);
        if (gachaitemlist == null || gachaitemlist.Count == 0)
        {
            //TODO: 없으면 문제가 있는거임 에러 로그 송출
            Debug.LogError("상품이 없습니다!");
            return default;
        }

        // 모든 아이템의 확률 총합 계산
        var totalpercent = 0f;
        for (int i = 0; i < gachaitemlist.Count; i++)
        {
            totalpercent += gachaitemlist[i]._percent;
        }

        // 0부터 확률 총합 사이의 랜덤값 생성
        var randomvalue = UnityEngine.Random.Range(0f, totalpercent);
        var currentpercent = 0f;

        for (int i = 0; i < gachaitemlist.Count; i++)
        {
            currentpercent += gachaitemlist[i]._percent;

            // 랜덤값이 현재 누적확률보다 작거나 같으면 해당 아이템 선택
            if (randomvalue <= currentpercent)
            {
                return gachaitemlist[i];
            }
        }

        return default;
    }
}


[Serializable]
public struct St_GachaTable
{
    public int _gachaid;
    public bool _equaldistribution;
    public List<St_GachaItemList> _rewardlist;
}

[Serializable]
public struct St_GachaItemList
{
    public int _itemid;
    public int _itemvalue;
    public float _percent;
}