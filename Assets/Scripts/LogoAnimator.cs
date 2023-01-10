using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoAnimator : MonoBehaviour
{
    [SerializeField] private GameObject[] logoObjects;

    [SerializeField] private GameObject butterfly;


    private bool isPulsing = false;
    private bool isRotating = false;

    private WaitForSeconds pulse = new WaitForSeconds(0.1f);
    private WaitForSeconds pause = new WaitForSeconds(1f);


    void Update()
    {
        // simple animation for the logo.
        // pulse the caterpillar segments and wiggle the butterfly.
        if (!isPulsing)
        {
            StartCoroutine(PulseRoutine());
        }
        if (!isRotating)
        {
            StartCoroutine(RotateRoutine());
        }
    }

    private IEnumerator PulseRoutine()
    {
        isPulsing = true;

        foreach (GameObject obj in logoObjects)
        {
            obj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            yield return pulse;
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        yield return pause;

        isPulsing = false;
    }

    private IEnumerator RotateRoutine()
    {
        Quaternion rightPos = Quaternion.Euler(0f, 0f, 10);
        Quaternion leftPos = Quaternion.Euler(0f, 0f, -10f);

        isRotating = true;
        while (butterfly.transform.rotation != rightPos)
        {
            butterfly.transform.rotation = Quaternion.RotateTowards(
                butterfly.transform.rotation,
                rightPos,
                0.02f
            );
            yield return null;
        }
        while (butterfly.transform.rotation != leftPos)
        {
            butterfly.transform.rotation = Quaternion.RotateTowards(
                butterfly.transform.rotation,
                leftPos,
                0.02f
            );
            yield return null;
        }

        isRotating = false;
    }
}
