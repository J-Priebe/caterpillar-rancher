using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FoodManager : SingletonMonoBehaviour<FoodManager>
{
    // may need to initialize upon sceneloaded event if we have multiple scenes
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private GameObject superFoodPrefab;

    private float superFoodSpawnChance = 0.3f;
    private float stepsRemainingForSuperFood;

    // for x2 speedy score bonus
    private int stepsRemainingForBonusScore;

    private Vector3Int GetSafeSpawnLocation()
    {
        HashSet<Coords> unoccupied = Caterpillar.Instance.GetUnOccupiedTiles();

        Coords coord = unoccupied.ElementAt(
            Random.Range(0, unoccupied.Count)
        );
        return new Vector3Int(coord.X, coord.Y);
    }

    private void MaybeSpawnSuperFood(Vector3 caterpillarPos, bool isTutorial)
    {
        // only spawn on random chance and if there isn't already a superfood
        if (
            (!isTutorial) &&
            (Random.Range(0f, 1f) > superFoodSpawnChance || stepsRemainingForSuperFood > 0)
        )
        {
            return;
        }
        Vector3 superSpawnLoc;
        if (isTutorial)
        {
            superSpawnLoc = grid.GetCellCenterWorld(
                Constants.TUTORIAL_SUPERFOOD_SPAWN_POSITION
            );
        }
        else
        {
            // could spawn on top of food but thats OK
            superSpawnLoc = grid.GetCellCenterWorld(
                GetSafeSpawnLocation()
            );
        }

        int dy = (int)Mathf.Abs(caterpillarPos.y - superSpawnLoc.y);
        int dx = (int)Mathf.Abs(caterpillarPos.x - superSpawnLoc.x);

        int dist = dy + dx;
        float multiplier = dist < 10 ? 2.5f : 2f;
        stepsRemainingForSuperFood = Mathf.RoundToInt((dy + dx) * multiplier);
        Instantiate(
            superFoodPrefab,
            superSpawnLoc,
            Quaternion.identity
        );

    }

    public void SpawnNewFood()
    {
        Vector3 spawnLoc;

        if (TutorialManager.Instance.tutorialState == TutorialState.Start)
        {
            spawnLoc = grid.GetCellCenterWorld(
                Constants.TUTORIAL_FIRST_FOOD_SPAWN_POSITION
            );
        }
        else if (TutorialManager.Instance.tutorialState == TutorialState.MoveToLeaf)
        {
            spawnLoc = grid.GetCellCenterWorld(
                Constants.TUTORIAL_SECOND_FOOD_SPAWN_POSITION
            );
        }
        else
        {
            // choose a safe location, within bounds
            spawnLoc = grid.GetCellCenterWorld(
                GetSafeSpawnLocation()
            );
        }

        // Debug.Log("spawning new food at " + spawnLoc);
        Vector3 caterpillarPos = Caterpillar.Instance.GetHeadPosition();

        int dy = (int)Mathf.Abs(caterpillarPos.y - spawnLoc.y);
        int dx = (int)Mathf.Abs(caterpillarPos.x - spawnLoc.x);

        int dist = dy + dx;
        stepsRemainingForBonusScore = Mathf.RoundToInt((dy + dx) * 1.75f);

        Instantiate(
            foodPrefab,
            spawnLoc,
            Quaternion.identity
        );

        if(TutorialManager.Instance.tutorialState == TutorialState.Start)
        {
            MaybeSpawnSuperFood(caterpillarPos, true);
            TutorialManager.Instance.AdvanceStage();

        }
        // don't spawn superfood on second stage of tutorial
        else if (TutorialManager.Instance.tutorialState >= TutorialState.UntilFirstCrash)
        {
            MaybeSpawnSuperFood(caterpillarPos, false);
        }

    }

    public void DestroyFood()
    {
        foreach (GameObject fo in GameObject.FindGameObjectsWithTag("Food"))
        {
            Destroy(fo);
        }
        foreach (GameObject fo in GameObject.FindGameObjectsWithTag("SuperFood"))
        {
            Destroy(fo);
        }
    }

    public void DecrementSteps()
    {
        stepsRemainingForBonusScore -= 1;
        stepsRemainingForSuperFood -= 1;
        // Debug.Log("bonus score remaining:" + stepsRemainingForBonusScore);

        // superfood disappears if not collected quickly
        if (stepsRemainingForSuperFood <= 0)
        {
            foreach (GameObject fo in GameObject.FindGameObjectsWithTag("SuperFood"))
            {
                Destroy(fo);
            }
        }

    }

    public bool FoodScoreBonusActive()
    {
        return stepsRemainingForBonusScore > 0;
    }
}
