using UnityEngine;

public class Player_Base : BaseNPC
{
    public virtual void OnSpawn(Vector2 spawnpoint)
    {
        transform.position = spawnpoint;
        base.OnSpawn();
    }
}