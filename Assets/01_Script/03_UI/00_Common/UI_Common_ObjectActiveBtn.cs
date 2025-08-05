using UnityEngine;

public class UI_Common_ObjectActiveBtn : MonoBehaviour
{
    [SerializeField] GameObject _activeobject;

    public void Btn_Active()
    {
        _activeobject.SetActive(!_activeobject.activeSelf);
    }
}
