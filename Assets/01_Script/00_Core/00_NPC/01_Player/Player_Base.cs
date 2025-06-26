using UnityEngine;

public class Player_Base : BaseNPC
{
    protected int _id;
    public int GetID() => _id;

    public virtual void OnSpawn(Vector2 spawnpoint)
    {
        transform.position = spawnpoint;
        base.OnSpawn();
    }

    public virtual void IDSetting(int id)
    {
        _id = id;
    }
}