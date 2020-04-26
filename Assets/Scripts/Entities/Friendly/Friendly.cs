using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friendly : MonoBehaviour, IBuffable
{
    [SerializeField]
    protected Navigation _navigation = null;

    [SerializeField]
    private float _idleMaxSpeed = 0.0f;

    [SerializeField]
    private float _maxSpeed = 0.0f;

    public Player _owner;
    protected AIState _state;

    private float _timeLastTarget = 0.0f;

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
                    _navigation.MoveTo(_owner.transform.position.ToVector2() + RandomPointInCircle(3.5f), true);
                    _timeLastTarget = float.MaxValue;
                }
                else if (Time.time - _timeLastTarget > UnityEngine.Random.Range(6.0f, 10.0f))
                {
                    _navigation.SetMaxSpeed(_idleMaxSpeed);
                    _navigation.MoveTo(transform.position.ToVector2() + RandomPointInCircle(2.0f), true);
                    _timeLastTarget = float.MaxValue;
                }

                break;
            case AIState.DEAD:
                break;
            default:
                break;
        }
    }

    protected Vector2 RandomPointInCircle(float radius)
    {
        float r = radius * Mathf.Sqrt(UnityEngine.Random.Range(0.0f, 1.0f));
        float theta = UnityEngine.Random.Range(0.0f, 1.0f) * 2 * Mathf.PI;

        float x = r * Mathf.Cos(theta);
        float y = r * Mathf.Sin(theta);

        return new Vector2(x, y);
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
