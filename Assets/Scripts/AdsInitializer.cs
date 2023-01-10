using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;


public class AdsInitializer : SingletonMonoBehaviour<AdsInitializer>, IUnityAdsInitializationListener, IUnityAdsShowListener, IUnityAdsLoadListener
{
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    string _gameId;
    [SerializeField] bool _testMode = false;

    private bool isShowingAd;

    protected override void Awake()
    {
        base.Awake();
        if (Advertisement.isInitialized)
        {
            Debug.Log("Advertisement already Initialized");
        }
        else
        {
            InitializeAds();
        }
    }
    public void InitializeAds()
    {
        _gameId = (Application.platform == RuntimePlatform.IPhonePlayer) ? _iOSGameId : _androidGameId;
        Advertisement.Initialize(_gameId, _testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Ads initialization complete");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    // basic interstitial ad
    public void LoadInterstitialAd()
    {
        // add units defined in Unity Monetization Dashhboard
        // this class is the listener for load events
        Advertisement.Load("Interstitial_Android", this);
    }

    public void HideBannerAd()
    {
        Debug.Log("Hiding banner");
        Advertisement.Banner.Hide();
    }

    // Entry point for showing banner ad.
    // banner shows at bottom of screen
    public void ShowBannerAd()
    {
        StartCoroutine(ShowBannerAfterInialization());
    }

    private IEnumerator ShowBannerAfterInialization() {
        yield return new WaitUntil(() => Advertisement.isInitialized);
        if (Advertisement.Banner.isLoaded)
        {
            Debug.Log("Banner already loaded. showing");
            OnBannerLoaded();
        }else
        {
            Debug.Log("Banner not yet loaded.");
            LoadBannerAd(); // which then calls OnBannerLoaded
        }
    }


    private void LoadBannerAd()
    {
        // Debug.Log("Loading banner");
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Load("Banner_Android",
            new BannerLoadOptions
            {
                loadCallback = OnBannerLoaded,
                errorCallback = OnBannerError
            }
            );
    }

    // shows when load complete
    public void OnUnityAdsAdLoaded(string placementId)
    {
        // Debug.Log("OnUnityAdsAdLoaded");
        Advertisement.Show(placementId, this);
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Error showing Ad Unit {placementId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError("OnUnityAdsShowFailure");
    }

    // this is where we want to pause the game
    public void OnUnityAdsShowStart(string placementId)
    {
        // Debug.Log("OnUnityAdsShowStart");
        // PauseManager.Instance.PauseGame();
        isShowingAd = true;
    }

    // TODO need to make sure we can return smoothly if they hit play again
    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("OnUnityAdsShowClick");
    }


    // this seems to be bugged and not triggering with 3.7
    // try toggling test mode on/off
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("OnUnityAdsShowComplete " + showCompletionState);
        isShowingAd = false;
    }

    // so countdown doesn't start while we're showing ad
    public bool IsShowingAd()
    {
        return isShowingAd;
    }


    void OnBannerLoaded()
    {
        // TODO check this actually refreshes to new ads
        Debug.Log("Banner loaded");
        Advertisement.Banner.Show("Banner_Android");
    }

    void OnBannerError(string message)
    {
        // usually it's a fail to fill :(
        Debug.Log("Banner failed to load: " + message);
        // an empty banner must be destroyed, or we'll get "already loaded" errors
        Advertisement.Banner.Hide(true);
    }

}