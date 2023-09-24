using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TutorialManager : SingletonMonoBehaviour<TutorialManager>
{

    private TutorialState _tutorialState;

    [SerializeField] private GameObject swipeRightHelper;
    [SerializeField] private GameObject swipeUpHelper;
    [SerializeField] private GameObject swipeLeftHelper;
    [SerializeField] private GameObject swipeDownHelper;

    private readonly TutorialState[] arrowPlacementStates = {
            TutorialState.PlaceFirstArrow,
            TutorialState.PlaceSecondArrow,
            TutorialState.PlaceThirdArrow,
            TutorialState.PlaceFourthArrow
    };

    public TutorialState tutorialState {
        get { return _tutorialState; }
        set {
            _tutorialState = value;
            // only save at checkpoints that won't break the tutorial
            // e.g., we can't save while forcing to place an arrow,
            // because on the next game the position won't make sense
            if (
                value == TutorialState.UntilFirstCrash
                || value == TutorialState.Complete
                || value == TutorialState.Start
            )
            {
                PlayerPrefs.SetString("tutorialState", ((int)value).ToString());
            }
        }

    }

    // when we need to pause for a tutorial prompt
    public bool isWaitingForTutorialPrompt {get;private set;}

    public void EnableTutorialPrompt(string promptText, bool requireAcknowledge)
    {
        if (requireAcknowledge)
        {
            isWaitingForTutorialPrompt = true;
        }
        UIManager.Instance.ShowTutorialPrompt(promptText, requireAcknowledge);
    }

    public void CloseTutorialPrompt()
    {
        isWaitingForTutorialPrompt = false;
        UIManager.Instance.HideTutorialPrompt();
    }


    public bool CanPlaceArrow(Vector3Int clickPosition, Vector3Int clickDirection)
    {
        // try to short-circuit as soon as possible
        return (
            (tutorialState >= TutorialState.UntilFirstCrash)
            || (
                tutorialState == TutorialState.PlaceFirstArrow
                && clickPosition == Constants.TUTORIAL_FIRST_ARROW_POSITION
                && clickDirection == Constants.TUTORIAL_FIRST_ARROW_DIRECTION
            )
            || (
                tutorialState == TutorialState.PlaceSecondArrow
                && clickPosition == Constants.TUTORIAL_SECOND_ARROW_POSITION
                && clickDirection == Constants.TUTORIAL_SECOND_ARROW_DIRECTION
            )
            || (
                tutorialState == TutorialState.PlaceThirdArrow
                && clickPosition == Constants.TUTORIAL_THIRD_ARROW_POSITION
                && clickDirection == Constants.TUTORIAL_THIRD_ARROW_DIRECTION
            )
            || (
                tutorialState == TutorialState.PlaceFourthArrow
                && clickPosition == Constants.TUTORIAL_FOURTH_ARROW_POSITION
                && clickDirection == Constants.TUTORIAL_FOURTH_ARROW_DIRECTION
            )
        );
    }

    public bool IsArrowPlacementStage()
    {
        return arrowPlacementStates.Contains(tutorialState);
    }

    public void AdvanceStage()
    {
        tutorialState =  tutorialState + 1;
        // things that happen upon transition to this state
        switch(_tutorialState)
        {
            case TutorialState.MoveToFirstArrow:
                break;
            case TutorialState.PlaceFirstArrow:
                EnableTutorialPrompt("Press and swipe to place arrows", false);
                swipeRightHelper.SetActive(true);
                break;
            case TutorialState.MoveToSecondArrow:
                CloseTutorialPrompt();
                swipeRightHelper.SetActive(false);
                break;
            case TutorialState.PlaceSecondArrow:
                EnableTutorialPrompt(
                    "You can place up to two arrows at once. Guide your bug to the apple",
                    false
                );
                swipeUpHelper.SetActive(true);
                break;
            case TutorialState.MoveToLeaf:
                CloseTutorialPrompt();
                swipeUpHelper.SetActive(false);
                break;
            case TutorialState.MoveToApple:
                EnableTutorialPrompt(
                    "Good Job! With each leaf your bug's tail grows. Don't crash into it!",
                    true
                );
                swipeUpHelper.SetActive(false);
                break;
            case TutorialState.WallAhead:
                EnableTutorialPrompt(
                    "Apples disappear quickly! Pick up 5 for an extra life",
                    true
                );
                break;
            case TutorialState.PlaceThirdArrow:
                EnableTutorialPrompt(
                    "There's a wall ahead! Avoid crashing into it and get to the next leaf.",
                    false
                );
                swipeLeftHelper.SetActive(true);
                swipeDownHelper.SetActive(true);
                break;
            case TutorialState.PlaceFourthArrow:
                break;
            case TutorialState.MoveToThirdArrow:
                CloseTutorialPrompt();
                swipeLeftHelper.SetActive(false);
                swipeDownHelper.SetActive(false);
                break;
            case TutorialState.MoveToFourthArrow:
                break;
            case TutorialState.UntilFirstCrash:
                EnableTutorialPrompt("Nice! Planning is the key to success. Keep placing arrows!", true);
                break;
            case TutorialState.Complete:
                EnableTutorialPrompt("You crashed, but an extra life saved you!", true);
                break;
            default:
                break;
        }
    }

    void Start()
    {
        // load tutorial state from playerprefs
        int savedState = Int32.Parse(PlayerPrefs.GetString("tutorialState", "0"));
        _tutorialState = (TutorialState) savedState;
    }

    public void ResetToStart()
    {
        // When game exits in the middle of the tutorial, we need to disable
        // all the prompts and set back to starting stage
        tutorialState = TutorialState.Start;
        CloseTutorialPrompt();
        swipeLeftHelper.SetActive(false);
        swipeDownHelper.SetActive(false);
        swipeUpHelper.SetActive(false);
        swipeRightHelper.SetActive(false);
    }
}
