using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class SettingsManager : MonoBehaviour
{
    [SerializeField] private AudioMixer masterMixer;

    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle skipTutorialToggle;

    [SerializeField] private TextMeshProUGUI versionLabel;

    private float sfxAttenuation;
    private float musicAttenuation;
    private bool skipTutorial;

    private AudioSource sampleClipAudioSource;


    public void OnEndDrag()
    {
        // play sample sound when sfx slider stops
        sampleClipAudioSource.Play();
    }

    void Start()
    {
        versionLabel.SetText("Version " + Application.version);
    }

    void Awake()
    {
        sampleClipAudioSource = GetComponent<AudioSource>();

        skipTutorial = TutorialManager.Instance.tutorialState == TutorialState.Complete;
        skipTutorialToggle.SetIsOnWithoutNotify(skipTutorial);

        // when we set the value the handlers will get called and update the mixers
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume", 0.75f);
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 0.75f);
    }


    // attentuation is logarithmic; convert it from a linear slider value
    // see https://johnleonardfrench.com/the-right-way-to-make-a-volume-slider-in-unity-using-logarithmic-conversion/
    public void AdjustSFXAudioLevel(float sliderValue)
    {
        masterMixer.SetFloat("sfxVolume", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("sfxVolume", sliderValue);
    }

    public void AdjustMusicAudioLevel(float sliderValue)
    {
        masterMixer.SetFloat("musicVolume", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("musicVolume", sliderValue);
    }

    public void ToggleTutorialSkip()
    {
        // cn only toggle between start over and completed, no intermediates
        if (TutorialManager.Instance.tutorialState == TutorialState.Start)
        {
            TutorialManager.Instance.tutorialState = TutorialState.Complete;
            skipTutorial = true;
        }
        else
        {
            TutorialManager.Instance.tutorialState = TutorialState.Start;
            skipTutorial = false;
        }
    }
}
