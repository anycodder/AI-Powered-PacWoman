using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Pacwoman : MonoBehaviour
{
    public Movement movement {get; private set; }
    public AnimationOfPacwoman animationOfPacwoman;
    public AnimatedSpriteforDeathforPacwoman deathSequence;
    private SpriteRenderer spriteRenderer;
    private new Collider2D collider;
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public GameManager gameManager; 
    private Vector2 lastPosition;
    private Vector2 currentPosition;
    public Functions func {get; private set; }
      
    private void Awake()
    {
        movement = GetComponent<Movement>();
        gameManager = FindObjectOfType<GameManager>();
        animationOfPacwoman = GetComponent<AnimationOfPacwoman>();
        deathSequence = GetComponent<AnimatedSpriteforDeathforPacwoman>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        lastPosition = transform.position;
        currentPosition  = transform.position;
        func  = FindObjectOfType<Functions>();
    }

    private void Update()
    {
        if(func.buttonName== "User"){
            HandleMovement();
            UpdateRotation();  
            int score=PelletNotEatenPoints();
            gameManager.SetScore(score);   
        }

        currentPosition = transform.position;   
    }

    public int PelletNotEatenPoints()
    {
        int a=0;
        if (Vector2.Distance(currentPosition, lastPosition) > 1f)
        {
            foreach(Vector2 pelletPosition in gameManager.pelletPositions)
            {
                if (pelletPosition == rb.position)
                {
                    a = 1;
                }
            }
            lastPosition = transform.position; 

            if(a == 0)
            {
                return gameManager.score-1;
            }
        }
        return gameManager.score;
    }

    private void HandleMovement()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            movement.SetDirection(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            movement.SetDirection(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            movement.SetDirection(Vector2.left);
            transform.localScale = new Vector3(transform.localScale.x, -Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            movement.SetDirection(Vector2.right);
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }
    }


    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    public void ResetState()
    {
        animationOfPacwoman.ResumeAnimation();
        //animationOfDeath.StopAnimation();
        enabled = true;
        spriteRenderer.enabled = true;
        collider.enabled = true;
        deathSequence.enabled = false;
        movement.ResetState();
        gameObject.SetActive(true);
    }


    public void DeathSequence()
    {
        animationOfPacwoman.PauseAnimation();
       // animationOfDeath.StartAnimation();
        enabled = false;
        collider.enabled = false;
        movement.enabled = false;
        spriteRenderer.enabled = false;
        deathSequence.enabled = true;
        deathSequence.Restart();

    }

}

