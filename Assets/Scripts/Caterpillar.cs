using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(AudioSource))]
public class Caterpillar : SingletonMonoBehaviour<Caterpillar>
{
    private float baseSpeed = 2.2f;

    private float speed;

    [SerializeField]
    public Transform head;

    [SerializeField]
    private Transform body;

    private List<Transform> segments;
    private List<Vector3> segmentTargets;

    private Vector3 headTarget;
    private Vector3 headDirection;

    private Vector3 tailLastTarget;

    [SerializeField]
    private GameObject segmentPrefab;

    [SerializeField]
    private Grid grid;

    private WaitForSecondsRealtime segmentDestroyInterval;
    private WaitForSecondsRealtime oneUpPause;

    private bool isWinAnimationActive = false;

    // track UNoccupied tiles, because we need to look them up
    // with a single Random() call
    private HashSet<Coords> unOccupiedTiles;

    private AudioSource crashAudioSource;

    protected override void Awake()
    {
        base.Awake();
        crashAudioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        AdsInitializer.Instance.HideBannerAd();

        segments = new List<Transform>();
        segmentTargets = new List<Vector3>();

        segmentDestroyInterval = new WaitForSecondsRealtime(0.1f);
        oneUpPause = new WaitForSecondsRealtime(1f);
    }

    void Update()
    {
        if (TutorialManager.Instance.isWaitingForTutorialPrompt)
        {
            return;
        }
        if (IsHeadAtTarget() & !isWinAnimationActive)
        {
            UpdateHeadTarget();
            UpdateSegmentTargets();
        }
        MoveSegments();
    }

    // NOTE: Ensure the colider on food is larger than the arrow,
    // because we need to add the new segment BEFORE changing direction,
    // since segment spawn position is dependent upon it.
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Food"))
        {
            AddSegment();
            // pickup has to happen first to calculate bonus score
            LevelManager.Instance.HandleFoodPickup(segments.Count);
            FoodManager.Instance.SpawnNewFood();

            if (TutorialManager.Instance.tutorialState == TutorialState.MoveToLeaf)
            {
                TutorialManager.Instance.AdvanceStage();
            }
        }
        // superfood doesn't spawn a new food
        else if (coll.gameObject.CompareTag("SuperFood"))
        {
            if (TutorialManager.Instance.tutorialState == TutorialState.MoveToApple)
            {
                TutorialManager.Instance.AdvanceStage();
            }
            LevelManager.Instance.HandleSuperFoodPickup(segments.Count);
        }
        // set the head's new direction and destroy the arrow
        else if (
            coll.gameObject.CompareTag("Arrow") && coll.gameObject.transform.position == headTarget
        )
        {
            headDirection = ArrowManager.Instance.GetArrowDirection(coll.gameObject);
            // we could maybe delay this by a frame or two so the arrow is visible until the
            // caterpillar actually overlaps it
            ArrowManager.Instance.DestroyArrow(coll.gameObject);
            if (
                TutorialManager.Instance.tutorialState == TutorialState.MoveToThirdArrow
                || TutorialManager.Instance.tutorialState == TutorialState.MoveToFourthArrow
            )
            {
                TutorialManager.Instance.AdvanceStage();
            }
        }
    }

    // ! avoid instant re-collision in case where snek hits two segments in the middle
    // might want to disable player input briefly during this sequence
    void OnCollisionEnter2D(Collision2D coll)
    {
        Handheld.Vibrate();
        crashAudioSource.Play();
        if (LevelManager.Instance.oneUpsRemaining > 0)
        {
            if (TutorialManager.Instance.tutorialState == TutorialState.UntilFirstCrash)
            {
                TutorialManager.Instance.AdvanceStage();
            }
            ArrowManager.Instance.DestroyAll();
            UseOneUp();
        }
        else
        {
            LevelManager.Instance.HandleGameOver();
        }
    }

    bool IsHeadAtTarget()
    {
        return head.position == headTarget;
    }

    void UpdateHeadTarget()
    {
        // the head always continues in its current direction,
        // unless we've explicitly set a change in direction by hitting an arrow
        headTarget = head.position + headDirection;
        head.rotation = Quaternion.Euler(0, 0, Utils.GetCardinalAngle(headDirection));

        // set new target as occupied, if it's in bounds
        Vector3Int headTargetCell = grid.WorldToCell(headTarget);
        if (
            0 <= headTargetCell.x
            && headTargetCell.x < Constants.tilesX
            && 0 <= headTargetCell.y
            && headTargetCell.y < Constants.tilesY
        )
        {
            unOccupiedTiles.Remove(new Coords(headTargetCell));
        }

        // if the head is the tail
        if (segments.Count == 0)
        {
            // set old last tail target as unoccupied
            // need to check if it's vector.zero?
            Vector3Int cell = grid.WorldToCell(tailLastTarget);
            unOccupiedTiles.Add(new Coords(cell));
            tailLastTarget = head.position;
        }

        // we've moved a unit,
        // decrement the time we have left to collect next food bonus
        FoodManager.Instance.DecrementSteps();

        // save on some checks when we're advanced far enough in the tutorial
        if (
            TutorialManager.Instance.tutorialState <= TutorialState.WallAhead
            && (
                (
                    TutorialManager.Instance.tutorialState == TutorialState.MoveToFirstArrow
                    && headTargetCell.x == Constants.TUTORIAL_FIRST_PAUSE_HEAD_POSITION.X
                    && headTargetCell.y == Constants.TUTORIAL_FIRST_PAUSE_HEAD_POSITION.Y
                )
                || (
                    TutorialManager.Instance.tutorialState == TutorialState.MoveToSecondArrow
                    && headTargetCell.x == Constants.TUTORIAL_SECOND_PAUSE_HEAD_POSITION.X
                    && headTargetCell.y == Constants.TUTORIAL_SECOND_PAUSE_HEAD_POSITION.Y
                )
                || (
                    TutorialManager.Instance.tutorialState == TutorialState.WallAhead
                    && headTargetCell.x == Constants.TUTORIAL_WALL_AHEAD_PROMPT_POSITION.X
                    && headTargetCell.y == Constants.TUTORIAL_WALL_AHEAD_PROMPT_POSITION.Y
                )
            )
        )
        {
            TutorialManager.Instance.AdvanceStage();
        }
    }

    void UpdateSegmentTargets()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (i == segments.Count - 1)
            {
                // set old last tail target as unoccupied
                Vector3Int cell = grid.WorldToCell(tailLastTarget);

                // a thought.. do we really need to prevent spawning under the body? or just the head?

                // when caterpillar first spawns, some segments are OOB.. don't try to mark them as occupied
                if (
                    cell.x < Constants.tilesX
                    && cell.y < Constants.tilesY
                    && cell.x >= 0
                    && cell.y >= 0
                )
                {
                    unOccupiedTiles.Add(new Coords(cell));
                }
                tailLastTarget = segmentTargets[i];
            }
            segmentTargets[i] = GetNextSegmentTarget(i);
        }
    }

    void MoveSegments()
    {
        // stop all movement while we're waiting for next arrow to be placed
        if (TutorialManager.Instance.IsArrowPlacementStage())
        {
            return;
        }

        Vector3 moveTo = Vector3.MoveTowards(head.position, headTarget, Time.deltaTime * speed);

        head.position = moveTo;

        for (int i = 0; i < segments.Count; i++)
        {
            // during win animation they all collect at the head
            if (segments[i].position == headTarget)
            {
                continue;
            }

            moveTo = Vector3.MoveTowards(
                segments[i].position,
                segmentTargets[i],
                Time.deltaTime * speed
            );
            segments[i].position = moveTo;
        }
    }

    Vector3 GetNextSegmentTarget(int i)
    {
        return segments[i].position
            + Utils.GetCardinalDirection(
                (i == 0 ? head : segments[i - 1]).position - segments[i].position
            );
    }

    Vector3 GetSegmentSpawnPosition()
    {
        Transform tail = segments.Count > 0 ? segments[segments.Count - 1] : head;
        // direction is wherever it's headed
        Vector3 direction =
            segments.Count > 0
                ? Utils.GetCardinalDirection(
                    segmentTargets[segments.Count - 1] - segments[segments.Count - 1].position
                )
                : headDirection;

        // spawn opposite of tail's direction, which by definition is where it was last
        // and thus safe (within bounds and not overlapping with itself)
        // as long as we add the segment before changing the tail's direction.
        // This assumption holds because the head collides with food before it reaches its
        // target (is perfectly overlapping with its target cell). Therefore all segments
        // will be in transit from their last safe position.
        return tail.position - direction;
    }

    private void AddSegment(Vector3 spawnPos)
    {
        GameObject newSegment = Instantiate(
            segmentPrefab,
            spawnPos,
            Quaternion.Euler(0f, 0f, 0f),
            body
        );
        Vector3 nextTarget = tailLastTarget;
        segmentTargets.Add(nextTarget);
        segments.Add(newSegment.transform);
    }

    private void AddSegment()
    {
        Vector3 spawnPos = GetSegmentSpawnPosition();
        AddSegment(spawnPos);
    }

    private void UseOneUp()
    {
        StartCoroutine(UseOneUpCoroutine());
    }

    private IEnumerator UseOneUpCoroutine()
    {
        UIManager.Instance.SetPauseButtonActive(false);
        PauseManager.Instance.PauseGame();
        int currentSegmentsLength = segments.Count;

        yield return new WaitUntil(
            () => TutorialManager.Instance.isWaitingForTutorialPrompt == false
        );

        // fade, reset caterpillar, unfade, consume
        yield return oneUpPause;
        yield return StartCoroutine(SceneControllerManager.Instance.Fade(1f));
        ResetCaterpillarPosition(currentSegmentsLength);
        yield return StartCoroutine(SceneControllerManager.Instance.Fade(0f));

        yield return oneUpPause;
        UIManager.Instance.ShowOneUpDecrementBanner(GetHeadPosition() + Vector3.up);
        LevelManager.Instance.ConsumeOneUp(segments.Count);
        yield return oneUpPause;
        UIManager.Instance.HideOneUpDecrementBanner();

        UIManager.Instance.SetPauseButtonActive(true);
        PauseManager.Instance.ResumeGame();
    }

    public void StartWinAnimation(Action callback)
    {
        StartCoroutine(AnimateWinRoutine(callback));
    }

    private IEnumerator AnimateWinRoutine(Action callback)
    {
        isWinAnimationActive = true;

        // hide caterpillar when butterfly appears
        SpriteRenderer spriteRenderer = null;
        float currentAlpha = 1f;
        while (currentAlpha > 0.01f)
        {
            spriteRenderer = head.gameObject.GetComponent<SpriteRenderer>();
            currentAlpha = spriteRenderer.color.a - Time.deltaTime;
            spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);

            foreach (Transform segment in body)
            {
                spriteRenderer = segment.gameObject.GetComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            }

            yield return null;
        }

        head.gameObject.SetActive(false);
        body.gameObject.SetActive(false);
        callback();
    }

    public float GetSpeedForLevel(int level)
    {
        // +0.2f every 2 levels before 16
        float newSpeed = baseSpeed + 0.2f * (Mathf.Min(level - 1, 16) / 2);

        // +0.15f every 2 levels until 30 (when food is maxed)
        if (level > 16)
        {
            newSpeed += 0.15f * ((Mathf.Min(level - 1, 30) - 16) / 2);
        }

        // 0.1f every level thereafter
        if (level > 30)
        {
            newSpeed += 0.1f * (level - 1 - 30);
        }
        return newSpeed;
    }

    public int GeStartingSegmentsForLevel(int level)
    {
        if (level < 2)
        {
            return 2;
        }
        return LevelManager.Instance.GetFoodToWin(level) / 2;
        // add 2 or 3 segments every second level so you're
        // always adding half the required food
    }

    // destroy existing segments, move head to zero
    private void ResetCaterpillarPosition(int numSegments)
    {
        while (segments.Count > 0)
        {
            Transform segment = segments[segments.Count - 1];
            segments.RemoveAt(segments.Count - 1);
            Destroy(segment.gameObject);
        }

        segmentTargets.Clear();

        unOccupiedTiles = new HashSet<Coords>();
        for (int i = 0; i < Constants.tilesX; i++)
        {
            for (int j = 0; j < Constants.tilesY; j++)
            {
                // starting corner is always occupied
                if (!(i == 0 && j == 0))
                {
                    unOccupiedTiles.Add(new Coords(i, j));
                }
            }
        }

        head.position = grid.GetCellCenterWorld(Vector3Int.zero);
        headDirection = Vector3.up;
        head.rotation = Quaternion.Euler(0, 0, Utils.GetCardinalAngle(headDirection));
        headTarget = head.position + headDirection;

        tailLastTarget = head.position;
        Vector3 newSegmentPos = head.position + Vector3.down; // 1 below
        for (int i = 0; i < numSegments; i++)
        {
            AddSegment(newSegmentPos);
            newSegmentPos += Vector3.down;
            tailLastTarget += Vector3.down;
        }

        LevelManager.Instance.SetFoodCollected(segments.Count);
    }

    // when we win a level we reset its position and segments
    // and increase the speed
    public void ResetForNextLevel(int newLevel)
    {
        head.gameObject.SetActive(true);
        body.gameObject.SetActive(true);
        // add segments every time caterpillar gets hungrier. cut out the boring part and get to
        // the part of the game where you're managing a long boi
        // we should make the tail look different so you know when it comes out?
        int numSegments = GeStartingSegmentsForLevel(newLevel);
        ResetCaterpillarPosition(numSegments);
        speed = GetSpeedForLevel(newLevel);
    }

    public HashSet<Coords> GetUnOccupiedTiles()
    {
        return unOccupiedTiles;
    }

    public Vector3 GetHeadPosition()
    {
        return head.transform.position;
    }
}
