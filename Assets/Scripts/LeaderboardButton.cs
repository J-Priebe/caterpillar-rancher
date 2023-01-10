using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class LeaderboardButton : MonoBehaviour
{
    public void ShowLeaderboard()
    {
        Social.ShowLeaderboardUI();
    }
}
