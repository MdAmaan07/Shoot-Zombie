using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Zombie : MonoBehaviour
{
    public bool isAlive;
    public NavMeshAgent enemyAgent;
    public Animator anim;
    private float health = 120f;
    private float visionRadius = 20f;

    private Vector3 lastPosition;
    private float stuckCheckTimer = 2f;
    private float lastCheckTimer = 0f;
    private float stuckThreshold = 1f;
    private float zombieTime;

    private Transform spawnPoint;

    public bool hasPunch;
    public GameObject player;
    public Transform headPos;
    public Transform lookAt;
    public bool getHit;
    public float currentTime;
    [SerializeField] AudioSource mouthSound;
    [SerializeField] AudioSource attackSound;
    [SerializeField] AudioSource walkSound;
    public ZombieAudio zombieAudio;
    public int soundIndex;
    private ZombieDetect zombieDetect;
    private SpawnZombie spawnZombie;
    private bool scream;
    private float distance;
    float currentdis = 3000f;
    float distanceNow = 3000f;
    float lastTime;
    public float waitTime;
    public float damage;
    private float maxDamage = 20f;
    public bool isPunch;
    private bool inScream;
    private bool checkPlayer;
    Rigidbody rb;

    private void Awake()
    {
        SetDistance();
        isAlive = true;
        zombieAudio = GetComponentInParent<ZombieAudio>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        enemyAgent = GetComponent<NavMeshAgent>();
        enemyAgent.SetDestination(transform.position);
        spawnZombie = GetComponentInParent<SpawnZombie>();
        zombieDetect = GetComponent<ZombieDetect>();
        spawnPoint = transform;
        anim.SetBool("Walk", true);
        zombieTime = Time.time;
        SetVolume();
        StopScreamSound();
        StopAttackSound();
        ZombieSound();
        SetDamage();
    }

    private void Update()
    {
        if (!isAlive || !zombieAudio.chaseStarted)
        {
            if(!isAlive) StopAllAudio();
            return;
        }

        GetClosest();
        checkPlayer = zombieDetect.PlayerSeen();
        if (checkPlayer || getHit)
        {
            if (checkPlayer && !getHit)
            {
                getHit = true;
                currentTime = Time.time;
            }
            HitPlayer();
        }
        else Roam();
    }

    private void FixedUpdate()
    {
        rb.velocity = Vector3.zero;
    }

    void Roam()
    {
        if (inScream) return;
        anim.SetBool("Run", false);
        anim.SetBool("Walk", true);

        enemyAgent.speed = 1f;
        if (!enemyAgent.hasPath || enemyAgent.isPathStale || enemyAgent.remainingDistance < 0.5f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * visionRadius;
            randomDirection += spawnPoint.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, visionRadius, NavMesh.AllAreas))
            {
                Vector3 finalPosition = hit.position;

                if (Vector3.Distance(transform.position, finalPosition) > 1f) enemyAgent.SetDestination(finalPosition);
                else Roam();
            }
        }
        CheckIfStuck();
    }

    private void CheckIfStuck()
    {
        if (Time.time - lastCheckTimer >= stuckCheckTimer)
        {
            lastCheckTimer = Time.time;
            if (Vector3.Distance(transform.position, lastPosition) < stuckThreshold) ChangeDestination();
            lastPosition = transform.position;
        }
    }

    private void ChangeDestination()
    {
        Vector3 oppositeDirection = (transform.position - lastPosition).normalized;
        Vector3 newDestination = transform.position + oppositeDirection * visionRadius;
        Vector3 clampedDestination = ClampToRadius(newDestination, spawnPoint.position, visionRadius);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(clampedDestination, out hit, visionRadius, NavMesh.AllAreas))
        {
            enemyAgent.SetDestination(hit.position);
        }
        else ChangeDestination();
    }

    private Vector3 ClampToRadius(Vector3 targetPosition, Vector3 center, float radius)
    {
        Vector3 direction = (targetPosition - center).normalized;
        if (Vector3.Distance(center, targetPosition) > radius)
        {
            return center + direction * radius;
        }
        return targetPosition;
    }

    public void TakeDamage(float damage)
    {
        if (!scream) StartCoroutine(Scream());
        NearbyZombies();
        if (getHit == false) getHit = true;

        currentTime = Time.time;
        health -= damage;

        if (health <= 0) ZombieDie();
    }

    public void ZombieDie()
    {
        if (!isAlive) return;
        StopAttackSound();
        StopScreamSound();
        isAlive = false;
        visionRadius = 0f;

        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
        anim.SetBool("Die", true);
        walkSound.PlayOneShot(zombieAudio.soundZombie[1]);
        if (!this.gameObject.name.StartsWith("Wolf") && !this.gameObject.name.StartsWith("Bear")) spawnZombie.ZombieKilled();

        ResetZombie();
    }

    private void ResetZombie()
    {
        getHit = false;
        enemyAgent.ResetPath();
        enemyAgent.isStopped = true;
        enemyAgent.velocity = Vector3.zero;
        enemyAgent.enabled = false;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.direction = 2;
            capsule.center = new Vector3(capsule.center.x, 0.3f, -1f);
        }
    }

    private void PunchPlayer()
    {
        enemyAgent.ResetPath();
        enemyAgent.velocity = Vector3.zero;
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);

        if (hasPunch) return;
        anim.SetTrigger("Attack");
    }

    private void HitPlayer()
    {
        if (lookAt == null) return;
        enemyAgent.speed = 4f;

        ContinueChase();
    }

    private void ContinueChase()
    {
        if (Time.time - currentTime > 30f && !checkPlayer)
        {
            enemyAgent.ResetPath();
            anim.SetBool("Run", false);
            anim.SetBool("Walk", true);
            scream = false;
            inScream = false;
            if (!this.gameObject.name.StartsWith("Wolf") && !this.gameObject.name.StartsWith("Bear")) getHit = false;
            StopAttackSound();
            ZombieSound();
            return;
        }
        else if (Vector3.Distance(transform.position, player.transform.position) < distance)
        {
            enemyAgent.SetDestination(player.transform.position);
            transform.LookAt(lookAt);

            PunchPlayer();
        }
        else
        {
            enemyAgent.SetDestination(player.transform.position);
            if (!scream) StartCoroutine(Scream());
            else
            {
                anim.SetBool("Walk", false);
                anim.SetBool("Run", true);
            }
        }
        if (!this.gameObject.name.StartsWith("Wolf") && !this.gameObject.name.StartsWith("Bear"))
        {
            if (!inScream)
            {
                StopScreamSound();
                AttackSound();
            }
        }
    }

    IEnumerator Scream()
    {
        scream = true;
        inScream = true;
        StopScreamSound();
        StopAttackSound();
        HandleSound();
        yield return new WaitForSeconds(2f);
        ScreamFalse();
    }

    private void ScreamFalse()
    {
        inScream = false;
        if (!isAlive) return;
    }

    private void HandleSound()
    {
        if (this.gameObject.name.StartsWith("Wolf")) walkSound.PlayOneShot(zombieAudio.soundZombie[7]);
        else if (this.gameObject.name.StartsWith("Bear")) walkSound.PlayOneShot(zombieAudio.soundZombie[8]);
        else walkSound.PlayOneShot(zombieAudio.soundZombie[2]);
    }

    private void StopScreamSound()
    {
        if (!mouthSound.isPlaying || this.gameObject.name.StartsWith("Wolf") || this.gameObject.name.StartsWith("Bear")) return;
        mouthSound.loop = false;
        mouthSound.Stop();
    }

    private void StopAttackSound()
    {
        if (!attackSound.isPlaying) return;
        attackSound.loop = false;
        attackSound.Stop();
    }

    private void ZombieSound()
    {
        if (mouthSound.isPlaying) return;
        if (this.gameObject.name.StartsWith("Wolf") || this.gameObject.name.StartsWith("Bear")) mouthSound.clip = zombieAudio.soundZombie[0];
        else mouthSound.clip = zombieAudio.soundZombie[soundIndex];
        mouthSound.loop = true;
        mouthSound.Play();
    }

    private void AttackSound()
    {
        if (attackSound.isPlaying) return;
        attackSound.clip = zombieAudio.soundZombie[2];
        attackSound.loop = true;
        attackSound.Play();
    }

    private void StepSound()
    {
        walkSound.PlayOneShot(zombieAudio.soundZombie[0]);
    }

    private void NearbyZombies()
    {
        if (Time.time - zombieTime < 2f) return;

        float detectionRadius = 5f;
        int layerMask = 1 << LayerMask.NameToLayer("Zombie");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, layerMask);

        foreach (Collider collider in hitColliders)
        {
            Zombie nearbyZombie = collider.GetComponent<Zombie>();
            if (nearbyZombie != null && nearbyZombie != this)
            {
                nearbyZombie.SetHit();
            }
        }
        zombieTime = Time.time;
    }

    public void SetHit()
    {
        getHit = true;
    }
    
    private void SetVolume()
    {
        float vol = PlayerPrefs.GetFloat("Volume");
        mouthSound.volume = vol;
        attackSound.volume = vol;
        walkSound.volume = vol;

        spawnZombie.SetAudio(mouthSound, walkSound, attackSound);
    }

    private void SetDistance()
    {
        if (this.gameObject.name.StartsWith("Wolf") || this.gameObject.name.StartsWith("Bear")) distance = 2f;
        else distance = 1f;
    }

    private void StopAllAudio()
    {
        mouthSound.loop = false;
        walkSound.loop = false;
        attackSound.loop = false;
        mouthSound.Stop();
        walkSound.Stop();
        attackSound.Stop();
    }

    private void GetClosest()
    {
        if (Time.time - lastTime < 2f) return;
        distanceNow = 3000f;

        for (int i = 0; i < spawnZombie.people.Count; i++)
        {
            currentdis = Vector3.Distance(transform.position, spawnZombie.people[i].transform.position);
            if (currentdis < distanceNow)
            {
                player = spawnZombie.people[i];
                distanceNow = currentdis;
                lookAt = spawnZombie.lookPoint[i];
                headPos = spawnZombie.headPos[i];
            }
        }
        lastTime = Time.time;
    }

    private void SetDamage()
    {
        int level = PlayerPrefs.GetInt("Level");
        int diff = PlayerPrefs.GetInt("Difficulty");
        if (PlayerPrefs.HasKey("Level"))
        {
            damage = Mathf.Min(9f + level + diff, maxDamage);
        }
        if (this.gameObject.name.StartsWith("Wolf") || this.gameObject.name.StartsWith("Bear")) damage = 15f;
        else damage = 10f;
    }

    private void SetPunch()
    {
        if (!hasPunch) hasPunch = true;
        else
        {
            hasPunch = false;
            isPunch = false;
        }
    }

    public void UpdateDistance(float dis)
    {
        zombieAudio.distance = dis;
    }
}
