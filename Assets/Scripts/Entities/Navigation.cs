using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidbody = null;

    [SerializeField]
    private float _maxSpeed = 0.0f;

    [SerializeField]
    private float _acceleration = 0.0f;

    [SerializeField]
    private float _deacceleration = 0.0f;

    private Vector2 _target = Vector2.zero;
    private Vector2 _destination = Vector2.zero;
    private bool _targetIsVisible = false;

    private float _minPathAge = 0.0f;
    private float _currentPathAge = 0.0f;
    private List<Vector2Int> _path = null;

    public bool HasPath => (_path != null && _path.Count > 0) || (_currentPathAge >= _minPathAge);

    private void Awake()
	{
        _minPathAge = Random.Range(3.0f, 5.0f);
        _currentPathAge = _minPathAge;
        _path = new List<Vector2Int>();

        _target = Vector2.zero;
    }
	
    private void Start()
    {
    }

    private void Update()
    {
    }

    public void SetMaxSpeed(float maxSpeed)
    {
        _maxSpeed = Mathf.Max(0f, maxSpeed);
    }

    public void SetAcceleration(float acceleration)
    {
        _acceleration = Mathf.Max(0.1f, acceleration);
    }

    public void MoveTo(GameObject target, bool targetIsVisible)
    {
        MoveTo(target.transform.position.ToVector2(), targetIsVisible);
    }

    public void MoveTo(Vector2 target, bool targetIsVisible)
    {
        _target = target;
        _targetIsVisible = targetIsVisible;
    }

    public bool AtDestination(float stoppingDistance = 1.0f)
    {
        return (_target.ToVector3() - transform.position).magnitude <= stoppingDistance;
    }

    public void Stop()
    {
        _target = Vector2.zero;
        _targetIsVisible = false;
    }

    private void FixedUpdate()
    {
        if (_target != Vector2.zero)
        {
            if (_targetIsVisible)
            {
                _path = null;
                _currentPathAge = _minPathAge;
                _destination = _target;
            }
            else
            {
                if (_currentPathAge >= _minPathAge)
                {
                    _path = MapManager.Instance.AStar(transform.position.ToVector2(), _target, out float distance);
                    _currentPathAge = 0.0f;

                    if (_path.Count > 0)
                    {
                        SetPathTarget();
                    }
                }
                else if (_path == null)
                {
                    _destination = _target;
                }
            }

            FollowPath();
            CalculateVelocity();
        }
        else
        {
            SmoothStop();
        }

        _currentPathAge += Time.deltaTime;
    }

    private void CalculateVelocity()
    {
        Vector2 direction = (_destination - transform.position.ToVector2()).normalized;
        _rigidbody.velocity += direction * _acceleration * Time.deltaTime;
        if (_rigidbody.velocity.magnitude > _maxSpeed)
        {
            _rigidbody.velocity = _rigidbody.velocity.normalized * _maxSpeed;
        }

        float angleToDir = Vector2.SignedAngle(_rigidbody.velocity, direction);
        _rigidbody.velocity = Quaternion.Euler(0.0f, 0.0f, angleToDir * 7.5f * Time.deltaTime) * _rigidbody.velocity;
    }

    private void SmoothStop()
    {
        if (_rigidbody.velocity.magnitude > 0.001f)
        {
            _rigidbody.velocity -= _rigidbody.velocity * _deacceleration * Time.deltaTime;
        }
        else
        {
            _rigidbody.velocity = Vector2.zero;
        }
    }

    private void FollowPath()
    {
        if (_path != null && _path.Count > 0
            && (_destination - MapManager.Instance.Map.WorldToCell(transform.position.ToVector2())).magnitude <= 1.0f)
        {
            SetPathTarget();
        }
    }

    private void SetPathTarget()
    {
        _destination = _path[0];
        _path.RemoveAt(0);
    }
}
