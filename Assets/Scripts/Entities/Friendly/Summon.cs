using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon : MonoBehaviour, IBuffable
{
    [SerializeField]
    protected Navigation _navigation = null;

    [SerializeField]
    protected float _idleMaxSpeed = 0.0f;

    [SerializeField]
    protected float _maxSpeed = 0.0f;

    protected GameObject _owner = null;
    protected AIState _state = AIState.IDLE;
    protected float _timeLastTarget = 0.0f;

	private void Awake()
	{
        _state = AIState.IDLE;
	}
	
    private void Start()
    {
        
    }

    private void Update()
    {
        switch (_state)
        {
            case AIState.ATTACKING:
                _navigation.SetMaxSpeed(_maxSpeed);

                break;
            case AIState.IDLE:
                if (_navigation.AtDestination() && _timeLastTarget == float.MaxValue)
                {
                    _timeLastTarget = Time.time;
                    _navigation.Stop();
                }

                if (Vector3.Distance(transform.position, _owner.transform.position) > 4.0f)
                {
                    _navigation.SetMaxSpeed(_maxSpeed);
                    _navigation.MoveTo(_owner.transform.position.ToVector2() + RandomHelper.RandomPointInCircle(0.5f), true);
                    _timeLastTarget = float.MaxValue;
                }
                else if (Time.time - _timeLastTarget > UnityEngine.Random.Range(6.0f, 10.0f))
                {
                    _navigation.SetMaxSpeed(_idleMaxSpeed);
                    _navigation.MoveTo(transform.position.ToVector2() + RandomHelper.RandomPointInCircle(2.0f), true);
                    _timeLastTarget = float.MaxValue;
                }

                break;
            case AIState.DEAD:
                break;
            default:
                break;
        }
    }

    public void SetOwner(GameObject owner)
    {
        _owner = owner;
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
