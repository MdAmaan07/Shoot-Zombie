using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Menu[] menus;

    [SerializeField] AudioSource[] audioSources;
    [SerializeField] AudioClip[] audioClip;
    [SerializeField] List<AudioSource> gameSources = new List<AudioSource>();
    [SerializeField] Slider[] sliders;
    [SerializeField] ZombieAudio zombie;
    private bool start;
    string sceneName;
    public Image[] buttonImage;
    public GameObject menuOption;
    public GameObject loadingScreen;
    public Text introInfo;
    public GameObject crossHair;
    public PlayerCamera plCamera;
    public Light sceneLight;
    public Text messageInfo;
    public SpawnItem item;
    public GameObject intro;
    public Text countingText;

    private void Awake()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        sceneName = activeScene.name;
        SetAudioScene();
        SetMatchScene();
        if (sceneName == "MainMenu")
        {
            menuOption.SetActive(true);
            loadingScreen.SetActive(false);
            BackGroundOn();
        }
        else if (sceneName == "Game")
        {
            ShowIntro();
            StartCoroutine(ShowHelp());
            crossHair.SetActive(false);
        }
        start = true;
    }

    public void OpenMenu(string menuName)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].name == menuName)
            {
                menus[i].Open();
            }
            else if (menus[i].isOpen)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    public void OpenMenu(Menu menu)
    {
        ButtonMusic();
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].isOpen)
            {
                CloseMenu(menus[i]);
            }
        }
        menu.Open();
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }

    public void CloseSetting(Menu menu)
    {
        if (menu.menuName == "Intro") crossHair.SetActive(true);
        ButtonMusic();
        menu.Close();
    }

    public void GameScene()
    {
        ButtonMusic();
        if (sceneName == "MainMenu")
        {
            menuOption.SetActive(false);
            loadingScreen.SetActive(true);
        }
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Game");
    }

    public void ExitGame()
    {
        ButtonMusic();
        Application.Quit();
    }

    public void Lobby()
    {
        ButtonMusic();
        StartCoroutine(GoLobby());
    }

    IEnumerator GoLobby()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("MainMenu");
    }

    public void Volume()
    {
        SliderMusic();
        float vol = sliders[0].value;
        if (sceneName == "Game") StartCoroutine(GameSound(vol));
        else
        {
            audioSources[0].volume = vol;
            audioSources[1].volume = vol;
        }
        PlayerPrefs.SetFloat("Volume", vol);
    }

    public void Music()
    {
        SliderMusic();
        audioSources[0].volume = sliders[1].value;
        PlayerPrefs.SetFloat("Music", sliders[1].value);
    }

    public void SoundSfx()
    {
        SliderMusic();
        audioSources[1].volume = sliders[2].value;
        PlayerPrefs.SetFloat("Sfx", sliders[2].value);
    }

    public void Difficulty()
    {
        SliderMusic();
        float val = sliders[3].value;
        PlayerPrefs.SetFloat("Difficulty", val);
    }

    public void Transparency()
    {
        SliderMusic();
        float val = sliders[4].value;
        if(sceneName == "Game") StartCoroutine(SetButtonView(val));
        PlayerPrefs.SetFloat("Transp", val);
    }

    public void Sensitivity()
    {
        SliderMusic();
        float val = sliders[5].value;
        PlayerPrefs.SetFloat("Sense", val);
        if (sceneName == "Game") plCamera.SetSensitivity(val);
    }

    private void SetMatchScene()
    {
        if (PlayerPrefs.HasKey("Transp")) sliders[4].value = PlayerPrefs.GetFloat("Transp");
        else PlayerPrefs.SetFloat("Transp", sliders[4].value);

        if (PlayerPrefs.HasKey("Sense")) sliders[5].value = PlayerPrefs.GetFloat("Sense");
        else PlayerPrefs.SetFloat("Sense", sliders[5].value);

        if (sceneName == "Game")
        {
            if (PlayerPrefs.HasKey("Light")) sliders[6].value = PlayerPrefs.GetFloat("Light");
            else PlayerPrefs.SetFloat("Light", sliders[6].value);
            sceneLight.intensity = sliders[6].value;

            StartCoroutine(SetButtonView(sliders[4].value));

            if (PlayerPrefs.HasKey("Scope")) sliders[7].value = PlayerPrefs.GetFloat("Scope");
            else PlayerPrefs.SetFloat("Scope", sliders[7].value);
        }
        else
        {
            if (PlayerPrefs.HasKey("Difficulty")) sliders[3].value = PlayerPrefs.GetFloat("Difficulty");
            else PlayerPrefs.SetFloat("Difficulty", sliders[3].value);
        }
    }

    private void SetAudioScene()
    {
        if (PlayerPrefs.HasKey("Volume")) sliders[0].value = PlayerPrefs.GetFloat("Volume");
        else PlayerPrefs.SetFloat("Volume", sliders[0].value);

        if (PlayerPrefs.HasKey("Sfx")) sliders[2].value = PlayerPrefs.GetFloat("Sfx");
        else PlayerPrefs.SetFloat("Sfx", sliders[2].value);

        if (sceneName == "MainMenu")
        {
            if (PlayerPrefs.HasKey("Music")) sliders[1].value = PlayerPrefs.GetFloat("Music");
            else PlayerPrefs.SetFloat("Music", sliders[1].value);
        }
    }

    private void BackGroundOn()
    {
        audioSources[0].clip = audioClip[0];
        audioSources[0].loop = true;
        audioSources[0].Play();
    }

    private void ButtonMusic()
    {
        audioSources[1].PlayOneShot(audioClip[1]);
    }

    private void SliderMusic()
    {
        if (!start) return;
        audioSources[1].PlayOneShot(audioClip[2]);
    }

    IEnumerator SetButtonView(float value)
    {
        yield return new WaitForSeconds(1f);
        Color currentColor;
        for(int i=0; i<=19; i++)
        {
            currentColor = buttonImage[i].color;
            currentColor.a = value;
            buttonImage[i].color = currentColor;
        }
    }

    public void HideItem()
    {
        foreach(Image image in buttonImage)
        {
            image.gameObject.SetActive(false);
        }
    }

    private void ShowIntro()
    {
        string info = GetMessage();
        introInfo.text = info;
    }

    private string GetMessage()
    {
        string[] lines = new string[]
        {
        "THE NIGHT IS DARK, AND THE DEAD ARE HUNGRY!",
        "THEY’RE COMING… FASTER THAN YOU THINK. BE READY!",
        "KEEP YOUR EYES OPEN. DANGER IS EVERYWHERE!",
        "SURVIVE THE NIGHT, OR JOIN THE DEAD!",
        "LOAD YOUR WEAPON. THE ZOMBIES WON'T WAIT!",
        "NO TIME TO HESITATE. FIGHT OR DIE!",
        "THE HORDE IS APPROACHING. STAND YOUR GROUND!",
        "MOVE FAST. SHOOT FASTER!",
        "ONLY THE STRONG WILL SURVIVE THIS NIGHT!",
        "THE DEAD WON’T STOP UNTIL YOU DO!",
        "PREPARE FOR WAR. THE DEAD WON'T GIVE UP EASILY!",
        "STAY SHARP. ONE MISTAKE COULD COST YOU YOUR LIFE!",
        "GATHER YOUR GEAR. THE HUNT BEGINS NOW!",
        "HIDE OR FIGHT. THE CHOICE IS YOURS!",
        "DEATH AWAITS THE UNPREPARED. BE READY!",
        "EVERY SHOT COUNTS. MAKE THEM ALL MATTER!",
        "THE NIGHT IS YOUR ENEMY. SURVIVE IT!",
        "SILENCE IS DEADLY. STAY ALERT!",
        "TRUST NO ONE. EVEN FRIENDS CAN TURN!",
        "THEY FEED ON FEAR. DON’T LET THEM WIN!",
        "AVOID WOLVES AND BEARS, FIGHT ZOMBIES!"
        };

        int randomIndex = Random.Range(0, lines.Length);
        return (lines[randomIndex]);
    }

    IEnumerator GameSound(float val)
    {
        yield return new WaitForSeconds(1f);
        foreach(AudioSource audio in gameSources)
        {
            audio.volume = val;
        }
    }

    public void AddAudio(AudioSource one, AudioSource two, AudioSource three)
    {
        gameSources.Add(one);
        gameSources.Add(two);
        gameSources.Add(three);
    }

    public void IncreaseLight()
    {
        SliderMusic();
        float val = sliders[6].value;
        sceneLight.intensity = val;
        PlayerPrefs.SetFloat("Light", val);
    }

    public void ScopeSense()
    {
        SliderMusic();
        float val = sliders[7].value;
        plCamera.SetScopeSense(val);
        PlayerPrefs.SetFloat("Scope", val);
    }

    private IEnumerator ShowHelp()
    {
        bool display = true;
        int time = 6;
        while (true)
        {
            yield return new WaitForSeconds(1f);
            time--;
            if(intro.activeSelf)
            {
                countingText.text = time.ToString();
                if(time <= 0)
                {
                    time = 5;
                    intro.SetActive(false);
                    zombie.chaseStarted = true;
                    crossHair.SetActive(true);
                }
            }
            else if (time <= 0)
            {
                if (!display) messageInfo.text = Message();
                else messageInfo.text = "";
                display = !display;
                item.scream = display;
                time = 5;
            }
        }
    }

    private string Message()
    {
        string[] lines = new string[]
        {
            "AVOID WOLVES AND BEARS, FIGHT ZOMBIES!",
            "STAY CLEAR OF WOLVES!",
            "HELP PEOPLE, KILL ZOMBIES!",
            "THEY ARE COMING FOR YOU!",
            "PROTECT SURVIVORS!",
            "FIGHT ZOMBIES NOW!",
            "RESCUE CIVILIANS!",
            "ZOMBIES INCOMING!",
            "SAVE THEM, KILL ZOMBIES!",
            "THE UNDEAD ARE NEAR!",
            "DON'T LET THE ZOMBIES BITE!",
            "PEOPLE NEED YOUR HELP!",
            "TAKE DOWN THE UNDEAD!",
            "CIVILIANS IN DANGER!",
            "ZOMBIES ON THE MOVE!",
            "SAVE HUMANITY!",
            "DON'T LET THEM GET AWAY!",
            "ZOMBIES ATTACKING!",
            "DEFEND THE WEAK!",
            "WATCH OUT! ZOMBIES CLOSE!",
            "ELIMINATE THE THREAT!",
            "FIGHT FOR SURVIVAL!",
        };
        int index = Random.Range(0, lines.Length);
        return lines[index];
    }
}
