using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LevelManager : SingletonMonoBehaviour<LevelManager>
{
    private int baseScorePerFoodPickup = 100;

    public int currentLevel = 1;
    public int currentScore = 0;

    // number of pickups to win a given level
    public int foodToWin;
    public int foodCollected = 0;

    public int superFoodCollected = 0;
    public const int superFoodsPerOneUp = 5;

    // start with one extra life, seems more friendly
    public int oneUpsRemaining = 1;

    private AudioSource audioSource;
    [SerializeField] private AudioClip levelWinAudioClip;
    [SerializeField] private AudioClip oneUpAudioClip;

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
    }

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
        // Debug.Log("LevelManager Start() called");
        oneUpsRemaining = 1;
        // UIManager.Instance.SetSuperFoodCounterText(0, superFoodsPerOneUp);
        UIManager.Instance.SetOneUpCounterText(1);
    }


    public void HandleSceneLoaded(string oldSceneName, string sceneName)
    {
        if (sceneName == "MainGameScene")
        {
            StartLevel();
            // Debug.Log("Level,Speed,Food To Win,Starting Segments");
            // for(int i =1; i<105; i++)
            // {
            //     Debug.Log(
            //         i + ","
            //         + Caterpillar.Instance.GetSpeedForLevel(i) + ","
            //         + GetFoodToWin(i) + ","
            //         + Caterpillar.Instance.GeStartingSegmentsForLevel(i)
            //     );

            // }
        }
    }

    public int GetFoodToWin(int level)
    {
        // see README on score
        if (level < 2) {
            return 5;
        }
        int foodToWin = Mathf.Min(
            5 + (level - level % 2) * 5/2,
            80
        );
        // Debug.Log("Points to win level " + level + ": " + foodToWin);
        return foodToWin;
    }

    private void IncreaseScore(int amount, bool quickBonus)
    {
        currentScore += amount;
        UIManager.Instance.ShowScoreBanner(Caterpillar.Instance.GetHeadPosition(), amount, quickBonus);
    }

    public void HandleFoodPickup(int currentCaterpillarSegmentsLength)
    {
        int score = baseScorePerFoodPickup;
        bool quick = false;
        if (FoodManager.Instance.FoodScoreBonusActive())
        {
            // Debug.Log("Quick! X2 score");
            score *= 2;
            quick = true;
        }

        IncreaseScore(score, quick);

        SetFoodCollected(foodCollected + 1);
        if (CheckWinCondition())
        {
            // on callback we complete the animation
            Caterpillar.Instance.StartWinAnimation(() => {
                HandleLevelWon();
            });
            // no need to disable collider, just check flag
            // Caterpillar.Instance.head.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    public void HandleSuperFoodPickup(int currentCaterpillarSegmentsLength)
    {
        // superfood gives a 3x score bonus
        IncreaseScore(baseScorePerFoodPickup * 3, false);

        superFoodCollected ++;

        if (superFoodCollected % superFoodsPerOneUp == 0)
        {
            oneUpsRemaining ++;
            audioSource.clip = oneUpAudioClip;
            audioSource.Play();
        }
        UIManager.Instance.SetOneUpCounterText(oneUpsRemaining);

    }

    public bool CheckWinCondition()
    {
        return foodCollected >= foodToWin;
    }

    void ResetFoodCollected()
    {
        SetFoodCollected(0);
        foodToWin = GetFoodToWin(currentLevel);
    }

    public void SetFoodCollected(int i)
    {
        // Debug.Log("Pickups counter set to " + i);
        foodCollected = i;
    }

    void StartLevel(){
        // Debug.Log("LevelManager StartLevel Called");
        ResetFoodCollected();
        // reset the caterpillar
        // this MUST happen before we spaw new food, otherwise
        // we could spawn food at the unsafe 0,0 location
        Caterpillar.Instance.ResetForNextLevel(currentLevel);
        // clear out last food that spawned in previous level
        FoodManager.Instance.DestroyFood();
        FoodManager.Instance.SpawnNewFood();
        ArrowManager.Instance.DestroyAll();
    }

    void HandleLevelWon()
    {
        // Debug.Log("You beat level " + currentLevel + "!");
        currentLevel += 1;
        audioSource.clip = levelWinAudioClip;
        audioSource.Play();

        // we can submit score liberally because it only
        // records new high scores
        SocialManager.Instance.SubmitScore(currentScore);

        UIManager.Instance.DisplayLevelInterstitial(currentLevel);
    }

    public void ConsumeOneUp(int caterpillarSegmentsRemaining)
    {
        SetFoodCollected(caterpillarSegmentsRemaining);
        oneUpsRemaining --;
        UIManager.Instance.SetOneUpCounterText(oneUpsRemaining);
        // Debug.Log("One ups remaining: " + oneUpsRemaining);
    }

    public void HandleGameOver()
    {
        UIManager.Instance.DisplayGameOver(currentLevel, currentScore);
        HandleEndGame();
    }

    // voluntary or via game over
    public void HandleEndGame()
    {
        PauseManager.Instance.PauseGame();
        // hacky check that we submit score on voluntary end and not just game over,
        // but don't submit it twice (since on game over the level manager resets it to 0)
        if (currentScore != 0)
        {
            SocialManager.Instance.SubmitScore(currentScore);
        }
        currentLevel = 1;
        currentScore = 0;
        superFoodCollected = 0;
        oneUpsRemaining = 1;
        // UIManager.Instance.SetSuperFoodCounterText(0, superFoodsPerOneUp);
        UIManager.Instance.SetOneUpCounterText(1);
    }
}
