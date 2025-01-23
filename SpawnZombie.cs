using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class SpawnZombie : MonoBehaviour
{
    public Transform spawnLocation;
    public GameObject zombie1Prefab;
    public GameObject zombie2Prefab;
    public GameObject zombie3Prefab;
    public GameObject zombie4Prefab;
    public GameObject wolf;
    public GameObject bear;
    public GameObject victoryScreen;

    [SerializeField] MainMenu mainMenu;
    private int totalZombie;
    List<int> availableIndices;
    public List<GameObject> people;
    public List<Transform> lookPoint;
    public List<Transform> headPos;
    public PlayerHealth health;
    public bool playerWon;
    public Text total;
    public Text killed;
    int totalKilled;

    public Image[] star;

    private void Awake()
    {
        totalKilled = 0;
        killed.text = "0";
        playerWon = false;
        availableIndices = new List<int>();
        people = new List<GameObject>();
        lookPoint = new List<Transform>();
        headPos = new List<Transform>();
        Store();
        TotalZombie();
        Wolf();
        Zombie1();
        Zombie2();
        Zombie3();
        Zombie4();
        victoryScreen.SetActive(false);
    }

    private void Store()
    {
        int childCount = spawnLocation.childCount;
        for (int i = 0; i < childCount; i++)
        {
            availableIndices.Add(i);
        }
    }

    private void Zombie1()
    {
        int count = totalZombie / 4;
        int index = 0;
        int getIndex = 0;
        Transform randomSpawn = null;

        for (int i = 0; i < count; i++)
        {
            if (availableIndices.Count == 0) break;

            getIndex = Random.Range(0, availableIndices.Count);
            index = availableIndices[getIndex];

            randomSpawn = spawnLocation.GetChild(index);
            Instantiate(zombie1Prefab, randomSpawn.position, randomSpawn.rotation, this.transform);
            availableIndices.RemoveAt(getIndex);
        }
    }

    private void Zombie2()
    {
        int count = totalZombie / 4;
        int index = 0;
        int getIndex = 0;
        Transform randomSpawn = null;

        for (int i = 0; i < count; i++)
        {
            if (availableIndices.Count == 0) break;

            getIndex = Random.Range(0, availableIndices.Count);
            index = availableIndices[getIndex];

            randomSpawn = spawnLocation.GetChild(index);
            Instantiate(zombie2Prefab, randomSpawn.position, randomSpawn.rotation, this.transform);
            availableIndices.RemoveAt(getIndex);
        }
    }

    private void Zombie3()
    {
        int count = totalZombie / 4;
        int index = 0;
        int getIndex = 0;
        Transform randomSpawn = null;

        for (int i = 0; i < count; i++)
        {
            if (availableIndices.Count == 0) break;

            getIndex = Random.Range(0, availableIndices.Count);
            index = availableIndices[getIndex];

            randomSpawn = spawnLocation.GetChild(index);
            Instantiate(zombie3Prefab, randomSpawn.position, randomSpawn.rotation, this.transform);
        }
    }

    private void Zombie4()
    {
        int count = totalZombie / 4;
        int index = 0;
        int getIndex = 0;
        Transform randomSpawn = null;

        for (int i = 0; i < count; i++)
        {
            if (availableIndices.Count == 0) break;

            getIndex = Random.Range(0, availableIndices.Count);
            index = availableIndices[getIndex];

            randomSpawn = spawnLocation.GetChild(index);
            Instantiate(zombie4Prefab, randomSpawn.position, randomSpawn.rotation, this.transform);
        }
    }

    private void Wolf()
    {
        int index = 0;
        int getIndex = 0;
        Transform randomSpawn = null;
        int count = 20;
        for (int i = 0; i < count; i++)
        {
            if (availableIndices.Count == 0) break;

            getIndex = Random.Range(0, availableIndices.Count);
            index = availableIndices[getIndex];

            randomSpawn = spawnLocation.GetChild(index);
            if (randomSpawn)
            {
                if(i < 10) Instantiate(wolf, randomSpawn.position, randomSpawn.rotation, this.transform);
                else Instantiate(bear, randomSpawn.position, randomSpawn.rotation, this.transform);
            }
            availableIndices.RemoveAt(getIndex);
        }
    }

    public void ZombieKilled()
    {
        totalKilled += 1;
        killed.text = totalKilled.ToString();
        totalZombie -= 1;
        total.text = totalZombie.ToString();
        if (totalZombie <= 0 && CheckPlayer())
        {
            health.StopFire();
            playerWon = true;
            mainMenu.HideItem();
            victoryScreen.SetActive(true);
            CalculateStar();
            int level = PlayerPrefs.GetInt("Level") + 1;
            PlayerPrefs.SetInt("Level", level);
        }
    }

    private void CalculateStar()
    {
        int rem = people.Count;
        float normalizedValue;
        float val;

        val = rem > 7 ? 7 : rem;
        normalizedValue = Mathf.Clamp(val, 0, 7f);
        star[0].fillAmount = (normalizedValue / 7f);
        rem -= 7;

        val = rem > 7 ? 7 : rem;
        normalizedValue = Mathf.Clamp(val, 0, 7f);
        star[1].fillAmount = (normalizedValue / 7f);
        rem -= 7;

        val = rem > 7 ? 7 : rem;
        normalizedValue = Mathf.Clamp(val, 0, 7f);
        star[2].fillAmount = (normalizedValue / 7f);
    }

    private bool CheckPlayer()
    {
        for(int i = 0; i < people.Count; i++)
        {
            if (people[i].tag == "Player") return true;
        }
        return false;
    }

    private void TotalZombie()
    {
        if (PlayerPrefs.HasKey("Level"))
        {
            int level = PlayerPrefs.GetInt("Level");
            totalZombie = 20 + (level * 4);
            if (totalZombie > 60) totalZombie = 60;
        }
        else
        {
            totalZombie = 24;
            PlayerPrefs.SetInt("Level", 1);
        }

        total.text = totalZombie.ToString();
    }

    public void SetAudio(AudioSource one, AudioSource two, AudioSource three)
    {
        mainMenu.AddAudio(one, two, three);
    }

    public void StorePeople(GameObject pl)
    {
        people.Add(pl);
        Transform head = pl.transform.Find("HeadPos");
        headPos.Add(head);
        Transform look = pl.transform.Find("LookPoint");
        lookPoint.Add(look);
    }

    public void RemovePeople(GameObject pl)
    {
        int index = people.FindIndex(p => p == pl);
        if(index != -1)
        {
            people.RemoveAt(index);
            headPos.RemoveAt(index);
            lookPoint.RemoveAt(index);
        }
    }
}
