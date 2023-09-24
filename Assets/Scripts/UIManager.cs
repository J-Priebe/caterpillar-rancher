using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : SingletonMonoBehaviour<UIManager>
{

    // combined pause/ game over.. with attributes hidden/shown
    [SerializeField] GameObject pauseMenu;
    [SerializeField] TextMeshProUGUI pauseMenuLevel;
    [SerializeField] TextMeshProUGUI pauseMenuScore;
    [SerializeField] GameObject  resumeButton; // pause only
    [SerializeField] GameObject  playAgainButton;  // game over only
    [SerializeField] GameObject gameOverBanner;

    // [SerializeField] GameObject gameOverMenu;

    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI oneUpCounterText;
    [SerializeField] TextMeshProUGUI superFoodCounterText;
    [SerializeField] TextMeshProUGUI segmentCounterText;

    // [SerializeField] TextMeshProUGUI quickPickupBanner;
    [SerializeField] GameObject scoreBannerPrefab;

    [SerializeField] GameObject oneUpDecrementBanner;
    [SerializeField] GameObject tutorialPrompt;
    [SerializeField] GameObject tutorialPromptAcknowledgeButton;
    [SerializeField] TextMeshProUGUI tutorialPromptText;


    [SerializeField] GameObject pauseButton;
    [SerializeField] TextMeshProUGUI countdownText;


    [SerializeField] GameObject levelInterstitialMenu;
    // hungrier,faster, etc
    [SerializeField] TextMeshProUGUI nextLevelLabel;
    [SerializeField] TextMeshProUGUI nextLevelDescriptor;


    WaitForSecondsRealtime oneSecond;
    WaitForSeconds oneSecondScaled;


    private void OnDisable()
    {

        EventHandler.AfterSceneLoadFadeInEvent -= HandleSceneLoaded;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadFadeInEvent += HandleSceneLoaded;
    }

    void Start()
    {
        countdownText.gameObject.SetActive(false);

        oneSecond = new WaitForSecondsRealtime(1f);
        oneSecondScaled = new WaitForSeconds(1f);
    }

    void Update()
    {
        oneUpCounterText.text = "x " + LevelManager.Instance.oneUpsRemaining.ToString();

        segmentCounterText.text =
            LevelManager.Instance.foodCollected
            + "/"
            + LevelManager.Instance.foodToWin
        ;

        superFoodCounterText.text =
            LevelManager.Instance.superFoodCollected % LevelManager.superFoodsPerOneUp
            + "/"
            + LevelManager.superFoodsPerOneUp
        ;

    }

    public void PauseGame()
    {
        if (!PauseManager.Instance.isPaused)
        {
            PauseManager.Instance.PauseGame();
            SetPauseButtonActive(false);

            pauseMenuScore.text = LevelManager.Instance.currentScore.ToString();
            pauseMenuLevel.text = LevelManager.Instance.currentLevel.ToString();

            gameOverBanner.SetActive(false);
            playAgainButton.SetActive(false);
            resumeButton.SetActive(true);

            pauseMenu.SetActive(true);
            AdsInitializer.Instance.ShowBannerAd();
        }
    }


    public void ResumeGame()
    {
        AdsInitializer.Instance.HideBannerAd();
        if  (PauseManager.Instance.isPaused)
        {
            PauseManager.Instance.ResumeGame();
            pauseMenu.SetActive(false);
            SetPauseButtonActive(true);
        }
    }

    public void DisplayLevelInterstitial(int nextLevel)
    {

        bool hungrier =
            LevelManager.Instance.GetFoodToWin(nextLevel)
            > LevelManager.Instance.GetFoodToWin(nextLevel - 1);

        bool faster = Caterpillar.Instance.GetSpeedForLevel(nextLevel)
            > Caterpillar.Instance.GetSpeedForLevel(nextLevel - 1);

        string desc = "";
        if (hungrier && faster)
        {
            desc = "Your bugs are hungrier and faster!";
        }
        else if (hungrier)
        {
            desc = "Your bugs are hungrier!";
        }
        else if (faster)
        {
            desc = "Your bugs are faster!";
        }
        PauseManager.Instance.PauseGame();

        nextLevelLabel.SetText(nextLevel.ToString());
        nextLevelDescriptor.SetText(desc);

        levelInterstitialMenu.SetActive(true);
    }

    public void DisplayGameOver(int level, int score)
    {
        SetPauseButtonActive(false);

        pauseMenuScore.text = score.ToString();
        pauseMenuLevel.text = level.ToString();

        gameOverBanner.SetActive(true);
        playAgainButton.SetActive(true);
        resumeButton.SetActive(false);

        pauseMenu.SetActive(true);
    }

    public void HandleSceneLoaded(string oldSceneName, string sceneName)
    {
        if (sceneName == "MainGameScene")
        {
            BeginCountdownCoroutine();
        }
    }

    private void BeginCountdownCoroutine()
    {
        PauseManager.Instance.PauseGame();
        StartCoroutine(BeginCountDown());
    }

    private IEnumerator BeginCountDown()
    {
        // delay countdown until interstitial ad completes
        // NOTE: we are dependent on ads only appearing
        // at this particular spot to enforce when we call pause/resume.
        yield return new WaitUntil(() => (!AdsInitializer.Instance.IsShowingAd()));

        countdownText.SetText("3");
        countdownText.gameObject.SetActive(true);

        yield return oneSecond;
        countdownText.SetText("2");

        yield return oneSecond;
        countdownText.SetText("1");

        yield return oneSecond;
        countdownText.gameObject.SetActive(false);
        SetPauseButtonActive(true);
        PauseManager.Instance.ResumeGame();
    }

    public void EndGame()
    {
        // #if !UNITY_EDITOR
        // // ads completion doesnt properly trigger in editor
        // AdsInitializer.Instance.LoadInterstitialAd();
        // #endif

        LevelManager.Instance.HandleEndGame();
        if (!(
            TutorialManager.Instance.tutorialState == TutorialState.UntilFirstCrash 
            || TutorialManager.Instance.tutorialState == TutorialState.Complete
            ))
        {
            TutorialManager.Instance.ResetToStart();
        }
        SceneControllerManager.Instance.LoadHomeScene();
        // gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        levelInterstitialMenu.SetActive(false);
    }

    public void PlayAgain()
    {

        // #if !UNITY_EDITOR
        // AdsInitializer.Instance.LoadInterstitialAd();
        // #endif

        SceneControllerManager.Instance.LoadGameScene();
        // gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        levelInterstitialMenu.SetActive(false);
    }

    public void ContinueNextLevel()
    {
        SceneControllerManager.Instance.LoadGameScene();
        // gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        levelInterstitialMenu.SetActive(false);
    }

    public void SetPauseButtonActive(bool active)
    {
        pauseButton.SetActive(active);
    }

    public void SetOneUpCounterText(int i)
    {
        oneUpCounterText.SetText("X " + i);
    }

    // show and hide are toggled from a Caterpillar.cs coroutine
    public void ShowOneUpDecrementBanner(Vector3 position)
    {
        oneUpDecrementBanner.SetActive(true);
    }

    public void HideOneUpDecrementBanner()
    {
        oneUpDecrementBanner.SetActive(false);
    }

    public void ShowTutorialPrompt(string promptText, bool requireAcknowledge)
    {
        tutorialPromptText.SetText(promptText);
        tutorialPromptAcknowledgeButton.SetActive(requireAcknowledge);
        tutorialPrompt.SetActive(true);
    }

    public void HideTutorialPrompt()
    {
        tutorialPrompt.SetActive(false);
    }

    public void ShowScoreBanner(Vector3 position, int amount, bool isQuick)
    {
        StartCoroutine(ScoreBannerCoroutine(position, amount, isQuick));
    }


    private IEnumerator ScoreBannerCoroutine(Vector3 position,int amount, bool isQuick)
    {
        GameObject banner = PoolManager.Instance.ReuseObject(
            scoreBannerPrefab, position, Quaternion.identity
        );

        foreach (TextMeshProUGUI textComponent in banner.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if(textComponent.CompareTag("ScoreBannerTextPrefix"))
            {
                textComponent.SetText(isQuick? "Quick!" : "");
            }
            else
            {
                textComponent.SetText(amount.ToString());
            }
        }

        banner.SetActive(true);
        yield return oneSecond;
        banner.SetActive(false);
    }
}
