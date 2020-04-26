using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IBuffable
{
    public bool HasAggro => _hasAggro;

    [SerializeField]
    protected float _maxHealth = 0.0f;

    [SerializeField]
    protected float _maxSpeed = 0.0f;

    [SerializeField]
    protected float _maxIdleSpeed = 0.0f;

    [SerializeField]
    protected float _acceleration = 0.0f;

    [SerializeField]
    protected float _deacceleration = 0.0f;

    [SerializeField]
    protected float _aggroDistance = 0.0f;

    [SerializeField]
    protected float _meleeDamage = 0.0f;

    [SerializeField]
    protected Rigidbody2D _rigidbody = null;

    [SerializeField]
    protected Collider2D _collider = null;

    [SerializeField]
    protected Navigation _navigation = null;

    protected Player _player = null;
    protected IBuffable _target = null;

    protected float _currentHealth = 0.0f;
    protected bool _hasAggro = false;
    protected AIState _state = AIState.IDLE;

    protected LayerMask _aggroLayerMask = 0;

    protected void Awake()
	{
        _currentHealth = _maxHealth;
        _player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        _aggroLayerMask = Layers.CombinedLayerMask(Layers.Map, Layers.Friendly, Layers.Player);
    }

    protected void Start()
    {
        
    }

    protected void Update()
    {
        
    }

    protected void FixedUpdate()
    {
        
    }

    protected virtual bool CheckAggro()
    {
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance < _aggroDistance && PlayerIsVisible(_aggroDistance))
        {
            AggroPlayer(true, 1);
        }

        return _hasAggro;
    }

    private void AggroPlayer(bool aggroSurrounding, int recursions = 0)
    {
        _hasAggro = true;
        //_target = _player.gameObject;

        if (aggroSurrounding)
        {
            List<Enemy> enemies = MapManager.Instance.Map.GetEnemiesInCircle(transform.position, _aggroDistance);
            int layerMask = Layers.CombinedLayerMask(Layers.Map, Layers.Enemy);
            int rec = recursions - 1;
            _collider.enabled = false;

            enemies.ForEach(x =>
            {
                if (!x.HasAggro)
                {
                    if (IsVisible(_aggroDistance * 3.0f, x.transform.position.ToVector2(), layerMask,
                        new List<int>() { Layers.Enemy }))
                    {
                        x.AggroPlayer(recursions > 0, rec);
                    }
                }
            });

            _collider.enabled = true;
        }
    }

    protected bool IsVisible(float viewDistance, Vector2 target, int layerMask, List<int> layers)
    {
        Vector2 origin = transform.position.ToVector2();

        if ((target - origin).magnitude <= viewDistance)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, (target - origin).normalized, viewDistance, layerMask);

            if (hit.collider != null && layers.Contains(hit.collider.gameObject.layer))
            {
                return true;
            }
        }

        return false;
    }

    protected virtual bool PlayerIsVisible(float viewDistance)
    {
        if (_hasAggro)
        {
            viewDistance *= 3.0f;
        }

        return IsVisible(viewDistance, _player.transform.position.ToVector2(), _aggroLayerMask, new List<int> { Layers.Friendly, Layers.Player });
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
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
