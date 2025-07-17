using UnityEngine;

public class UI_Title : MonoBehaviour
{
    [SerializeField] UI_Loading _loading;

    void Start()
    {
        //TODO 추후에 풀링이나, DataManager에 있는거 어드레서블로 가져올때 로딩하기
        _loading.UpdateGage(1);
    }
}