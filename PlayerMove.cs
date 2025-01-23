using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    public float currentMoveSpeed;
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float airSpeed = 1f;
    [HideInInspector] public Vector3 dir;
    [HideInInspector] public float hzInput, vInput;
    public CharacterController controller;
    public Animator animator;

    public GameObject weapon1;
    public GameObject weapon2;
    public GameObject currentWeapon;
    public GameObject weaponItems;

    [SerializeField] float groundYOffset;
    [SerializeField] LayerMask groundMask;
    Vector3 spherePos;
    public float rotationSpeed = 100f;
    [SerializeField] float gravity = -9.81f;
    Vector3 velocity;

    public bool playerRun;
    public bool mobileInput;
    public bool hasPunch;

    public int Alert = 30;
    private PlayerCamera cameraPlayer;
    private PlayerHealth playerHealth;

    private Weapon weapon;
    private WeaponReload reload;
    private BombLaunch bomb;
    public GameObject bombImage;
    public GameObject shootImage;
    public GameObject shootButton;
    public FixedJoystick joystick;

    [SerializeField] float jumpForce = 1f;
    [HideInInspector] public bool jumped;
    public bool switching;
    public GameObject lockImage;
    public GameObject active1;
    public GameObject active2;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cameraPlayer = GetComponent<PlayerCamera>();
        currentMoveSpeed = walkSpeed;
        playerHealth = GetComponent<PlayerHealth>();
        bomb = GetComponent<BombLaunch>();
        weapon2.SetActive(false);
        weapon1.SetActive(true);
        currentWeapon = weapon1;
        ChangeFunction();
    }

    void Update()
    {
        if (!playerHealth.isAlive) return;
        Gravity();
        GetDirectionAndMove();
        SetInput();
        WalkAnimations();
    }

    void GetDirectionAndMove()
    {
        if (cameraPlayer.mobileInput)
        {
            hzInput = joystick.Horizontal;
            vInput = joystick.Vertical;
        }
        else
        {
            hzInput = Input.GetAxis("Horizontal");
            vInput = Input.GetAxis("Vertical");
        }

        RunAuto();

        Vector3 airDir = Vector3.zero;
        if (!IsGrounded()) airDir = transform.forward * vInput + transform.right * hzInput;
        else dir = transform.forward * vInput + transform.right * hzInput;

        controller.Move((dir.normalized * currentMoveSpeed + airDir.normalized * airSpeed) * Time.deltaTime);
    }

    public bool IsGrounded()
    {
        spherePos = new Vector3(transform.position.x, transform.position.y - groundYOffset, transform.position.z);
        if (Physics.CheckSphere(spherePos, controller.radius - 0.1f, groundMask)) return true;
        return false;
    }

    private void RunAuto()
    {
        if((vInput != 0f && vInput <= 0.9f) || hzInput != 0f)
        {
            if (playerRun) RunFalse();
            if (playerHealth && playerHealth.healthStop) playerHealth.Cancel();
        }
        if(vInput > 0.98f || playerRun)
        {
            vInput = 1f;
            if (!playerRun) RunTrue();
            if (playerHealth && playerHealth.healthStop) playerHealth.Cancel();
        }
    }

    void Gravity()
    {
        if (!IsGrounded()) velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0) velocity.y = -2;

        controller.Move(velocity * Time.deltaTime);
    }

    void WalkAnimations()
    {
        if (playerRun) return;
        bool isWalking = (hzInput != 0 || vInput != 0);
        animator.SetBool("Walking", isWalking);
    }

    public void RunButton()
    {
        if (cameraPlayer.isAim) return;
        if (playerRun) RunFalse();
        else
        {
            currentMoveSpeed = runSpeed;
            playerRun = true;
            animator.SetBool("Crouching", false);
            cameraPlayer.crouch = false;
            animator.SetBool("Running", true);
            if (playerHealth && playerHealth.healthStop) playerHealth.Cancel();
        }
    }

    private void RunTrue()
    {
        if (cameraPlayer.isAim || cameraPlayer.crouch) return;
        lockImage.SetActive(true);
        if (cameraPlayer.crouch) return;
        currentMoveSpeed = runSpeed;
        playerRun = true;
        animator.SetBool("Running", true);
    }

    public void RunFalse()
    {
        lockImage.SetActive(false);
        currentMoveSpeed = walkSpeed;
        playerRun = false;
        animator.SetBool("Running", false);
    }

    private void SetInput()
    {
        animator.SetFloat("hzInput", hzInput);
        animator.SetFloat("vInput", vInput);
    }

    public void SwitchWeapon()
    {
        if (cameraPlayer.isAim || switching || bomb.bombIns || playerHealth.healthStop) return;
        currentWeapon.SetActive(false);
        switching = true;
        shootButton.SetActive(false);
        if (weapon) weapon.FireButtonOff();
        if (reload) reload.isReloading = false;

        bomb.bombTaken = false;

        playerHealth.PlayGun();
        if (cameraPlayer.isAim) cameraPlayer.ScopeZoom();
        if (bombImage.activeSelf) WeaponActive();
        else StartCoroutine(Switching());
    }

    IEnumerator Switching()
    {
        weaponItems.SetActive(false);

        yield return new WaitForSeconds(1f);

        currentWeapon = currentWeapon == weapon1 ? weapon2 : weapon1;
        ChangeFunction();
        weaponItems.SetActive(true);
        switching = false;
        shootButton.SetActive(true);
        currentWeapon.SetActive(true);

        active1.SetActive(!active1.activeSelf);
        active2.SetActive(!active2.activeSelf);
    }

    public void WeaponActive()
    {
        WeaponReload reload = currentWeapon.GetComponent<WeaponReload>();
        if(reload) reload.isReloading = false;
        bool flag = bombImage.activeSelf;
        if(cameraPlayer.isAim) cameraPlayer.ScopeZoom();
        shootImage.SetActive(flag);
        bombImage.SetActive(!flag);
        weaponItems.SetActive(flag);
        currentWeapon.SetActive(flag);
        ChangeFunction();
        switching = false;
        shootButton.SetActive(true);
    }

    public void CrouchUIButton()
    {
        if (playerRun) return;
        if (cameraPlayer.crouch)
        {
            CrouchFalse();
        }
        else
        {
            cameraPlayer.crouch = true;
            animator.SetBool("Crouching", true);
        }
    }

    private void CrouchFalse()
    {
        cameraPlayer.crouch = false;
        animator.SetBool("Crouching", false);
    }

    public void ChangeFunction()
    {
        weapon = currentWeapon.GetComponent<Weapon>();
        reload = currentWeapon.GetComponent<WeaponReload>();
        if (reload && reload.currentAmmo == 0) reload.CanReload();
    }

    public void WeaponOn()
    {
        if (currentWeapon.activeSelf || playerHealth.healthStop)
        {
            currentWeapon.SetActive(true);
            weapon.FireButtonOn();
        }
        else bomb.InstantiateBomb();
    }
    public void WeaponOff()
    {
        if (currentWeapon.activeSelf) weapon.FireButtonOff();
        else bomb.ThrowGrenade();
    }

    public void WeaponReload()
    {
        reload.CanReload();
    }

    public void WeaponReloaded()
    {
        WeaponReload reload1 = currentWeapon.GetComponent<WeaponReload>();
        if (!reload1 || !reload1.isReloading) return;
        reload1.isReloading = false;
        reload1.Reload();
        if (reload1.clipSize == 15) bomb.UpdateFireAmmo(reload1.currentAmmo, reload1.extraAmmo);
        else bomb.UpdateAmmo(reload1.currentAmmo, reload1.extraAmmo);
    }

    public void JumpButton()
    {
        if (cameraPlayer.isAim || jumped) return;
        if(playerHealth && playerHealth.healthStop) playerHealth.Cancel();
        CrouchFalse();
        jumped = true;
        animator.SetTrigger("Jump");
    }

    public void HealthActive(bool flag)
    {
        if(!flag)
        {
            WeaponReload reload = currentWeapon.GetComponent<WeaponReload>();
            if (reload) reload.isReloading = false;
        }
        else
        {
            ChangeFunction();
        }
        currentWeapon.SetActive(flag);
    }

    public bool IsReloading()
    {
        return reload.isReloading;
    }

    public void JumpForce() => velocity.y += jumpForce;
    public void Jumped() => jumped = false;
}