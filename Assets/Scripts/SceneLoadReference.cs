using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// scene-specific class to reference persistent scenemanager from local gameobjects (e.g., buttons )

public class SceneLoadReference : MonoBehaviour
{
    [SerializeField] GameObject settingsPage;

    public void LoadGame()
    {
        SceneControllerManager.Instance.LoadGameScene();
    }

    public void LoadHomeScreen()
    {
        SceneControllerManager.Instance.LoadHomeScene();
    }

    public void ShowSettingsPage()
    {
        settingsPage.SetActive(true);
    }

    public void HideSettingsPage()
    {
        settingsPage.SetActive(false);
    }
}
