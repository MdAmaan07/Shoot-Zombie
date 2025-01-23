using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultBullet : MonoBehaviour
{
    [SerializeField] float timeToDestroy;
    [HideInInspector] public Weapon weapon;
    public GameObject[] hitEffectPrefab;
    GameObject hitEffect;

    void Start()
    {
        Destroy(this.gameObject, timeToDestroy);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        ContactPoint contact = collision.GetContact(0);

        if (collision.gameObject.tag == "Zombie")
        {
            Zombie pl = collision.gameObject.GetComponent<Zombie>();
            if (pl && pl.isAlive) pl.TakeDamage(weapon.damage);

            hitEffect = Instantiate(hitEffectPrefab[1], contact.point, Quaternion.identity);
        }
        else
        {
            hitEffect = Instantiate(hitEffectPrefab[0], contact.point, Quaternion.identity);
        }
        Destroy(hitEffect, 2.0f);

        if (rb) rb.isKinematic = false;
        Destroy(this.gameObject);
    }
}