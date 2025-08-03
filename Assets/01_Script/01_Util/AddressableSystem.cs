using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;

public class AddressableSystem
{
    public static async UniTask<T> LoadAsync<T>(string key)
    {
        try
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            var result = await handle.Task; // 직접 await

            if (result == null)
            {
                Debug.LogWarning($"어드레서블 키에 해당하는 에셋이 null입니다: {key}");
                return default;
            }

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"어드레서블 로드 중 오류 발생: {key}, 오류: {e.Message}");
            return default;
        }
    }
}