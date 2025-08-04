using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UI_Title : MonoBehaviour
{
    [SerializeField] UI_Loading _loading;
    [SerializeField] GameObject _downloadpopup;

    async void Start()
    {
        var result = await AddressableSystem.CheckDownLoadSize("DATA");

        //게임 문제 발생
        if (result.Item1 == false)
        {
            //TODO:팝업창 띄울 것
            return;
        }
        _loading.UpdateGage(0.3f);

        if (result.Item2 > 0)
        {
            await CreateDownLoadPopUp(result.Item2);
        }

        await DataManager.instance.LoadTable();
        _loading.UpdateGage(1f);
    }

    async UniTask<bool> CreateDownLoadPopUp(long downalodsize)
    {
        var popup = Instantiate(_downloadpopup, null);
        var result = await popup.GetComponent<UI_DownLoadPopup>().Setting(downalodsize);
        return result;
    }
}