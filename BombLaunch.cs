using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BombLaunch : MonoBehaviour
{
    [SerializeField] GameObject bomb;
    [SerializeField] Transform bombPos;
    private List<Bomb> bombPrefab;

    [SerializeField] GameObject cancelImage;
    [SerializeField] GameObject bombImage;
    private int bombCount;
    private PlayerMove playerMove;
    private PlayerHealth playerHealth;
    [SerializeField] AudioSource bombSource;
    [SerializeField] AudioClip[] audioClip;

    [SerializeField] Text ammoText;
    [SerializeField] Text ammoTextFire;
    [SerializeField] Text bombText;
    public Text countDown;
    [SerializeField] GameObject itemPick;
    [SerializeField] Text itemPickBullet;

    public bool bombTaken;
    WeaponReload reload;
    private bool bombPick;
    private bool healthPick;
    private BulletPick bulletPick;
    public GameObject bulletPickImage;
    public GameObject bombPickImage;
    public GameObject healthPickImage;
    private GameObject bombItem;
    private GameObject healthItem;
    public bool bombIns;

    private void Awake()
    {
        bombPrefab = new List<Bomb>();
        bombCount = 5;
        bombTaken = false;
        bombPick = false;
        bombIns = false;
        playerMove = GetComponent<PlayerMove>();
        playerHealth = GetComponent<PlayerHealth>();
        itemPick.SetActive(false);
        ManageButton(false, true);
        ImageActive(false, false, false);
    }

    public void InstantiateBomb()
    {
        bombIns = true;
        ManageButton(true, false);
        Vector3 spawnPosition = bombPos.position;
        Quaternion spawnRotation = Quaternion.identity;
        GameObject bombInit = Instantiate(bomb, spawnPosition, spawnRotation);

        Rigidbody grenadeRb = bombInit.GetComponent<Rigidbody>();
        if (grenadeRb) grenadeRb.isKinematic = true;

        bombPrefab.Add(bombInit.GetComponent<Bomb>());
        bombPrefab[bombPrefab.Count - 1].handPos = bombPos;

        StartCoroutine(StartTime());
    }

    public void LaunchGrenade()
    {
        if (bombCount <= 0 || !playerHealth.isAlive || playerMove.switching) return;
        if (cancelImage.activeSelf) CancelThrow();
        else if (!bombTaken)
        {
            bombTaken = true;
            bombSource.PlayOneShot(audioClip[0]);
            playerMove.WeaponActive();
        }
    }

    public void ThrowGrenade()
    {
        if (bombCount <= 0 || bombPrefab.Count < 1 || !bombTaken) return;
        bombPrefab[bombPrefab.Count - 1].ThrowBomb();
        MaintainBomb();
    }

    IEnumerator StartTime()
    {
        float tm = 6f;
        int latestBombIndex = bombPrefab.Count - 1;
        Bomb latestBomb = bombPrefab[latestBombIndex];
        countDown.text = "";
        while (tm > 0 && bombPrefab.Contains(latestBomb))
        {
            if (bombPrefab.IndexOf(latestBomb) == bombPrefab.Count - 1) countDown.text = tm.ToString();
            yield return new WaitForSeconds(1f);
            tm -= 1f;
        }

        if (bombPrefab.Contains(latestBomb))
        {
            if (!latestBomb.isThrown) MaintainBomb();
            bombPrefab[0].StartDamage();
            bombPrefab.RemoveAt(0);
            bombSource.PlayOneShot(audioClip[1]);
        }
    }

    private void MaintainBomb()
    {
        ManageButton(false, true);
        bombCount--;
        bombText.text = bombCount.ToString();
        bombIns = false;
        if (bombCount <= 0)
        {
            playerMove.WeaponActive();
            bombTaken = false;
        }
    }

    public void CancelThrow()
    {
        bombIns = false;
        bombPrefab[bombPrefab.Count - 1].CancelGrenade();
        bombPrefab.RemoveAt(bombPrefab.Count - 1);
        ManageButton(false, true);
    }

    private void ManageButton(bool cancel, bool launch)
    {
        cancelImage.SetActive(cancel);
        bombImage.SetActive(launch);
    }

    public void UpdateAmmo(int currentAmmo, int clipAmmo)
    {
        ammoTextFire.text = currentAmmo.ToString() + " | " + clipAmmo.ToString();
    }
    public void UpdateFireAmmo(int currentAmmo, int clipAmmo)
    {
        ammoText.text = currentAmmo.ToString() + " | " + clipAmmo.ToString();
    }

    public void MagIn() => bombSource.PlayOneShot(audioClip[2]);
    public void MagOut() => bombSource.PlayOneShot(audioClip[3]);
    public void ReleaseSlide() => bombSource.PlayOneShot(audioClip[4]);

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("FireBullet"))
        {
            ImageActive(true, false, false);
            reload = playerMove.weapon1.GetComponent<WeaponReload>();
            bulletPick = other.GetComponent<BulletPick>();
            itemPick.SetActive(true);
            itemPickBullet.text = bulletPick.bulletCount.ToString();
        }
        else if (other.CompareTag("GunBullet"))
        {
            ImageActive(true, false, false);
            reload = playerMove.weapon2.GetComponent<WeaponReload>();
            bulletPick = other.GetComponent<BulletPick>();
            itemPick.SetActive(true);
            itemPickBullet.text = bulletPick.bulletCount.ToString();
        }
        else if (other.CompareTag("Grenade"))
        {
            bombItem = other.gameObject;
            ImageActive(false, true, false);
            bombPick = true;
            itemPick.SetActive(true);
            itemPickBullet.text = "1";
        }
        else if (other.CompareTag("Health"))
        {
            healthItem = other.gameObject;
            ImageActive(false, false, true);
            healthPick = true;
            itemPick.SetActive(true);
            itemPickBullet.text = "1";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DestroyItem(false);
    }

    private void DestroyItem(bool flag)
    {
        if (flag)
        {
            if(bulletPick) Destroy(bulletPick.gameObject);
            else if(bombItem) Destroy(bombItem.gameObject);
            else if(healthItem) Destroy(healthItem.gameObject);
        }
        reload = null;
        bombPick = false;
        bulletPick = null;
        bombItem = null;
        healthPick = false;
        healthItem = null;
        itemPick.SetActive(false);
    }

    public void PickUp()
    {
        if (bombPick && bombCount < 5)
        {
            bombCount++;
            bombText.text = bombCount.ToString();
            DestroyItem(true);
        }
        else if(healthPick && playerHealth.totalHealth < 5)
        {
            playerHealth.HealthItem();
            DestroyItem(true);
        }
        else if (!bulletPick || !reload) return;
        else if (reload.gameObject.name == "AssaultGun") PickAssault();
        else if (reload.gameObject.name == "FireGun") PickFire();
    }

    private void PickAssault()
    {
        reload.extraAmmo += bulletPick.bulletCount;
        if (reload.extraAmmo > 300)
        {
            bulletPick.bulletCount = reload.extraAmmo - 300;
            itemPickBullet.text = bulletPick.bulletCount.ToString();
            reload.extraAmmo = 300;
            UpdateAmmo(reload.currentAmmo, reload.extraAmmo);
        }
        else
        {
            UpdateAmmo(reload.currentAmmo, reload.extraAmmo);
            DestroyItem(true);
        }
        playerMove.ChangeFunction();
    }

    private void PickFire()
    {
        reload.extraAmmo += bulletPick.bulletCount;
        if (reload.extraAmmo > 150)
        {
            bulletPick.bulletCount = reload.extraAmmo - 150;
            itemPickBullet.text = bulletPick.bulletCount.ToString();
            reload.extraAmmo = 150;
            UpdateFireAmmo(reload.currentAmmo, reload.extraAmmo);
        }
        else
        {
            UpdateFireAmmo(reload.currentAmmo, reload.extraAmmo);
            DestroyItem(true);
        }
        playerMove.ChangeFunction();
    }

    private void ImageActive(bool flag1, bool flag2, bool flag3)
    {
        bulletPickImage.SetActive(flag1);
        bombPickImage.SetActive(flag2);
        healthPickImage.SetActive(flag3);
    }
}
