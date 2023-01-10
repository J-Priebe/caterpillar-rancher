using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

using GooglePlayGames;

using GooglePlayGames.BasicApi;

public class SocialManager : SingletonMonoBehaviour<SocialManager>
{
    private bool _isAuthenticated = false;

    void Start()
    {
#if !UNITY_EDITOR

        // PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(OnSignInResult);
#endif
    }

    internal void DoAuthenticate()
    {
        PlayGamesPlatform.Instance.ManuallyAuthenticate(OnSignInResult);
    }

    internal void OnSignInResult(SignInStatus status)
    {
        Debug.Log("SOCIAL TEST: CALLING ProcessAuthentication with status " + status);
        if (status == SignInStatus.Success)
        {
            _isAuthenticated = true;
            // Continue with Play Games Services
        }
        else
        {
            // If automatic auth fails, we show a sign in button for manual authentication.
            // See SignInButton.cs
            _isAuthenticated = false;
            Debug.LogError("Failed to authenticate. Status: " + status);
        }
    }

    public void SubmitScore(int score)
    {
#if !UNITY_EDITOR
        // post score 12345 to leaderboard ID "Cfji293fjsie_QA")
        Social.ReportScore(
            score,
            GPGSIds.leaderboard_high_score,
            (bool success) =>
            {
                if (!success)
                {
                    Debug.LogError("Failed to report high score");
                }
            }
        );
#endif
    }

    public void SignIn()
    {
        DoAuthenticate();
    }

    public bool IsAuthenticated()
    {
        return _isAuthenticated; // PlayGamesPlatform.Instance.IsAuthenticated();
    }
}
