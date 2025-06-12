using UnityEngine;

public class UI_Follows_Object : MonoBehaviour
{
    [SerializeField] RectTransform _myui;
    [SerializeField] Transform _targetobject;
    [SerializeField] Vector3 _offset;
    Camera _maincamera;

    void Start()
    {
        _maincamera = Camera.main;
    }

    void LateUpdate()
    {
        if (_targetobject == null || _maincamera == null)
        {
            return;
        }

        _myui.position = _maincamera.WorldToScreenPoint(_targetobject.position) + _offset;
    }
}