using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] GameObject bombPrefab;
    [SerializeField] GameObject bombChild;

    [SerializeField] private float baseThrowForce = 5f;
    [SerializeField] private float throwUpwardForce = 5f;
    [SerializeField] private float bombTimer = 5f;
    [SerializeField] LineRenderer lineRenderer;

    private Rigidbody rb;
    private Camera playerCamera;
    public Transform handPos;
    public bool isThrown;

    private void Awake()
    {
        isThrown = false;
        rb = GetComponent<Rigidbody>();
        playerCamera = Camera.main;
    }

    private void Update()
    {
        if (!isThrown)
        {
            transform.position = handPos.position;
            DrawTrajectory();
        }
    }

    public void ThrowBomb()
    {
        isThrown = true;
        lineRenderer.positionCount = 0;

        if (playerCamera == null) return;

        rb.isKinematic = false;

        Vector3 cameraForward = playerCamera.transform.forward;
        float cameraPitch = playerCamera.transform.eulerAngles.x;
        float pitchMultiplier;

        if (cameraPitch <= 90f)
        {
            pitchMultiplier = Mathf.Lerp(1.5f, 2.5f, Mathf.InverseLerp(90f, 0f, cameraPitch));
        }
        else
        {
            pitchMultiplier = Mathf.Lerp(0.5f, 1.5f, Mathf.InverseLerp(90f, 180f, cameraPitch));
        }
        float adjustedThrowForce = baseThrowForce * pitchMultiplier;
        Vector3 throwDirection = cameraForward * adjustedThrowForce + transform.up * throwUpwardForce;
        rb.AddForce(throwDirection, ForceMode.Impulse);
    }

    private void DrawTrajectory()
    {
        if (lineRenderer == null) return;

        Vector3 startPos = handPos.position;
        float cameraPitch = playerCamera.transform.eulerAngles.x;
        cameraPitch = cameraPitch > 180 ? cameraPitch - 360 : cameraPitch;

        float pitchMultiplier = cameraPitch <= 90f ? Mathf.Lerp(1.5f, 2.5f, Mathf.InverseLerp(90f, 0f, cameraPitch))
                                                   : Mathf.Lerp(0.5f, 1.5f, Mathf.InverseLerp(90f, 180f, Mathf.Abs(cameraPitch)));

        Vector3 startVelocity = playerCamera.transform.forward * (baseThrowForce * pitchMultiplier) + transform.up * throwUpwardForce;

        float timeStep = 0.02f;
        float totalTime = bombTimer;
        float gravity = Physics.gravity.y;
        int numPoints = Mathf.CeilToInt(totalTime / timeStep);
        lineRenderer.positionCount = numPoints;

        for (int i = 0; i < numPoints; i++)
        {
            float time = i * timeStep;
            Vector3 position = startPos + startVelocity * time + 0.5f * Vector3.up * gravity * time * time;
            lineRenderer.SetPosition(i, position);
        }
    }

    public void StartDamage()
    {
        GameObject mainBomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player") || hitCollider.CompareTag("Zombie") || hitCollider.CompareTag("People"))
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance <= 4f)
                {
                    ApplyDamage(hitCollider.gameObject, 120f);
                }
                else if (distance <= 8f)
                {
                    ApplyDamage(hitCollider.gameObject, 60f);
                }
            }
        }
        Destroy(mainBomb, 2f);
        Destroy(this.gameObject);
    }

    private void ApplyDamage(GameObject target, float damage)
    {
        if (!HasLineOfSight(target.transform)) return;

        Collider targetCollider = target.GetComponent<Collider>();
        Vector3 hitPosition = targetCollider.bounds.center;

        GameObject hitEffect = Instantiate(bombChild, hitPosition, Quaternion.identity);
        Transform hitPart = targetCollider.transform;
        hitEffect.transform.SetParent(hitPart);

        Destroy(hitEffect, 5f);
        PlayerHealth player = target.GetComponentInParent<PlayerHealth>();
        if (player && player.isAlive)
        {
            player.TakeDamage(damage);
            return;
        }
        Zombie zombie = target.GetComponentInParent<Zombie>();
        if (zombie && zombie.isAlive)
        {
            zombie.TakeDamage(damage);
            return;
        }
        People people = target.GetComponentInParent<People>();
        if (people && people.isAlive)
        {
            people.TakeDamage(damage);
        }
    }

    private bool HasLineOfSight(Transform target)
    {
        LayerMask ignoreLayers = LayerMask.GetMask("Zombie", "People", "Player");
        LayerMask obstacleMask = ~ignoreLayers;

        Vector3 direction = (target.GetComponent<Collider>().bounds.center - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distanceToTarget, obstacleMask))
        {
            int hitLayer = hit.transform.gameObject.layer;
            if ((ignoreLayers & (1 << hitLayer)) == 0)
            {
                return false;
            }
        }
        return true;
    }

    public void CancelGrenade()
    {
        Destroy(this.gameObject);
    }
}
