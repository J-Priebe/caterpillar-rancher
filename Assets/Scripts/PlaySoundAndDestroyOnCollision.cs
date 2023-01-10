using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlaySoundAndDestroyOnCollision : MonoBehaviour
{
    private AudioSource audioSource;
    private SpriteRenderer sprite;
    private BoxCollider2D itemCollider2D;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>();
        itemCollider2D = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        // sometimes the food can spawn juuust as a segment passes over it.
        // if we don't check the collision is with the head, it could be
        // destroyed without triggering another one to be produced
        if (coll.gameObject.tag == "BugHead")
        {
            StartCoroutine(PlaySoundAndDestroy());
        }
    }

    private IEnumerator PlaySoundAndDestroy()
    {
        itemCollider2D.enabled = false;
        sprite.enabled = false;

        audioSource.Play();
        yield return new WaitUntil(() => audioSource.isPlaying == false);
        Destroy(gameObject);
    }

}
