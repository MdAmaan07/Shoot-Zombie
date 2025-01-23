using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    [SerializeField] Zombie zombie;
    [SerializeField] GameObject hitEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (zombie.isPunch || !zombie.hasPunch || (other.gameObject.tag != "Player" && other.gameObject.tag != "People")) return;
        zombie.isPunch = true;
        if (other.gameObject.tag == "Player")
        {
            PlayerHealth pl = other.gameObject.GetComponent<PlayerHealth>();
            if (pl.isAlive) pl.TakeDamage(zombie.damage);
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            Instantiate(hitEffect, collisionPoint, Quaternion.identity);
        }

        if (other.gameObject.tag == "People")
        {
            People pl = other.gameObject.GetComponent<People>();
            if (pl.isAlive) pl.TakeDamage(zombie.damage);
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            Instantiate(hitEffect, collisionPoint, Quaternion.identity);
        }
    }
}
