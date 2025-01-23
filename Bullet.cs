using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float timeToDestroy;
    [HideInInspector] public Weapon weapon;
    [SerializeField] GameObject hitEffectPrefab;

    void Start()
    {
        Destroy(this.gameObject, timeToDestroy);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        if (collision.gameObject.tag == "Zombie")
        {
            Zombie pl = collision.gameObject.GetComponentInParent<Zombie>();
            if (pl && pl.isAlive) pl.TakeDamage(weapon.damage);
        }

        ContactPoint contact = collision.GetContact(0);
        GameObject hitEffect = Instantiate(hitEffectPrefab, contact.point, Quaternion.identity);
        Transform hitPart = contact.otherCollider.transform;
        hitEffect.transform.SetParent(hitPart);

        Destroy(hitEffect, 5.0f);

        if (rb) rb.isKinematic = false;
        Destroy(this.gameObject);
    }
}