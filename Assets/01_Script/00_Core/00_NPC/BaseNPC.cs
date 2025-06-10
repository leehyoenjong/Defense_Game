using System;
using UnityEngine;

public abstract class BaseNPC : MonoBehaviour
{
    //NPC 별 데이터 
    [SerializeField] SO_NPC _so_npc;

    //기본 맴버변수 
    protected int _current_hp;

    //이벤트 변수들
    public event Action _die_event;
    public event Action _hit_event;
    public event Action _attack_event;

    //함수
    protected virtual void Target_To_Attack(BaseNPC target_npc)
    {
        var my_damage = _so_npc.TotalDamage();
        target_npc.Hp_Update(my_damage);
        _attack_event?.Invoke();
    }

    protected virtual void Hp_Update(int target_damage)
    {
        _current_hp -= target_damage;
        _hit_event?.Invoke();

        if (_current_hp <= 0)
        {
            NPC_Die();
        }
    }
    protected virtual void NPC_Die()
    {
        _die_event?.Invoke();
    }
}