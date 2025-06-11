using System;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    public event Action _move_event;
    [SerializeField] float _speed;

    Vector2? _targetPosition = null;

    void FixedUpdate()
    {
        if (!_targetPosition.HasValue)
        {
            return;
        }
        MoveToUpdate();
    }

    public virtual void MoveToTarget(Vector2 target)
    {
        _targetPosition = target;
        _move_event?.Invoke();
    }
    
    public void ReSetting()
    {
        _targetPosition = null;
    }

    void MoveToUpdate()
    {
        Vector2 currentPosition = transform.position;
        Vector2 target = _targetPosition.Value;
        if (Vector2.Distance(currentPosition, target) > 0.01f)
        {
            Vector2 newPosition = Vector2.MoveTowards(currentPosition, target, _speed * Time.fixedDeltaTime);
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(target.x, target.y, transform.position.z);
            _targetPosition = null;
        }
    }
}