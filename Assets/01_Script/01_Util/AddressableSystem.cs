using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;

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

    public static async UniTask<bool> CheckDownLoadSize(string key, Func<long> downloadsizecheck)
    {
        try
        {
            AsyncOperationHandle<long> getdownloadsize = Addressables.GetDownloadSizeAsync(key);
            await getdownloadsize;
            downloadsizecheck = () => getdownloadsize.Result;

            //메모리 해제
            Addressables.Release(getdownloadsize);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"다운로드 체크에 실패 했습니다. : {e}");
            return false;
        }
    }

    public static async UniTask<bool> DownLoadData(string key, Action<float> onProgress = null)
    {
        try
        {
            var handle = Addressables.DownloadDependenciesAsync(key);

            // 진행률 업데이트
            while (!handle.IsDone)
            {
                onProgress?.Invoke(handle.PercentComplete);
                await UniTask.Yield();
            }

            // 다운로드 완료 시 100% 전달
            onProgress?.Invoke(1.0f);
            
            await handle;
            await UniTask.WaitForSeconds(0.1f);

            //메모리 해제
            Addressables.Release(handle);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"다운로드에 실패 했습니다. : {e}");
            return false;
        }
    }
}