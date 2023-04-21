using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdDisplay : MonoBehaviour
{
    public string myGameIdAndroid = "YOUR_GAME_ID_HERE";
    public string myGameIdIOS = "YOUR_GAME_ID_HERE";

    public string adUnitIdAndroid = "Interstitial_Android";
    public string adUnitIdIOS = "Interstitial_iOS";

    public string myAdUnitId;
    public bool adStarted;

    private bool testMode = true;



    // Start is called before the first frame update
    void Start()
    {
#if UNITY_IOS
	        Advertisement.Initialize(myGameIdIOS, testMode);
	        myAdUnitId = adUnitIdIOS;
#else
        Advertisement.Initialize(myGameIdAndroid, testMode);
        myAdUnitId = adUnitIdAndroid;
#endif

    }

    // Update is called once per frame
    void Update()
    {
        if (Advertisement.isInitialized && !adStarted)
        {
            Advertisement.Load(myAdUnitId);
            Advertisement.Show(myAdUnitId);
            adStarted = true;
        }

    }
}