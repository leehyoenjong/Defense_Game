using UnityEngine;

public class UI_CreateUIBtn : MonoBehaviour
{
    [SerializeField] GameObject _popup;
    public void Btn_Create()
    {
        Instantiate(_popup, null);
    }
}