using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Weapon : MonoBehaviour
{
    public AudioSource audioSource;
    [SerializeField] AudioClip[] gun;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform barrelPos;
    [SerializeField] float bulletVelocity;
    [SerializeField] int bulletsPerShot;

    public float damage;
    float fireRateTimer;
    [SerializeField] float fireRate;
    [SerializeField] PlayerMove plMove;
    private bool isShoot;

    [SerializeField] PlayerHealth plHealth;
    [SerializeField] BombLaunch plBomb;
    [SerializeField] PlayerCamera plCamera;
    private WeaponReload reload;
    private WeaponRecoil recoil;
    [SerializeField] ParticleSystem flash;
    private float currentTime;

    private void Awake()
    {
        reload = GetComponent<WeaponReload>();
        recoil = GetComponent<WeaponRecoil>();
        currentTime = Time.time;
    }

    private void Update()
    {
        if (!plHealth.isAlive) return;
        if (isShoot && Fire())
        {
            ShootPlayer();
        }
    }

    public void ShootPlayer()
    {
        if (plHealth && plHealth.healthStop) plHealth.Cancel();
        reload.currentAmmo--;
        if (reload.currentAmmo == 0) reload.CanReload();
        if(plMove.weapon1.activeSelf) plBomb.UpdateFireAmmo(reload.currentAmmo, reload.extraAmmo);
        else plBomb.UpdateAmmo(reload.currentAmmo, reload.extraAmmo);

        barrelPos.LookAt(plCamera.aimPos);
        audioSource.PlayOneShot(gun[0]);
        recoil.TriggerRecoil();
        TriggerFlash();

        fireRateTimer = 0;
        for (int i = 0; i < bulletsPerShot; i++)
        {
            GameObject currentBullet = Instantiate(bullet, barrelPos.position, barrelPos.rotation);
            Bullet bulletScript = currentBullet.GetComponent<Bullet>();
            if (bulletScript) bulletScript.weapon = this;
            else
            {
                AssaultBullet assaultBullet = currentBullet.GetComponent<AssaultBullet>();
                assaultBullet.weapon = this;
            }
            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            rb.AddForce(barrelPos.forward * bulletVelocity, ForceMode.Impulse);
        }
    }

    private bool Fire()
    {
        fireRateTimer += Time.deltaTime;
        if (fireRateTimer < fireRate) return false;
        if (reload.currentAmmo == 0 || reload.isReloading)
        {
            if (!reload.isReloading && Time.time - currentTime > 0.3f)
            {
                audioSource.PlayOneShot(gun[1]);
                currentTime = Time.time;
            }
            return false;
        }
        if (plMove && plMove.currentWeapon != this.gameObject) return false;
        return true;
    }

    public void FireButtonOn()
    {
        isShoot = true;
    }

    public void FireButtonOff()
    {
        isShoot = false;
    }

    private void TriggerFlash()
    {
        flash.Play();
    }
}
