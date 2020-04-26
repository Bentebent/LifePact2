using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IBuffable
{
    public bool HasAggro => _state == AIState.ATTACKING;

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
    protected GameObject _target = null;

    protected float _currentHealth = 0.0f;
    protected AIState _state = AIState.IDLE;

    protected LayerMask _aggroLayerMask = 0;

    protected void Awake()
	{
        _currentHealth = _maxHealth;
        _player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        _aggroLayerMask = Layers.CombinedLayerMask(Layers.WallCollision, Layers.Friendly, Layers.Player);
    }

    protected void Start()
    {
        
    }

    protected void Update()
    {
        
    }

    protected void FixedUpdate()
    {
        switch (_state)
        {
            case AIState.ATTACKING:

                bool targetVisible = TargetIsVisible(_target, _aggroDistance);
                _navigation.MoveTo(_target, targetVisible);

                if (!targetVisible && !_navigation.HasPath)
                {
                    _navigation.Stop();
                }

                break;
            case AIState.IDLE:
                if (!CheckAggro())
                {
                    if (_navigation.AtDestination())
                    {
                        _navigation.Stop();
                    }
                }
                break;
            case AIState.DEAD:
                break;
            default:
                break;
        }
    }

    protected virtual bool CheckAggro()
    {
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance < _aggroDistance && TargetIsVisible(_player.gameObject, _aggroDistance))
        {
            AggroTarget(true, 1);
            return true;
        }

        for(int i = 0; i < _player.Minions?.Count; i++)
        {
            distance = Vector3.Distance(transform.position, _player.transform.position);
            if (distance < _aggroDistance && TargetIsVisible(_player.Minions[i], _aggroDistance))
            {
                AggroTarget(true, 1);
                return true;
            }
        }

        return false;
    }

    protected virtual void AggroTarget(bool aggroSurrounding, int recursions = 0)
    {
        List<GameObject> targets = new List<GameObject>(_player.Minions);
        targets.Add(_player.gameObject);

        _state = AIState.ATTACKING;
        _target = GetRandomTarget(targets);

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
                        x.AggroTarget(recursions > 0, rec);
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

    protected virtual bool TargetIsVisible(GameObject target, float viewDistance)
    {
        if (_state == AIState.ATTACKING)
        {
            viewDistance *= 3.0f;
        }

        return IsVisible(viewDistance, target.transform.position.ToVector2(), _aggroLayerMask, 
            new List<int> { Layers.Friendly, Layers.Player });
    }

    protected GameObject GetClosestTarget(List<GameObject> targets)
    {
        GameObject bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (GameObject potentialTarget in targets)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }

        return bestTarget;
    }

    protected GameObject GetRandomTarget(List<GameObject> targets)
    {
        return targets[Random.Range(0, targets.Count)];
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
