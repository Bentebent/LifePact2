using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _spriteRenderer = null;

    [SerializeField]
    private Sprite _nonTriggeredSprite = null;

    [SerializeField]
    private Sprite _triggeredSprite = null;

    [SerializeField]
    private AudioSource _audioSource = null;

    [SerializeField]
    private AudioClip _triggerSound = null;

    [SerializeField]
    private AudioClip _resetSound = null;

    [SerializeField]
    private Bounds _bounds = new Bounds();

    [SerializeField]
    private bool _resetOnExit = false;

    [SerializeField]
    private bool _triggerOnInvulnerable = false;

    [SerializeField]
    private float _resetTime = 0.0f;

    public Bounds Bounds => _bounds;

    protected bool _triggered = false;
    protected float _triggerTimer = 0.0f;

    protected virtual void Awake()
	{
		
	}

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        if (_triggered && _resetOnExit && _triggerTimer > 0.0f)
        {
            _triggerTimer -= Time.deltaTime;

            if (_triggerTimer <= 0.0f)
            {
                ResetTrap();
                _triggered = false;
                _triggerTimer = 0.0f;
            }
        }
    }

    public virtual void Apply(GameObject gameObject)
    {
        if (_triggered)
        {
            return;
        }

        IBuffable go = gameObject.GetComponent<IBuffable>();
        if (go != null/* && (!player.IsInvulnerable || _triggerOnInvulnerable)*/)
        {
            ApplyEffect(go);
        }
    }

    public virtual void ApplyEffect(IBuffable go)
    {
        _audioSource.PlayOneShot(_triggerSound);
        _spriteRenderer.sprite = _triggeredSprite;
        _triggered = true;
    }

    protected virtual void ResetTrap()
    {
        _audioSource.PlayOneShot(_resetSound);
        _spriteRenderer.sprite = _nonTriggeredSprite;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            Apply(collision.gameObject);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject && _resetOnExit && _triggered)
        {
            _triggerTimer = _resetTime;
        }
    }
}
