using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAudio : MonoBehaviour
{
    public AudioClip[] soundZombie;
    public bool chaseStarted;
    public GameObject mainPlayer;
    public Transform MinimapCam;
    public float distance;

    private void Awake()
    {
        chaseStarted = false;
        distance = 25f;
    }
}
