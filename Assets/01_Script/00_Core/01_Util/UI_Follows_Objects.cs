using UnityEngine;

public class UI_Follows_Objects : MonoBehaviour
{
    [SerializeField] RectTransform[] _myui;
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

        var maxcount = _myui.Length;
        for (int i = 0; i < maxcount; i++)
        {
            _myui[i].position = _maincamera.WorldToScreenPoint(_targetobject.position) + _offset;
        }
    }
}