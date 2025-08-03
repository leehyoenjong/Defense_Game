using System;
using UnityEngine;

public class ParticleObject : MonoBehaviour
{
    int _hashcode;
    public int GetHashCode_Key() => _hashcode;
    public static event Action<ParticleObject> _disable_particle;

    void OnDisable()
    {
        _disable_particle?.Invoke(this);
    }

    public void Setting(int hashcode)
    {
        _hashcode = hashcode;
        var particlesystemlist = GetComponentsInChildren<ParticleSystem>();
        if (particlesystemlist != null && particlesystemlist.Length > 0)
        {
            foreach (var item in particlesystemlist)
            {
                // StopAction을 모두 Disable로 변경
                var main = item.main;
                main.stopAction = ParticleSystemStopAction.Disable;
            }
        }
    }
}