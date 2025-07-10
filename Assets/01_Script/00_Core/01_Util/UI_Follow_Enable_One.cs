using UnityEngine;

public class UI_Follow_Enable_One : MonoBehaviour
{
    Camera _maincamera;

    [SerializeField] RectTransform _myui;

    [SerializeField] Vector3 _offset;

    [SerializeField] Transform _targetobject;

    void OnEnable()
    {
        if (_maincamera == null)
        {
            _maincamera = Camera.main;
        }

        if (_targetobject == null || _maincamera == null)
        {
            return;
        }

        _myui.position = _maincamera.WorldToScreenPoint(_targetobject.position) + _offset;
    }
}