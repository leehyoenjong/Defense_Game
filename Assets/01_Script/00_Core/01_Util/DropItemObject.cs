using System;
using UnityEngine;

public class DropItemObject : MonoBehaviour
{
    [SerializeField] SpriteRenderer _sprite;
    [SerializeField] float _speed;
    [SerializeField] float _rotation_speed = 180f; // Y축 회전 속도 (도/초)
    [SerializeField] Vector3 _target_pos;

    public static event Action<int, int> _drop_item_event;

    int _dropitemid, _totalvalue;

    public void Setting(int itemid, int totalvalue)
    {
        _sprite.sprite = DataManager.instance.GetItemTable().SearchItemData(itemid)._itemicon;
        _dropitemid = itemid;
        _totalvalue = totalvalue;
        Destroy(this.gameObject, 1f);
    }

    private void Update()
    {
        // 목표 위치로 이동
        transform.position = Vector3.MoveTowards(transform.position, _target_pos, _speed * Time.deltaTime);

        // Y축 회전
        transform.Rotate(0, _rotation_speed * Time.deltaTime, 0);
    }

    void OnDestroy()
    {
        _drop_item_event?.Invoke(_dropitemid, _totalvalue);
    }
}