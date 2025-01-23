using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZombieMark : MonoBehaviour
{
    private Transform MinimapCam;
    private GameObject playerCheck;
    public float MinimapSize;
    public Transform parent;
    private ZombieAudio zombieAudio;
    public Zombie zombie;

    private Vector3 originalLocalPosition;
    private SpriteRenderer spriteRenderer;
    float distance;

    void Start()
    {
        zombieAudio = GetComponentInParent<ZombieAudio>();
        originalLocalPosition = transform.localPosition;
        spriteRenderer = GetComponent<SpriteRenderer>();
        MinimapCam = zombieAudio.MinimapCam;
        playerCheck = zombieAudio.mainPlayer;
    }

    void Update()
    {
        if (playerCheck != null && MinimapCam != null)
        {
            distance = Vector3.Distance(playerCheck.transform.position, parent.position);
            if (zombie && !zombie.isAlive) HideIcon();
            else if (zombieAudio && distance < zombieAudio.distance) ShowIcon();
            else HideIcon();
        }
    }

    private void ShowIcon()
    {
        spriteRenderer.enabled = true;
        Vector3 worldPosition = parent.position + originalLocalPosition;
        worldPosition.y = transform.position.y;

        worldPosition = new Vector3(
            Mathf.Clamp(worldPosition.x, MinimapCam.position.x - MinimapSize, MinimapCam.position.x + MinimapSize),
            worldPosition.y,
            Mathf.Clamp(worldPosition.z, MinimapCam.position.z - MinimapSize, MinimapCam.position.z + MinimapSize)
        );
        transform.position = worldPosition;
        transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }

    private void HideIcon()
    {
        spriteRenderer.enabled = false;
    }
}