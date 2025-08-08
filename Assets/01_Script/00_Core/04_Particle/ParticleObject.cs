using System;
using UnityEngine;

public class ParticleObject : MonoBehaviour
{
    int _hashcode;
    public int GetHashCode_Key() => _hashcode;
    public static event Action<ParticleObject> _disable_particle;
    ParticleSystem[] _particlesystemlist;

    void OnEnable()
    {
        if (_particlesystemlist == null || _particlesystemlist.Length <= 0)
        {
            _particlesystemlist = GetComponentsInChildren<ParticleSystem>();
        }

        foreach (var item in _particlesystemlist)
        {
            item.Play();
            item.gameObject.SetActive(true);
        }
    }

    void OnDisable()
    {
        _disable_particle?.Invoke(this);
    }

    public void Setting(int hashcode)
    {
        _hashcode = hashcode;
        if (_particlesystemlist != null && _particlesystemlist.Length > 0)
        {
            foreach (var item in _particlesystemlist)
            {
                // StopAction을 모두 Disable로 변경
                var main = item.main;
                main.stopAction = ParticleSystemStopAction.Disable;
            }
        }
    }
}