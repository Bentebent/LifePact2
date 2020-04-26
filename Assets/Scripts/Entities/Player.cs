using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour, IBuffable
{
    [SerializeField]
    private Rigidbody2D _rigidbody;

    [SerializeField]
    private float _maxSpeed = 0.0f;

    [SerializeField]
    private float _acceleration = 0.0f;

    [SerializeField]
    private float _deceleration = 0.0f;

    [SerializeField]
    private float _viewDistance = 0.0f;

    private Vector3 _inputVector = Vector3.zero;
    private Vector3 _velocity = Vector3.zero;

    private List<KeyType> _keys = null;

    private void Awake()
    {
        _keys = new List<KeyType>();
    }

    private void Start()
    {

    }

    private void Update()
    {
        CalculateInputVector();

        MapManager.Instance.UpdateFogOfWar(transform.position, _viewDistance);
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

    public void AddKey(KeyType type)
    {
        _keys.Add(type);
    }

    public bool UseKey(KeyType type)
    {
        if (_keys.Contains(type))
        {
            _keys.Remove(type);
            return true;
        }

        return false;
    }

    public bool ReceiveDamage(int damage, Vector2 velocity, bool maxHealth = false, bool spawnBloodSpray = true)
    {
        throw new System.NotImplementedException();
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        throw new System.NotImplementedException();
    }

    public void HandleStatusEffects()
    {
        throw new System.NotImplementedException();
    }
}
