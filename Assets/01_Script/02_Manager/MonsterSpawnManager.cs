using UnityEngine;

public class MonsterSpawnManager : MonoBehaviour
{
    [SerializeField] Transform[] _protectpoint;

    public static MonsterSpawnManager instance;

    void Awake()
    {
        instance = this;
    }

    public void CreateMonster()
    {
        Monster_Base monsterbase = null;
        monsterbase.OnSpawn(MonsterMovePoint());
    }

    public Vector3 MonsterMovePoint()
    {
        var randomindex = UnityEngine.Random.Range(0, _protectpoint.Length);
        var movepoint = _protectpoint[randomindex].position;
        movepoint.x -= 0.5f;
        movepoint.y -= 0.5f;
        return movepoint;
    }
}
