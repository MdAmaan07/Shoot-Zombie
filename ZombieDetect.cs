using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.XR;

public class ZombieDetect : MonoBehaviour
{
    private float lookDistance;
    private float fov;
    public Transform enemyEyes;
    Zombie zombie;
    private float currentTime;

    private void Awake()
    {
        zombie = GetComponent<Zombie>();
        currentTime = Time.time;
        lookDistance = 25f;
        fov = 270f;
        if(zombie) zombie.UpdateDistance(lookDistance);
    }

    public bool PlayerSeen()
    {
        if (zombie == null || zombie.headPos == null || enemyEyes == null || zombie.player == null) return false;
        IncreaseDistance();
        if ((this.gameObject.name.StartsWith("Wolf") || this.gameObject.name.StartsWith("Bear")) && !zombie.getHit) return false;
        if (Vector3.Distance(enemyEyes.position, zombie.headPos.position) < 10f) return true;
        if (Vector3.Distance(enemyEyes.position, zombie.headPos.position) > lookDistance) return false;

        Vector3 dirToPlayer = (zombie.headPos.position - enemyEyes.position).normalized;

        float angleToPlayer = Vector3.Angle(enemyEyes.parent.forward, dirToPlayer);

        if (angleToPlayer > (fov / 2)) return false;

        enemyEyes.LookAt(zombie.headPos.position);

        RaycastHit hit;
        if (Physics.Raycast(enemyEyes.position, enemyEyes.forward, out hit, lookDistance))
        {
            if (hit.transform == null) return false;
            if (hit.transform.name == zombie.headPos.root.name)
            {
                return true;
            }
        }
        return false;
    }

    private void IncreaseDistance()
    {
        if (Time.time - currentTime > 600f)
        {
            lookDistance = Mathf.Infinity;
            if (zombie) zombie.UpdateDistance(lookDistance);
        }
        else if (lookDistance < 60f && Time.time - currentTime > 500f)
        {
            lookDistance = 60f;
            if (zombie) zombie.UpdateDistance(lookDistance);
        }
        else if (lookDistance < 45f && Time.time - currentTime > 300f)
        {
            lookDistance = 45f;
            if (zombie) zombie.UpdateDistance(lookDistance);
        }
        else if (lookDistance < 30f && Time.time - currentTime > 200f)
        {
            lookDistance = 30f;
            if (zombie) zombie.UpdateDistance(lookDistance);
        }
    }
}
