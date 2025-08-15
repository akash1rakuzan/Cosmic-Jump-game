
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public static bool isInfiniteMode = false; // ADD THIS
    public TextMeshProUGUI ringCounterText;

    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI reachedLevelText;

    public static bool gameOver;
    public static bool levelWin;
    public static bool isHardMode = false;
    public static bool gameJustStarted = true;
    public Toggle hardModeToggle;

    public GameObject gameOverPannel;
    public GameObject levelWinPannel;
    public GameObject StartPannel;
    public static int CurrentLevelIndex;
    public static int noOfPassingRings;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI nextLevelText;
    public Slider ProgressBar;
    public Button ExitPlayState;
    private int currentMaxLevel;


    public TextMeshProUGUI startPanelLevelText;
    public TextMeshProUGUI infiniteModeHighScoreText;

    private const string FirstLaunchKey = "HasLaunchedBefore";

    private const string StandardLevelKey = "StandardLevelIndex";
    private const string HardLevelKey = "HardLevelIndex";


    public Image hardModeIndicatorImage; // Assign in inspector
    public Color standardModeColor = Color.green;
    public Color hardModeColor = Color.red;

    public GameObject ball;
    public static bool SpawnBallAtStart = false;

    public Toggle soundToggle;
    public static bool isSoundOn = true;

    public TextMeshProUGUI hardModeMaxLevelText;

    //private bool hasSavedHighScore = false;
    private bool hasSavedLevelProgress = false;

    public GameObject newHighScoreIndicator; // Assign your GIF/image in inspector
    private bool hasShownHighScoreIndicator = false; // To ensure it shows once per run
    private float indicatorDisplayTime = 3f; // How long to show the gif

    private Vector3 indicatorOriginalPosition;

    [SerializeField]
    HelixManager helixManager;

    private void Start()
    {
        //delete this - testing purposes //
        //PlayerPrefs.SetInt(HardLevelKey, 1);
        //PlayerPrefs.SetInt(StandardLevelKey, 1);

        //PlayerPrefs.SetInt("HardModeMaxLevel", 1);
        //PlayerPrefs.SetInt("InfiniteHighScore", 0);
        //delete this - testing purposes //

        // Ensure defaults on first launch
        EnsureFirstLaunchDefaults();

        Application.targetFrameRate = 60;

        Time.timeScale = 1f;
        noOfPassingRings = 0;
        gameOver = false;
        levelWin = false;

        // Restore hard mode toggle from PlayerPrefs
        int hardModePref = PlayerPrefs.GetInt("HardModeEnabled", 0);
        isHardMode = hardModePref == 1;

        // Set CurrentLevelIndex from the correct PlayerPref right away
        CurrentLevelIndex = isHardMode ? PlayerPrefs.GetInt(HardLevelKey, 1) : PlayerPrefs.GetInt(StandardLevelKey, 1);

        // Update the UI toggle and bind listener AFTER we've read the saved toggle
        hardModeToggle.isOn = isHardMode;
        hardModeToggle.onValueChanged.AddListener(delegate { OnHardModeToggleChanged(); });

        UpdateStartPanelLevelText();
        UpdateHardModeIndicatorColor();
        if (infiniteModeHighScoreText != null)
        {
            int savedHighScore = PlayerPrefs.GetInt("InfiniteHighScore", 0);
            infiniteModeHighScoreText.text = "High Score: " + savedHighScore;
        }

        if (gameJustStarted)
        {
            StartPannel.SetActive(true);
            ProgressBar.gameObject.SetActive(false);
            ExitPlayState.gameObject.SetActive(false);
        }
        else
        {
            StartPannel.SetActive(false);
            if (!isInfiniteMode)
                ProgressBar.gameObject.SetActive(true);
            ExitPlayState.gameObject.SetActive(true);
            Time.timeScale = 1f;
        }

        if (SpawnBallAtStart)
            ball.SetActive(true);

        int soundPref = PlayerPrefs.GetInt("SoundEnabled", 1); // 1 = on, 0 = off
        isSoundOn = soundPref == 1;
        soundToggle.isOn = isSoundOn;
        soundToggle.onValueChanged.AddListener(delegate { OnSoundToggleChanged(); });

        if (newHighScoreIndicator != null)
            indicatorOriginalPosition = newHighScoreIndicator.transform.localPosition;
    }


    private void Update()
    {


        //if (StartPannel.activeSelf)
        //{
        //    // Only proceed if click was NOT on a UI element
        //    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        //    {
        //        isHardMode = hardModeToggle.isOn;
        //        StartPannel.SetActive(false);
        //        ProgressBar.gameObject.SetActive(true);
        //        Time.timeScale = 1f;
        //        gameJustStarted = false;
        //    }
        //    return;
        //}

        if (StartPannel.activeSelf)
        {
            return; // Do nothing until user presses the Start Game button
        }


        if (isInfiniteMode)
        {
            ringCounterText.gameObject.SetActive(true);
            ExitPlayState.gameObject.SetActive(true);
            ringCounterText.text = "Rings: " + noOfPassingRings;
            finalScoreText.text = "Final Score: " + noOfPassingRings;

            int highScore = PlayerPrefs.GetInt("InfiniteHighScore", 0);
            if (noOfPassingRings > highScore)
            {
                PlayerPrefs.SetInt("InfiniteHighScore", noOfPassingRings);
                PlayerPrefs.Save();
                highScore = noOfPassingRings;

                // Only show indicator if not shown before
                if (!hasShownHighScoreIndicator && newHighScoreIndicator != null)
                {
                    StartCoroutine(ShowHighScoreIndicator());
                    hasShownHighScoreIndicator = true;
                }
            }

            highScoreText.text = "High Score: " + highScore;
        }
        else
        {
            ringCounterText.gameObject.SetActive(false);


            int playerProgress = noOfPassingRings * 100 / FindFirstObjectByType<HelixManager>().noOfrings;

        }


        if (gameOver)
        {
            //Time.timeScale = 0;
            newHighScoreIndicator.SetActive(false);
            ProgressBar.gameObject.SetActive(false);
            gameOverPannel.SetActive(true);
            ExitPlayState.gameObject.SetActive(false);

            if (isInfiniteMode)
            {
                ringCounterText.gameObject.SetActive(false);
                finalScoreText.gameObject.SetActive(true);
                highScoreText.gameObject.SetActive(true);


                
            }
            else
            {
                finalScoreText.gameObject.SetActive(false);
                highScoreText.gameObject.SetActive(false);

            }
        }

        


        currentLevelText.text = CurrentLevelIndex.ToString();
        nextLevelText.text = (CurrentLevelIndex + 1).ToString();
        int progress = noOfPassingRings * 100 / FindFirstObjectByType<HelixManager>().noOfrings;
        ProgressBar.value = progress;

        if (levelWin)
        {
            reachedLevelText.text = "Reached Level: " + currentLevelText.text;
            ProgressBar.gameObject.SetActive(false);
            levelWinPannel.SetActive(true);
            ExitPlayState.gameObject.SetActive(false);
            //if (Input.GetMouseButtonDown(0))
            //{

            //    PlayerPrefs.SetInt("CurrentLevelIndex", CurrentLevelIndex + 1);
            //    SceneManager.LoadScene(0);

            //}
            if (levelWin)
            {
                levelWinPannel.SetActive(true);
                ExitPlayState.gameObject.SetActive(false);

                if (ball != null)
                    ball.SetActive(false); // Disable ball when level is won

                if (!hasSavedLevelProgress)
                {
                    CurrentLevelIndex++; // Advance level since it was won

                    if (isHardMode)
                    {
                        PlayerPrefs.SetInt(HardLevelKey, CurrentLevelIndex);

                        int maxLevel = PlayerPrefs.GetInt("HardModeMaxLevel", 1);
                        if (CurrentLevelIndex > maxLevel)
                        {
                            PlayerPrefs.SetInt("HardModeMaxLevel", CurrentLevelIndex);
                            StartCoroutine(ShowHighScoreIndicator(150f));
                        }
                    }
                    else
                    {
                        PlayerPrefs.SetInt(StandardLevelKey, CurrentLevelIndex);
                    }
                    PlayerPrefs.Save();
                    hasSavedLevelProgress = true;
                }
            }

        }





    }
    public void StartGame()
    {

        if (ball != null)
            ball.SetActive(true);

        isHardMode = hardModeToggle.isOn;
        UpdateHardModeIndicatorColor();


        if (isHardMode)
        {
            CurrentLevelIndex = PlayerPrefs.GetInt(HardLevelKey, 1);
        }
        else
        {
            CurrentLevelIndex = PlayerPrefs.GetInt(StandardLevelKey, 1);
        }

        StartPannel.SetActive(false);
        ProgressBar.gameObject.SetActive(true);
        ExitPlayState.gameObject.SetActive(true);
        Time.timeScale = 1f;
        gameJustStarted = false;
        UpdateStartPanelLevelText();
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    public void ReturnToStartMenu()
    {
        if (isInfiniteMode)
        {

            finalScoreText.text = "Final Score: " + noOfPassingRings;

            int highScore = PlayerPrefs.GetInt("InfiniteHighScore", 0);
            if (noOfPassingRings > highScore)
            {
                PlayerPrefs.SetInt("InfiniteHighScore", noOfPassingRings);
                PlayerPrefs.Save();
                highScore = noOfPassingRings;
            }

            highScoreText.text = "High Score: " + highScore;
        }

        if (levelWin)
        {
            

            UpdateHardModeIndicatorColor();

            levelWin = false; // reset flag
        }
        else if (gameOver && isHardMode)
        {

            PlayerPrefs.SetInt(HardLevelKey, 1);
            PlayerPrefs.Save();


        }


        gameOverPannel.SetActive(false);

        gameJustStarted = true;
        isInfiniteMode = false;
        SpawnBallAtStart = false;
        UpdateStartPanelLevelText();
        SceneManager.LoadScene(0);

    }
    public void RetryGame()
    {

        if (isHardMode)
        {
            CurrentLevelIndex = 1;
            PlayerPrefs.SetInt(HardLevelKey, 1); // Reset stored hard mode progress
        }
        else
        {
            CurrentLevelIndex = PlayerPrefs.GetInt(StandardLevelKey, 1);
        }

        gameJustStarted = false;
        UpdateStartPanelLevelText();
        SpawnBallAtStart = true;
        SceneManager.LoadScene(0);

    }
    public void ContinueToNextLevel()
    {
        

        SpawnBallAtStart = true;
        SceneManager.LoadScene(0);

    }

    public void StartInfiniteMode()
    {
        if (ball != null)
            ball.SetActive(true);

        isInfiniteMode = true;
        isHardMode = false; // Optional: disable hard mode for infinite
        gameJustStarted = false;
        hasShownHighScoreIndicator = false; // Reset for new run
        if (newHighScoreIndicator != null)
            newHighScoreIndicator.SetActive(false); // Hide at start

        StartPannel.SetActive(false);
        ProgressBar.gameObject.SetActive(false); // Hide progress bar
        ExitPlayState.gameObject.SetActive(false);
        Time.timeScale = 1f;
        helixManager.StartInfiniteMode();
    }


    public void UpdateStartPanelLevelText()
    {
        int level = CurrentLevelIndex;

        string mode = isHardMode ? "Hard Mode" : "Standard Mode";

        //startPanelLevelText.text = $"{mode} - Level {level}";
        startPanelLevelText.text = $"Level {level}";
        if (isHardMode)
        {
            int maxHard = PlayerPrefs.GetInt("HardModeMaxLevel", 1);
            hardModeMaxLevelText.text = $"Highest Level: {maxHard}";
            hardModeMaxLevelText.gameObject.SetActive(true);
        }
        else { hardModeMaxLevelText.gameObject.SetActive(false); }


    }

    private void OnHardModeToggleChanged()
    {
        isHardMode = hardModeToggle.isOn;

        // Save the user's selection
        PlayerPrefs.SetInt("HardModeEnabled", isHardMode ? 1 : 0);
        PlayerPrefs.Save();

        // load the level index relevant to the newly selected mode
        CurrentLevelIndex = isHardMode ? PlayerPrefs.GetInt(HardLevelKey, 1) : PlayerPrefs.GetInt(StandardLevelKey, 1);


        UpdateStartPanelLevelText();
        UpdateHardModeIndicatorColor();
    }


    private void UpdateHardModeIndicatorColor()
    {
        if (hardModeIndicatorImage != null)
        {
            hardModeIndicatorImage.color = isHardMode ? hardModeColor : standardModeColor;
        }
    }

    public void OnSoundToggleChanged()
    {
        isSoundOn = soundToggle.isOn;
        PlayerPrefs.SetInt("SoundEnabled", isSoundOn ? 1 : 0);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
#else
            Application.Quit(); // Quit the built application
#endif
    }

    private void EnsureFirstLaunchDefaults()
    {
        if (!PlayerPrefs.HasKey(FirstLaunchKey))
        {
            Debug.Log("First launch - initializing PlayerPrefs defaults.");
            PlayerPrefs.SetInt(StandardLevelKey, 1);
            PlayerPrefs.SetInt(HardLevelKey, 1);
            PlayerPrefs.SetInt("HardModeMaxLevel", 1);
            PlayerPrefs.SetInt("InfiniteHighScore", 0);
            PlayerPrefs.SetInt(FirstLaunchKey, 1);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator ShowHighScoreIndicator()
    {
        newHighScoreIndicator.SetActive(true);
        yield return new WaitForSeconds(indicatorDisplayTime);
        newHighScoreIndicator.SetActive(false);
    }
    private IEnumerator ShowHighScoreIndicator(float yOffset = 0f)
    {
        if (newHighScoreIndicator == null)
            yield break;

        // Save original position
        Vector3 startPos = indicatorOriginalPosition;

        // Apply temporary offset (local position so it’s relative to UI layout)
        newHighScoreIndicator.transform.localPosition = startPos + new Vector3(0f, yOffset, 0f);

        // Show indicator
        newHighScoreIndicator.SetActive(true);
        yield return new WaitForSeconds(indicatorDisplayTime);

        // Hide and reset position
        newHighScoreIndicator.SetActive(false);
        newHighScoreIndicator.transform.localPosition = startPos;
    }

}