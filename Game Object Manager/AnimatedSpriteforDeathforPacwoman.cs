using System;
using UnityEngine;

public class AnimatedSpriteforDeathforPacwoman : MonoBehaviour
{
    public Sprite[] sprites;
    public float animationTime = 0.25f;
    public SpriteRenderer spriteRenderer;

    private int animationFrame = 0;
    private float totalTime = 5f; // Total time before stopping the loop
    private float timeElapsed = 0f; // Time elapsed since the animation started

    // Custom looping frames
    private int loopStart = 9; // 10th sprite (zero-indexed)
    private int loopEnd = 11;  // 12th sprite (zero-indexed)

    // Event declaration
    public event Action OnDeathAnimationComplete;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Restart()
    {
        StopAnimation();
        ResetAnimation();
        StartAnimation();
    }

    public void StartAnimation()
    {
        animationFrame = 0;
        timeElapsed = 0f;
        spriteRenderer.enabled = true;
        InvokeRepeating(nameof(Advance), 0, animationTime);
    }

    public void StopAnimation()
    {
        CancelInvoke(nameof(Advance));
        OnAnimationComplete();
    }

    private void Advance()
    {
        if (timeElapsed >= totalTime)
        {
            StopAnimation();
            return;
        }

        if (animationFrame > loopEnd)
        {
            animationFrame = loopStart; // Loop back to start of loop range
        }

        spriteRenderer.sprite = sprites[animationFrame++];
        timeElapsed += animationTime; // Update the elapsed time
    }

    private void ResetAnimation()
    {
        animationFrame = 0;
        timeElapsed = 0f;
    }

    private void OnAnimationComplete()
    {
        OnDeathAnimationComplete?.Invoke();  // Trigger event when animation is complete
    }
}