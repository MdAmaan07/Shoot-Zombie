using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    public Transform spawnLocation;
    public GameObject fireBullet;
    public GameObject gunBullet;
    public GameObject health;
    public GameObject bomb;
    public GameObject people1;
    public GameObject people2;
    public GameObject people3;
    private int totalItems;
    List<int> availableIndices;
    public bool scream;
    public GameObject player;
    public Transform look;
    public SpawnZombie spawnZb;
    public AudioClip help;
    public GameObject mainPlayer;
    public Transform MinimapCam;

    private void Awake()
    {
        scream = false;
        availableIndices = new List<int>();
        TotalItems();
        Store();
        FireBullet();
        GunBullet();
        Health();
        Bomb();
        People();
    }

    private void Store()
    {
        int childCount = spawnLocation.childCount;
        for (int i = 0; i < childCount; i++)
        {
            availableIndices.Add(i);
        }
    }

    private void FireBullet()
    {
        int count = totalItems / 4;
        int index = 0;
        int getIndex = 0;
        Transform randomSpawn = null;

        for (int i = 0; i < count; i++)
        {
            if (availableIndices.Count == 0) break;

            getIndex = Random.Range(0, availableIndices.Count);
            index = availableIndices[getIndex];
            randomSpawn = spawnLocation.GetChild(index);
            Vector3 spawnPosition = new Vector3(randomSpawn.position.x, randomSpawn.position.y + 0.25f, randomSpawn.position.z);

            Instantiate(fireBullet, spawnPosition, randomSpawn.rotation, this.transform);
            availableIndices.RemoveAt(getIndex);
        }
    }

    private void GunBullet()
    {
        int count = totalItems / 4;
        int index = 0;
        int getIndex = 0;
        Transform randomSpawn = null;

        for (int i = 0; i < count; i++)
        {
            if (availableIndices.Count == 0) break;

            getIndex = Random.Range(0, availableIndices.Count);
            index = availableIndices[getIndex];

            randomSpawn = spawnLocation.GetChild(index);
            Vector3 spawnPosition = new Vector3(randomSpawn.position.x, randomSpawn.position.y + 0.25f, randomSpawn.position.z);

            Instantiate(gunBullet, spawnPosition, randomSpawn.rotation, this.transform);
            availableIndices.RemoveAt(getIndex);
        }
    }

    private void Health()
    {
        int count = totalItems / 4;
        int index = 0;
        int getIndex = 0;
        Transform randomSpawn = null;

        for (int i = 0; i < count; i++)
        {
            if (availableIndices.Count == 0) break;

            getIndex = Random.Range(0, availableIndices.Count);
            index = availableIndices[getIndex];

            randomSpawn = spawnLocation.GetChild(index);
            Vector3 spawnPosition = new Vector3(randomSpawn.position.x, randomSpawn.position.y + 0.25f, randomSpawn.position.z);

            Instantiate(health, spawnPosition, randomSpawn.rotation, this.transform);
            availableIndices.RemoveAt(getIndex);
        }
    }

    private void Bomb()
    {
        int count = totalItems / 6;
        int index = 0;
        int getIndex = 0;
        Transform randomSpawn = null;

        for (int i = 0; i < totalItems; i++)
        {
            if (availableIndices.Count == 0) break;

            getIndex = Random.Range(0, availableIndices.Count);
            index = availableIndices[getIndex];

            randomSpawn = spawnLocation.GetChild(index);
            Vector3 spawnPosition = new Vector3(randomSpawn.position.x, randomSpawn.position.y + 0.25f, randomSpawn.position.z);

            Instantiate(bomb, spawnPosition, randomSpawn.rotation, this.transform);
            availableIndices.RemoveAt(getIndex);
        }
    }

    private void TotalItems()
    {
        if (PlayerPrefs.HasKey("Level"))
        {
            int random = Random.Range(4, 15);
            int level = PlayerPrefs.GetInt("Level");
            totalItems = 80 + (level * random);
            if (totalItems > 170) totalItems = 170;
        }
        else
        {
            totalItems = 80;
            PlayerPrefs.SetInt("Level", 1);
        }
    }

    private void People()
    {
        int count = 20;
        int index = 0;
        int getIndex = 0;
        Transform randomSpawn = null;
        GameObject people;

        for (int i = 0; i < count; i++)
        {
            if (availableIndices.Count == 0) break;

            getIndex = Random.Range(0, availableIndices.Count);
            index = availableIndices[getIndex];

            randomSpawn = spawnLocation.GetChild(index);
            Vector3 spawnPosition = new Vector3(randomSpawn.position.x, randomSpawn.position.y + 0.25f, randomSpawn.position.z);

            if(i < 5) people = Instantiate(people1, spawnPosition, randomSpawn.rotation, this.transform);
            else if (i < 10) people = Instantiate(people2, spawnPosition, randomSpawn.rotation, this.transform);
            else people = Instantiate(people3, spawnPosition, randomSpawn.rotation, this.transform);
            spawnZb.StorePeople(people);

            availableIndices.RemoveAt(getIndex);
        }
    }

    public void RemoveDied(GameObject pl)
    {
        spawnZb.RemovePeople(pl);
    }
}
