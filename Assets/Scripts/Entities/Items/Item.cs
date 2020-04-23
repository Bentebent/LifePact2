using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField]
    private AudioSource _pickupSound;

    [SerializeField]
    private bool _destroyOnPickup = true;

    protected virtual void Apply(GameObject owner)
    {
        Player player = owner.GetComponent<Player>();

        if (player != null)
        {
            ApplyEffect(owner);
            _pickupSound?.Play();

            if (_destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            Apply(collision.gameObject);
        }
    }

    protected abstract void ApplyEffect(GameObject owner);
}
