using UnityEngine;

public class Monster_Normal : Monster_Base
{
    protected override void Start()
    {
        base.Start();
        OnSpawn(new Vector2(-0.23f, 0.25f));
    }
}