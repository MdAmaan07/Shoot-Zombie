using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class People : MonoBehaviour
{
    public GameObject player;
    private float rangeDis;
    private float minDis;
    public bool isAlive;
    private NavMeshAgent peopleAgent;
    public Transform lookAt;
    private bool foundPlayer;
    private float distancePlayer;
    private Animator anim;
    private float health;

    private Vector3 lastPosition;
    private float stuckCheckTimer = 2f;
    private float lastCheckTimer = 0f;
    private float stuckThreshold = 0.5f;
    private float visionRadius = 3f;
    private AudioSource audioSource;
    SpawnItem item;

    private void Awake()
    {
        rangeDis = 15f;
        minDis = 5f;
        health = 100f;
        isAlive = true;
        foundPlayer = false;
        distancePlayer = Mathf.Infinity;
        peopleAgent = GetComponent<NavMeshAgent>();
        item = GetComponentInParent<SpawnItem>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = item.player;
        lookAt = item.look;
    }

    private void Update()
    {
        if(!isAlive) return;
        distancePlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distancePlayer <= rangeDis || foundPlayer) FollowPlayer();
        else if (item && item.scream) Scream();
        else Roam();
    }

    private void Scream()
    {
        audioSource.PlayOneShot(item.help);
        anim.SetBool("Walk", false);
        anim.SetTrigger("Scream");
    }

    private void Roam()
    {
        RunAway();
    }

    private void FollowPlayer()
    {
        if(!foundPlayer) foundPlayer = true;
        peopleAgent.speed = 3f;

        if (distancePlayer <= minDis)
        {
            anim.SetBool("Walk", false);
            peopleAgent.ResetPath();
            peopleAgent.velocity = Vector3.zero;
        }
        else
        {
            anim.SetBool("Walk", true);
            peopleAgent.SetDestination(player.transform.position);
            transform.LookAt(lookAt);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) PeopleDie();
        else RunAway();
    }

    private void PeopleDie()
    {
        if (!isAlive) return;
        isAlive = false;

        item.RemoveDied(this.gameObject);
        anim.SetBool("Walk", false);
        anim.SetBool("Die", true);

        ResetZombie();
    }

    private void ResetZombie()
    {
        peopleAgent.ResetPath();
        peopleAgent.isStopped = true;
        peopleAgent.velocity = Vector3.zero;
        peopleAgent.enabled = false;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.direction = 2;
            capsule.center = new Vector3(capsule.center.x, 0.3f, -1f);
        }
    }

    private void RunAway()
    {
        peopleAgent.speed = 3f;
        anim.SetBool("Walk", true);

        if (!peopleAgent.hasPath || peopleAgent.isPathStale)
        {
            Vector3 randomDirection = Random.insideUnitSphere * visionRadius + transform.position;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomDirection, out hit, visionRadius, NavMesh.AllAreas))
            {
                peopleAgent.SetDestination(hit.position);
            }
        }
        else if (peopleAgent.remainingDistance <= 1f)
        {
            peopleAgent.ResetPath();
            peopleAgent.velocity = Vector3.zero;
        }

        CheckIfStuck();
    }

    private void CheckIfStuck()
    {
        if (Time.time - lastCheckTimer >= stuckCheckTimer)
        {
            lastCheckTimer = Time.time;
            if (Vector3.Distance(transform.position, lastPosition) <= stuckThreshold)
            {
                RunAway();
            }
            lastPosition = transform.position;
        }
    }
}
