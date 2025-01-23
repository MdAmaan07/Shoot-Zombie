using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    private float health;
    [SerializeField] Animator animator;
    public bool isAlive;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource hitSource;
    [SerializeField] AudioSource healSource;
    [SerializeField] AudioClip[] audioPlayer;
    [SerializeField] MainMenu menu;
    public int Alert = 30;
    [SerializeField] Image bar;
    public GameObject defeat;
    public Transform spawnPoint;
    public SpawnZombie spawnZombie;
    private bool hitSound;
    private BombLaunch bomb;
    public bool healthStop;

    public GameObject healthBox;
    public GameObject grenadeBox;
    public GameObject healthTime;
    public GameObject cancelTime;
    public int totalHealth;
    private PlayerCamera playerCamera;
    private PlayerMove playerMove;
    public Text countDown;
    public Text healthText;
    private Coroutine healthCoroutine;

    private void Awake()
    {
        SetPlayerSpawn();
        totalHealth = 3;
        health = 120f;
        isAlive = true;
        animator = GetComponent<Animator>();
        bomb = GetComponent<BombLaunch>();
        playerMove = GetComponent<PlayerMove>();
        playerCamera = GetComponent<PlayerCamera>();
        SetHealthBar(health);
        defeat.SetActive(false);
    }

    private void SetPlayerSpawn()
    {
        int index = Random.Range(0, spawnPoint.childCount);
        Transform selectedSpawnPoint = spawnPoint.GetChild(index);
        Transform playerBody = transform;
        playerBody.position = selectedSpawnPoint.position;
        playerBody.rotation = selectedSpawnPoint.rotation;
        spawnZombie.StorePeople(this.gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (!isAlive) return;
        audioSource.PlayOneShot(audioPlayer[1]);
        if (!hitSound)
        {
            hitSound = true;
            hitSource.PlayOneShot(audioPlayer[4]);
            StartCoroutine(HitSound());
        }

        health -= damage;
        SetHealthBar(health);
        if (health <= 0f) PlayerDie();
    }

    private void PlayerDie()
    {
        if (spawnZombie.playerWon) return;
        spawnZombie.RemovePeople(this.gameObject);
        isAlive = false;
        defeat.SetActive(true);
        menu.HideItem();
        animator.SetBool("Crouching", false);
        animator.SetBool("Running", false);
        animator.SetBool("Die", true);
    }

    private void SetHealthBar(float val)
    {
        float normalizedValue = Mathf.Clamp(val, 0, 120f);
        bar.fillAmount = (normalizedValue / 120f);

        if (normalizedValue <= Alert) bar.color = Color.red;
        else bar.color = Color.green;
    }

    private void Step()
    {
        audioSource.PlayOneShot(audioPlayer[0]);
    }

    private void PlayHeal()
    {
        animator.SetBool("Health", true);
        healSource.clip = audioPlayer[5];
        healSource.Play();
    }

    public void PlayGun()
    {
        healSource.PlayOneShot(audioPlayer[6]);
    }

    private void StopHeal()
    {
        healSource.Stop();
    }

    private void Die()
    {
        audioSource.PlayOneShot(audioPlayer[2]);
    }

    private void JumpSound()
    {
        audioSource.PlayOneShot(audioPlayer[3]);
    }

    IEnumerator HitSound()
    {
        yield return new WaitForSeconds(2f);
        hitSound = false;
    }

    private void IncreaseHealth()
    {
        CoroutineNull();
        health += 60f;
        if (health > 120f) health = 120f;
        SetHealthBar(health);
        healthStop = false;
        ManageButton(true, false);
        totalHealth -= 1;
        healthText.text = totalHealth.ToString();
        playerMove.HealthActive(true);
    }

    private void CoroutineNull()
    {
        animator.SetBool("Health", false);
        if (healthCoroutine != null)
        {
            StopCoroutine(healthCoroutine);
            healthCoroutine = null;
        }
    }

    public void Health()
    {
        if (totalHealth <= 0 || playerMove.switching || playerMove.IsReloading() || health >= 120f) return;
        if (healthStop) Cancel();
        else
        {
            if (healthCoroutine != null) StopCoroutine(healthCoroutine);
            PlayHeal();
            playerMove.HealthActive(false);
            if (playerCamera && playerCamera.isAim) playerCamera.ScopeZoom();
            ManageButton(false, true);
            healthStop = true;
            healthCoroutine = StartCoroutine(Increase());
        }
    }

    IEnumerator Increase()
    {
        float tm = 5f;
        countDown.text = "";
        while (tm > 0 && healthStop)
        {
            countDown.text = tm.ToString();
            yield return new WaitForSeconds(1f);
            tm -= 1f;
        }
        if(healthStop) IncreaseHealth();
    }

    public void Cancel()
    {
        CoroutineNull();
        StopHeal();
        healthStop = false;
        ManageButton(true, false);
        playerMove.HealthActive(true);
        SetHealthBar(health);
    }

    public void ChangeItem()
    {
        if (healthStop || bomb.bombIns) return;
        if (bomb.bombTaken)
        {
            bomb.bombTaken = false;
            playerMove.WeaponActive();
        }
        healthBox.SetActive(!healthBox.activeSelf);
        grenadeBox.SetActive(!grenadeBox.activeSelf);
    }

    private void ManageButton(bool flag1, bool flag2)
    {
        healthTime.SetActive(flag1);
        cancelTime.SetActive(flag2);
    }

    public void HealthItem()
    {
        totalHealth += 1;
        healthText.text = totalHealth.ToString();
    }

    public void StopFire()
    {
        if(playerMove) playerMove.WeaponOff();
    }
}
