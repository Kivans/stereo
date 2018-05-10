using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sound_WebGL : MonoBehaviour {

    #region Variables
    private AudioSource OpeningTheme;
    private AudioSource GameSound;
    private AudioSource PointSound;
    private AudioSource[] LeftSounds;
    private AudioSource[] RightSounds;

    private int lives;
    private int rewardPoints;
    private int rand;
    private int points;
    private int streak;
    private int countToStreak;
    private bool isLeft;
    private bool playing;
    private float TimerPlaceholder;

    private List<GameObject> Lives;
    private List<GameObject> Panels;
    private GameObject OpeningPanel;
    private GameObject InstructionsPanel;
    private GameObject GamePanel;
    private GameObject PausePanel;
    private GameObject SummaryPanel;
    private Slider TimeSlider;
    private Color GamePanelColor;

    public float Timer;
    public GameObject PointsPrefab;

    #endregion

    #region Methods
    //Start method
    private void Start()
    {
        TimerPlaceholder = Timer;
        OpeningPanel = GameObject.Find("Opening");
        InstructionsPanel = GameObject.Find("Instructions");
        GamePanel = GameObject.Find("Game");
        GamePanelColor = GamePanel.GetComponent<Image>().color;
        PausePanel = GameObject.Find("Pause");
        SummaryPanel = GameObject.Find("Summary");
        TimeSlider = GamePanel.transform.Find("Slider").GetComponent<Slider>();
        Panels = new List<GameObject>();

        foreach (Transform item in GameObject.Find("Canvas").transform)
        {
            Panels.Add(item.gameObject);
        }

        RightSounds = GameObject.Find("Right").GetComponents<AudioSource>();
        LeftSounds = GameObject.Find("Left").GetComponents<AudioSource>();
        OpeningTheme = GameObject.Find("Canvas").GetComponent<AudioSource>();
        MainMenu();
    }

    //Go back to main menu
    public void MainMenu()
    {
        ShowPanel(OpeningPanel);
        if (!OpeningTheme.isPlaying)
            OpeningTheme.Play();
    }

    //Checks if you should get a point or lose 
    private void Point(bool score)
    {
        playing = false;
        GamePanel.GetComponent<Image>().color = GamePanelColor;
        StopAllCoroutines();

        //Correct
        if (score)
        {
            PointSound = GamePanel.GetComponents<AudioSource>()[0];
            PointSound.Play();
            rewardPoints = streak * (int)(Mathf.Ceil((TimeSlider.value) * 10));
            points += rewardPoints;
            PopupPoints();

            if (points > PlayerPrefs.GetInt("Highscore"))
            {
                PlayerPrefs.SetInt("Highscore", points);
            }

            countToStreak++;
            if (countToStreak == 6)
            {
                streak++;
                countToStreak = 0;
                Timer += 0.0012f;
            }
        }
        //Wrong
        else
        {
            lives--;
            if (lives <= 0)
            {
                Summary();
                return;
            }
            PointSound = GamePanel.GetComponents<AudioSource>()[1];
            PointSound.Play();
            streak = 1;
            countToStreak = 0;
            GameObject lifeToKill = Lives[Lives.Count - 1];
            lifeToKill.GetComponent<RawImage>().enabled = false;
            Lives.Remove(lifeToKill);
        }

        //Start another game in 0.25 seconds
        Invoke("Play", 0.45f);
    }

    //After answering correctly, popup the number of points given
    private void PopupPoints()
    {
        PointsPrefab.GetComponent<Text>().text = "+" + rewardPoints.ToString();
        Vector3 prefabLocation = new Vector3(Random.Range(-350, 333), Random.Range(-80, 160), 1);
        GameObject popup = Instantiate(PointsPrefab, GameObject.Find("Canvas").transform, true) as GameObject;
        popup.transform.localPosition = prefabLocation;
    }

    //Controls
    void Update()
    {
        //Makes sure you're in game and not menu
        if (playing)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (isLeft == true)
                {
                    Point(true);
                }
                else
                {
                    Point(false);
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (isLeft == false)
                {
                    Point(true);
                }
                else
                {
                    Point(false);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    public void ClickScreen(string side)
    {
        if (side == "left")
        {
            if (isLeft == true)
            {
                Point(true);
            }
            else
            {
                Point(false);
            }
        }
        else if (side == "right")
        {
            if (isLeft == false)
            {
                Point(true);
            }
            else
            {
                Point(false);
            }
        }
    }


    //The player clicks the start button on the menu
    public void StartGame()
    {
        points = 0;
        countToStreak = 0;
        streak = 1;
        Timer = TimerPlaceholder;
        lives = 4;
        ShowPanel(GamePanel);

        //Refresh lives
        Lives = new List<GameObject>() { GameObject.Find("Life1"), GameObject.Find("Life2"), GameObject.Find("Life3") };
        foreach (var item in Lives)
        {
            item.GetComponent<RawImage>().enabled = true;
        }

        Play();
    }

    //Play the sound + Mechanics
    private void Play()
    {
        playing = true;
        if (OpeningTheme.isPlaying)
            OpeningTheme.Stop();
        if (GameSound != null)
            GameSound.pitch = 1;

        //Update text fields
        GamePanel.transform.Find("Highscore").GetComponent<Text>().text = "Highscore: " + PlayerPrefs.GetInt("Highscore").ToString();
        GamePanel.transform.Find("Multiplier").GetComponent<Text>().text = "Multiplier: x" + streak.ToString();
        GameObject.Find("Points").GetComponent<Text>().text = points.ToString();

        //Refresh time
        TimeSlider.value = TimeSlider.maxValue;

        //Mechanics of the game
        rand = Random.Range(1, 3);
        if (rand == 1)
            isLeft = true;
        else isLeft = false;

        if (isLeft == true)
            GameSound = LeftSounds[Random.Range(0, LeftSounds.Length)];
        else
            GameSound = RightSounds[Random.Range(0, RightSounds.Length)];

        #region OPPOSITE MODE
        //Opposite mode:
        //Pick a random number between 1 and 7.
        if (points >= 100)
        {
            rand = Random.Range(1, 7);

            //If isLeft is true make it false, and vice versa, also change color of background for player's clarity
            //Thus, changing the direction, if the sound is coming from the left, you need to click right to win
            if (rand == 1)
            {
                if (isLeft)
                    isLeft = false;
                else
                    isLeft = true;
                GamePanel.GetComponent<Image>().color = new Color(0.525f, 0.572f, 0.572f);
                GameSound.pitch = 1.5f;
            }
        }
        #endregion

        GameSound.Play();
        StartCoroutine(SliderMechanics(Timer));
    }

    //Shows a specific panel and disables the rest of the array
    private void ShowPanel(GameObject panel)
    {
        foreach (var item in Panels)
        {
            item.SetActive(false);
        }
        panel.SetActive(true);
    }

    //Instructions button
    public void Instructions()
    {
        ShowPanel(InstructionsPanel);
    }

    //Returns to the main menu
    public void Return()
    {
        ShowPanel(OpeningPanel);
    }

    //Pause
    private void Pause()
    {
        playing = false;
        StopAllCoroutines();
        ShowPanel(PausePanel);
    }

    //User has lost his 4 lives and goes to game summary
    private void Summary()
    {
        playing = false;
        ShowPanel(SummaryPanel);
        OpeningTheme.Play();
        SummaryPanel.transform.Find("Text_endScore").gameObject.GetComponent<Text>().text = "Score: " + points.ToString();

        //Show "new highscore" or "nice try.." depends on points relative to old highscore
        if (points >= PlayerPrefs.GetInt("Highscore") && points != 0)
            SummaryPanel.transform.Find("Text_endTitle").gameObject.GetComponent<Text>().text = "NEW HIGHSCORE!";
        else
            SummaryPanel.transform.Find("Text_endTitle").gameObject.GetComponent<Text>().text = "NICE TRY..";

    }

    //Exits the game
    public void Quit()
    {
        Application.Quit();
    }

    //IEnums to control time with 'WaitForSeconds'
    IEnumerator SliderMechanics(float time)
    {
        while (TimeSlider.value > 0)
        {
            yield return new WaitForSeconds(0.001f);
            TimeSlider.value -= time;
        }
        Point(false);
    }
    #endregion
}
