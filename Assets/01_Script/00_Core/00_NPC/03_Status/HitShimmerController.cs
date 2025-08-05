using Cysharp.Threading.Tasks;
using UnityEngine;

public class HitShimmerController : MonoBehaviour
{
    [SerializeField] BaseNPC _npc;
    [SerializeField] SpriteRenderer[] _mysprite;
    bool _ishitshimmer;

    void OnEnable()
    {
        _npc._hit_event += SetHitShimmer;
    }

    void OnDisable()
    {
        _npc._hit_event -= SetHitShimmer;
    }

    /// <summary>
    /// 공격 받으면 반짝임 효과
    /// </summary>
    public async void SetHitShimmer()
    {
        if (_ishitshimmer)
        {
            return;
        }

        _ishitshimmer = true;
        for (int i = 0; i < _mysprite.Length; i++)
        {
            _mysprite[i].color = Color.red;
        }

        await UniTask.WaitForSeconds(0.1f, cancellationToken: this.GetCancellationTokenOnDestroy());

        _ishitshimmer = false;
        for (int i = 0; i < _mysprite.Length; i++)
        {
            _mysprite[i].color = Color.white;
        }

        await UniTask.WaitForSeconds(0.1f, cancellationToken: this.GetCancellationTokenOnDestroy());
    }
}