using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidbody;

    [SerializeField]
    private float _maxSpeed = 0.0f;

    [SerializeField]
    private float _acceleration = 0.0f;

    [SerializeField]
    private float _deceleration = 0.0f;

    private Vector3 _inputVector = Vector3.zero;
    private Vector3 _velocity = Vector3.zero;

    private void Awake()
    {

    }

    private void Start()
    {

    }

    private void Update()
    {
        CalculateInputVector();
    }

    private void FixedUpdate()
    {
        CalculateDeceleration();
        CalculateVelocity();
    }

    private void CalculateInputVector()
    {
        _inputVector = Vector3.zero;
        _inputVector.x -= Keybindings.MoveLeft;
        _inputVector.x += Keybindings.MoveRight;
        _inputVector.y += Keybindings.MoveUp;
        _inputVector.y -= Keybindings.MoveDown;
    }

    private void CalculateVelocity()
    {
        _velocity += _inputVector.normalized * _acceleration * Time.deltaTime;

        if (_velocity.magnitude > _maxSpeed)
        {
            _velocity = _velocity.normalized * _maxSpeed;
        }

        _rigidbody.velocity = _velocity;
    }

    private void CalculateDeceleration()
    {
        if (_inputVector.x <= 0f && _velocity.x > 0f)
        {
            _velocity.x -= _deceleration * Time.deltaTime;
            if (_velocity.x < 0f)
            {
                _velocity.x = 0f;
            }
        }
        if (_inputVector.x >= 0f && _velocity.x < 0f)
        {
            _velocity.x += _deceleration * Time.deltaTime;
            if (_velocity.x > 0f)
            {
                _velocity.x = 0f;
            }
        }

        if (_inputVector.y <= 0f && _velocity.y > 0f)
        {
            _velocity.y -= _deceleration * Time.deltaTime;
            if (_velocity.y < 0f)
            {
                _velocity.y = 0f;
            }
        }
        if (_inputVector.y >= 0f && _velocity.y < 0f)
        {
            _velocity.y += _deceleration * Time.deltaTime;
            if (_velocity.y > 0f)
            {
                _velocity.y = 0f;
            }
        }
    }
}
