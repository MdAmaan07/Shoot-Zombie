using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponReload : MonoBehaviour
{
    public int clipSize;
    public int extraAmmo;
    public int currentAmmo;
    [SerializeField] Animator animator;
    [SerializeField] PlayerCamera playerCamera;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] BombLaunch bomb;
    public bool isReloading;

    private void Awake()
    {
        currentAmmo = clipSize;
    }

    public void Reload()
    {
        if (extraAmmo >= clipSize)
        {
            int ammoToReload = clipSize - currentAmmo;
            extraAmmo -= ammoToReload;
            currentAmmo += ammoToReload;
        }
        else if (extraAmmo > 0)
        {
            if (extraAmmo + currentAmmo > clipSize)
            {
                int leftOverAmmo = extraAmmo + currentAmmo - clipSize;
                extraAmmo = leftOverAmmo;
                currentAmmo = clipSize;
            }
            else
            {
                currentAmmo += extraAmmo;
                extraAmmo = 0;
            }
        }
    }

    public void CanReload()
    {
        if (extraAmmo == 0 || currentAmmo == clipSize || isReloading || (playerHealth && playerHealth.healthStop)) return;
        if (playerCamera.isAim) playerCamera.ScopeZoom();
        isReloading = true;
        animator.SetTrigger("Reload");
    }
}
