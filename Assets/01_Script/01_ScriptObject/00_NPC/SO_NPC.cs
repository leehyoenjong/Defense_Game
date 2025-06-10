using UnityEngine;

[CreateAssetMenu(fileName = "SO_NPC", menuName = "SO_NPC", order = 0)]
public class SO_NPC : ScriptableObject
{
    public int _Hp;
    public int _Damge;
    public int _Armor;
    public float _Critical;// 0~1
    public float _Critical_damage; // 0~1

    public int TotalDamage()
    {
        float damage = _Damge;

        var critical_random_value = UnityEngine.Random.Range(0f, 1f);
        if (critical_random_value <= _Critical)
        {
            damage = damage * (_Critical_damage + 1);
        }

        return Mathf.FloorToInt(damage);
    }
}