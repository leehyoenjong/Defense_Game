using System.Collections.Generic;
using UnityEngine;

public class DropItemManager : MonoBehaviour
{
    Dictionary<int, int> _current_item_value = new Dictionary<int, int>();

    [SerializeField] GameObject _dropitemobject;

    void Start()
    {
        Monster_Base._monsterdie += DropItem;
    }

    void OnDisable()
    {
        Monster_Base._monsterdie -= DropItem;
    }

    void DropItem(Monster_Base monster)
    {
        var monsterinfo = monster.GetMonsterInfo();
        var dropitemid = monsterinfo._drop_itemid;
        var dropitemvalue = monsterinfo._drop_itemvalue;

        //현재 아이템 갯수 정보 추가 
        if (_current_item_value.ContainsKey(dropitemid) == false)
        {
            _current_item_value.Add(dropitemid, 0);
        }

        //갯수 추가 및 이벤트 발생
        _current_item_value[dropitemid] += dropitemvalue;

        //바닥에 떨어뜨릴 오브젝트 생성
        Instantiate(_dropitemobject, monster.transform.position, default).GetComponent<DropItemObject>().Setting(dropitemid, _current_item_value[dropitemid]);
    }
}