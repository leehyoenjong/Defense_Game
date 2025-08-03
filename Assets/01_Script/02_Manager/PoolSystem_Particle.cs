using System.Collections.Generic;
using UnityEngine;

public class PoolSystem_Particle : MonoBehaviour
{
    public static PoolSystem_Particle instance;
    Dictionary<int, Queue<GameObject>> _particle_dic = new Dictionary<int, Queue<GameObject>>();

    void Awake()
    {
        instance = this;
        ParticleObject._disable_particle += ReleseParticle;
    }

    void OnDisable()
    {
        ParticleObject._disable_particle -= ReleseParticle;
    }

    public GameObject GetParticle(GameObject particle)
    {
        var hashcode = particle.GetHashCode();
        if (_particle_dic.ContainsKey(hashcode) == false)
        {
            _particle_dic.Add(hashcode, new Queue<GameObject>());
        }

        if (_particle_dic[hashcode].Count > 0)
        {
            var dequeueresult = _particle_dic[hashcode].Dequeue();
            dequeueresult.gameObject.SetActive(true);
            return dequeueresult.gameObject;
        }

        var par = Instantiate(particle, null);
        var particleojbect = par.GetComponent<ParticleObject>();
        if (particleojbect == null)
        {
            particleojbect = par.AddComponent<ParticleObject>();
        }

        particleojbect.Setting(hashcode);
        return particleojbect.gameObject;
    }

    void ReleseParticle(ParticleObject particleobject)
    {
        var hashcode = particleobject.GetHashCode_Key();
        if (_particle_dic.ContainsKey(hashcode) == false)
        {
            _particle_dic.Add(hashcode, new Queue<GameObject>());
        }

        _particle_dic[hashcode].Enqueue(particleobject.gameObject);
        particleobject.gameObject.SetActive(false);
    }
}
