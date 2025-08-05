using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DownLoadPopup : MonoBehaviour
{
    [SerializeField] GameObject _downloadpanel;
    [SerializeField] TextMeshProUGUI _downloadsize;
    [SerializeField] TextMeshProUGUI _downloadper;
    [SerializeField] Image _per;

    bool _isdownloadcompleted;

    public async UniTask<bool> Setting(long downloadsize)
    {
        _downloadsize.text = FormatFileSize(downloadsize);
        _downloadpanel.SetActive(false);
        await UniTask.WaitUntil(() => _isdownloadcompleted == true);
        Destroy(this.gameObject, 0.1f);
        return true;
    }

    public async void Btn_DownLoad()
    {
        // 진행률 초기화
        UpdateProgress(0f);
        _downloadsize.gameObject.SetActive(false);
        _downloadpanel.SetActive(true);
        var result = await AddressableSystem.DownLoadData("DATA", UpdateProgress);
        _isdownloadcompleted = result;
    }

    public void Btn_Exit()
    {
        Application.Quit();
    }

    void UpdateProgress(float progress)
    {
        // 퍼센트 텍스트 업데이트 (0% ~ 100%)
        int percentage = Mathf.RoundToInt(progress * 100f);
        _downloadper.text = $"{percentage}%";

        // 이미지 Fill Amount 업데이트 (0.0f ~ 1.0f)
        _per.fillAmount = progress;
    }

    string FormatFileSize(long bytes)
    {
        if (bytes < 1000)
        {
            return $"{bytes}Byte";
        }
        else if (bytes < 1000000) // 1000KB 미만
        {
            float kb = bytes / 1000f;
            return $"{kb:F1}KB";
        }
        else if (bytes < 1000000000) // 1000MB 미만
        {
            float mb = bytes / 1000000f;
            return $"{mb:F1}MB";
        }
        else
        {
            float gb = bytes / 1000000000f;
            return $"{gb:F1}GB";
        }
    }
}