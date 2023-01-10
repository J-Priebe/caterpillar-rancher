using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeGestureAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer handSpriteRenderer;
    [SerializeField] private SpriteRenderer pulseCircleSpriteRenderer;

    [SerializeField] private Vector3Int swipeDirection;

    // private float animationDuration = 1.0f;

    private Vector3 startPos;
    private Vector3 endPos;

    private Vector3 startPulseScale;
    private Vector3 endPulseScale;
    private float pulseDuration = 0.6f;

    private bool isResetting = false;
    private float swipeSpeed = 2f;

    private WaitForSeconds pause = new WaitForSeconds(0.5f);

    private void Awake()
    {
        startPos = handSpriteRenderer.transform.position;
        // one unit long swipe path
        endPos = startPos + swipeDirection;
        startPulseScale = pulseCircleSpriteRenderer.transform.localScale;
        endPulseScale = startPulseScale * 1.5f;
    }

    // Start is called before the first frame update
    void Start()
    {
        // AnimateSwipe();
    }

    // Update is called once per frame
    void Update()
    {
        // move unless we're resetting position
        if (!isResetting)
        {
            Vector3 moveTo = Vector3.MoveTowards(
                handSpriteRenderer.transform.position,
                endPos,
                Time.deltaTime * swipeSpeed
            );
            handSpriteRenderer.transform.position = moveTo;
            if (moveTo == endPos)
            {
                ResetGesture();
            }
        }

        if (pulseCircleSpriteRenderer.transform.localScale == startPulseScale)
        {
            PulseCircle();
        }
        // Vector3.MoveTowards()
    }

    private void ResetGesture()
    {
        isResetting = true;
        StartCoroutine(ResetGestureRoutine());
    }

    private void PulseCircle()
    {
        StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        Vector3 currentScale = pulseCircleSpriteRenderer.transform.localScale;
        Vector3 distance =  endPulseScale - currentScale ;

        while ((currentScale - endPulseScale).magnitude > 0.05f)
        {
            currentScale += distance / pulseDuration * Time.deltaTime;
            pulseCircleSpriteRenderer.transform.localScale = currentScale;
            yield return null;
        }
        // yield return pause;
        pulseCircleSpriteRenderer.transform.localScale = startPulseScale;
    }

    private IEnumerator ResetGestureRoutine()
    {

        yield return pause;
        handSpriteRenderer.transform.position = startPos;
        yield return pause;

        isResetting = false;

    }

}
