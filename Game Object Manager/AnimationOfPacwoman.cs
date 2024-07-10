using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimationOfPacwoman : MonoBehaviour
{
    //public Sprite[] sprites = new Sprite[0];
    public float animationTime = 0.25f;
    public bool loop = true;

   // private SpriteRenderer spriteRenderer;
   // private int animationFrame;
    public Sprite[] sprites;
    public SpriteRenderer spriteRenderer { get; private set; } 
    public int animationFrame { get; private set; }
    private bool isPaused = false;

    public void Restart()
    {
        animationFrame = -1;

        Advance();
    }
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Start()
    {
        InvokeRepeating(nameof(Advance), animationTime, animationTime);
    }

    public void PauseAnimation()
    {
        isPaused = true;
    }

    public void ResumeAnimation()
    {
        isPaused = false;
    }

    private void Advance()
    {
        if (isPaused || !spriteRenderer.enabled) return;

        animationFrame++;
        if (animationFrame >= sprites.Length && loop) {
            animationFrame = 0;
        }
        if (animationFrame < sprites.Length) {
            spriteRenderer.sprite = sprites[animationFrame];
        }
    }
    
}
