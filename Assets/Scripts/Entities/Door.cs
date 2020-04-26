using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D _trigger = null;

    [SerializeField]
    private BoxCollider2D _collider = null;

    [SerializeField]
    private Rigidbody2D _rigidBody = null;

    [SerializeField]
    private Animator _animator = null;

    [SerializeField]
    private AudioSource _audioSource = null;

    [SerializeField]
    private AudioClip _moving = null;

    [SerializeField]
    private AudioClip _accessDenied = null;

    [SerializeField]
    private bool _closeOnExit = true;

    [SerializeField]
    private bool _closed = true;

    [SerializeField]
    private bool _locked = true;

    public RectInt Bounds { get; set; }

    public KeyType KeyType;
    public List<Door> Siblings = new List<Door>();

    private void Awake()
	{
		
	}
	
    private void Start()
    {
        if (!_closed)
        {
            ToggleClosed(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleClosed(!_closed);
        }
    }

    public void Open(bool unlock)
    {
        _collider.enabled = false;
        MapManager.Instance.Map?.UpdateCollisionMap(Bounds, 0);

        if (unlock)
        {
            _locked = false;
        }

        _closed = false;
    }

    public void Close(bool lockDoor)
    {
        _collider.enabled = true;
        MapManager.Instance.Map?.UpdateCollisionMap(Bounds, 1);

        if (lockDoor)
        {
            _locked = true;
        }

        _closed = true;
    }

    public void ToggleClosed(bool closed)
    {
        if (!closed)
        {
            _animator.SetTrigger("OpenDoor");
        }
        else
        {
            _animator.SetTrigger("CloseDoor");
        }

        _audioSource.clip = _moving;
        _audioSource.Play();

        _closed = closed;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            if (!_locked && _closed)
            {
                ToggleClosed(false);

                Siblings.ForEach(x =>
                {
                    x.ToggleClosed(false);
                });
            }
            else if (_locked && _closed)
            {
                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null && Keybindings.Use)
                {
                    if (player.UseKey(KeyType))
                    {
                        ToggleClosed(false);
                        Siblings.ForEach(x =>
                        {
                            x.ToggleClosed(false);
                        });
                    }
                    else
                    {
                        _audioSource.clip = _accessDenied;
                        _audioSource.Play();
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_closed && _closeOnExit)
        {
            ToggleClosed(true);
        }
    }

}
