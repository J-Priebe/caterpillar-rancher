using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
public class ArrowManager : SingletonMonoBehaviour<ArrowManager>
{
    [SerializeField] private GameObject arrowPrefab;
    private Camera mainCamera;

    [SerializeField] private Tilemap tilemap;

    // may need to initialize upon sceneloaded event if we have multiple scenes
    [SerializeField] private Grid grid;

    // may be able to increase # of arrows with powerups
    [SerializeField] private int numArrows;

    private Vector3Int clickPosition;

    private List<GameObject> arrows;
    private List<Vector3> arrowDirections;

    private float dragThreshold = 0.5f;

    // TODO visual aid when placing over top caterpillar's body

    void Start()
    {
        mainCamera = Camera.main;
        arrows = new List<GameObject>();
        arrowDirections = new List<Vector3>();
    }

    void Update()
    {
        if (PauseManager.Instance.isPaused)
        {
            return;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            clickPosition = GetGridPositionForCursor();
        }

        if (Input.GetButtonUp("Fire1"))
        {
            // determine direction of mouse drag to set the arrow
            Vector3 releasePosition = GetGridPositionForCursor();
            // ignore beneath a drag threshold
            if (Vector3.Distance(clickPosition, releasePosition) < dragThreshold)
            {
                return;
            }

            Vector3Int direction = Utils.GetCardinalDirection(releasePosition - clickPosition);
            CreateArrow(direction);
        }
    }

    private Vector3Int GetGridPositionForCursor()
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z
            )
        );  // z is how far the objects are in front of the camera - camera is at -10 so objects are (-)-10 in front = 10
        return grid.WorldToCell(worldPosition);
    }

    private void CreateArrow(Vector3Int direction)
    {
        if (!tilemap.cellBounds.Contains(clickPosition))
        {
            return;
        }

        // Arrow placement is restricted during stages of the tutorial
        if (TutorialManager.Instance.CanPlaceArrow(clickPosition, direction))
        {
            Vector3 newArrowPos = grid.GetCellCenterWorld(clickPosition);
            // check if we're overwriting another arrow
            foreach (GameObject arr in arrows)
            {
                if (arr.transform.position == newArrowPos)
                {
                    DestroyArrow(arr);
                    break;
                }
            }

            float angle = Utils.GetCardinalAngle(direction);
            // instantiate at center of 1x1 grid tile
            GameObject newArrow = Instantiate(
                arrowPrefab,
                newArrowPos,
                Quaternion.Euler(0f, 0f, angle)
            );
            arrows.Add(
                newArrow
            );
            arrowDirections.Add(
                direction
            );

            // maintain at most numArrows, destroying the oldest one
            if (arrows.Count > numArrows)
            {
                DestroyArrow(0);
            }

            if (TutorialManager.Instance.IsArrowPlacementStage())
            {
                TutorialManager.Instance.AdvanceStage();
            }
        }
    }

    private void DestroyArrow(int index)
    {
        GameObject toDestroy = arrows[index];
        arrows.RemoveAt(index);
        arrowDirections.RemoveAt(index);
        Destroy(toDestroy);
    }

    public Vector3 GetArrowDirection(GameObject arrow)
    {
        return arrowDirections[
            arrows.IndexOf(arrow)
        ];
    }

    public void DestroyArrow(GameObject obj)
    {
        int index = arrows.IndexOf(obj);
        DestroyArrow(index);
    }

    public void DestroyAll()
    {
        foreach (GameObject arr in arrows)
        {
            Destroy(arr);
        }
        arrows.Clear();
        arrowDirections.Clear();
    }

}
