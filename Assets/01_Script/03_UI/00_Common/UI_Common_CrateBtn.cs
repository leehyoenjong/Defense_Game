using UnityEngine;

public class UI_Common_CrateBtn : MonoBehaviour
{
    [SerializeField] GameObject _cratepopup;

    public void Btn_Create()
    {
        Instantiate(_cratepopup, null);
    }
}
