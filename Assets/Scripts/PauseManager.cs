using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : SingletonMonoBehaviour<PauseManager>
{
    public bool isPaused = false;

    public void PauseGame()
    {
        // Debug.Log("Calling pause");
        Time.timeScale = 0;
        isPaused = true;
    }

    public void ResumeGame()
    {
        // Debug.Log("Calling resume");
        Time.timeScale = 1;
        isPaused = false;
    }
}
