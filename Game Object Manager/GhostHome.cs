using System.Collections;
using UnityEngine;

public class GhostHome : GhostBehavior
{
    public Transform inside;
    public Transform outside;

    private void OnEnable()
    {
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        if (gameObject.activeInHierarchy) {
            StartCoroutine(ExitTransition());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
       // if (enabled && collision.gameObject.layer == LayerMask.NameToLayer("Obstacle")) {
       //     ghost.movement.SetDirection(-ghost.movement.direction);
       // }
    }
/*
    private IEnumerator ExitTransition()
    {
       //ghost.movement.SetDirection(Vector2.up, true);
        ghost.movement.rigidbody.isKinematic = true;
        ghost.movement.enabled = false;

        Vector3 position = transform.position;

        float duration = 0.5f;
        float elapsed = 0f;

        // Animate to the starting point
        while (elapsed < duration)
        {
            ghost.SetPosition(Vector3.Lerp(position, inside.position, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration)
        {
            ghost.SetPosition(Vector3.Lerp(inside.position, outside.position, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        ghost.SetPosition(outside.position); 
        //ghost.movement.SetDirection(new Vector2(Random.value < 0.5f ? -1f : 1f, 0f), true);
        ghost.movement.rigidbody.isKinematic = false;
        ghost.movement.enabled = true;
    }*/

    private IEnumerator ExitTransition()
    {
        
        ghost.movement.rigidbody.isKinematic = true;
        ghost.movement.enabled = false;

        Vector3 position = transform.position;

        float duration = 0.5f; // Duration for each segment of the transition
        float elapsed = 0f;

        // Animate to the starting point (inside position)
        while (elapsed < duration)
        {
            ghost.SetPosition(Vector3.Lerp(position, inside.position, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Delay for a certain amount of time inside the home
        float delayInside = 1.0f; // Delay inside the home for 1 second
        yield return new WaitForSeconds(delayInside);

        elapsed = 0f;

        // Animate from inside position to outside position
        while (elapsed < duration)
        {
            ghost.SetPosition(Vector3.Lerp(inside.position, outside.position, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        ghost.SetPosition(outside.position); 
        ghost.movement.rigidbody.isKinematic = false;
        ghost.movement.enabled = true;
    }


}