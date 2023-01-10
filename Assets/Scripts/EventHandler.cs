using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventHandler
{
    // Scene Load Events - in the order they happen

    // Before Scene Unload Fade Out Event
    public static event Action<string, string> BeforeSceneUnloadFadeOutEvent;

    public static void CallBeforeSceneUnloadFadeOutEvent(string oldSceneName, string newSceneName)
    {
        if (BeforeSceneUnloadFadeOutEvent != null)
        {
            BeforeSceneUnloadFadeOutEvent(oldSceneName, newSceneName);
        }
    }

    // Before Scene Unload Event
    public static event Action BeforeSceneUnloadEvent;

    public static void CallBeforeSceneUnloadEvent()
    {
        if (BeforeSceneUnloadEvent != null)
        {
            BeforeSceneUnloadEvent();
        }
    }

    // After Scene Loaded Event
    public static event Action AfterSceneLoadEvent;

    public static void CallAfterSceneLoadEvent()
    {
        if (AfterSceneLoadEvent != null)
        {
            AfterSceneLoadEvent();
        }
    }

    // After Scene Load Fade In Event
    public static event Action<string, string> AfterSceneLoadFadeInEvent;

    public static void CallAfterSceneLoadFadeInEvent(string oldSceneName, string newSceneName)
    {
        if (AfterSceneLoadFadeInEvent != null)
        {
            AfterSceneLoadFadeInEvent(oldSceneName, newSceneName);
        }
    }
}