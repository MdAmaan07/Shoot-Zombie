using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleFind : MonoBehaviour
{
    private Transform MinimapCam;
    private GameObject playerCheck;
    public float MinimapSize;
    public Transform parent;
    private SpawnItem spawn;
    public People people;

    private Vector3 originalLocalPosition;
    private SpriteRenderer spriteRenderer;
    float distance;

    void Start()
    {
        spawn = GetComponentInParent<SpawnItem>();
        originalLocalPosition = transform.localPosition;
        spriteRenderer = GetComponent<SpriteRenderer>();
        MinimapCam = spawn.MinimapCam;
        playerCheck = spawn.mainPlayer;
    }

    void Update()
    {
        if (playerCheck != null && MinimapCam != null)
        {
            distance = Vector3.Distance(playerCheck.transform.position, parent.position);
            if (people && !people.isAlive) HideIcon();
            else if (distance < 30f) ShowIcon();
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
