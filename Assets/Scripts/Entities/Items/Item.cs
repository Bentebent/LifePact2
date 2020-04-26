using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField]
    private AudioSource _pickupSound;

    [SerializeField]
    private bool _destroyOnPickup = true;

    [SerializeField]
    private GameObject _visual = null;

    protected virtual void Apply(GameObject owner)
    {
        Player player = owner.GetComponent<Player>();

        if (player != null)
        {
            ApplyEffect(player);
            _pickupSound?.Play();

            if (_destroyOnPickup)
            {
                _visual.SetActive(false);
                Destroy(gameObject, _pickupSound.clip.length);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            Apply(collision.gameObject);
        }
    }

    protected abstract void ApplyEffect(Player owner);
}
