using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostEyes : MonoBehaviour
{
    public Sprite up;
    public Sprite down;
    public Sprite left;
    public Sprite right;
    private SpriteRenderer spriteRenderer;
    public Functions func { get; private set; }


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        func  = FindObjectOfType<Functions>();
    }

    private void Update()
    { 
        string objeAdi = gameObject.name;
        switch (objeAdi)
        {
            case "Eyes0":
                Vector2 move1 = func.ghostsLastMoves[0];
                MoveEyes(move1);            
                break;

            case "Eyes1":
                Vector2 move2 = func.ghostsLastMoves[1];
                MoveEyes(move2);           
                break;  

            case "Eyes2":
                Vector2 move3 = func.ghostsLastMoves[2];
                MoveEyes(move3);
                break;

            case "Eyes3":
                Vector2 move4 = func.ghostsLastMoves[3];
                MoveEyes(move4);           
                break;

            default:
                break;
        }
    }

    public void MoveEyes(Vector2 bestMove)
    {
        if (bestMove.x == -1)
        {
            spriteRenderer.sprite = left;
        }
        else if (bestMove.x == 1)
        {
            spriteRenderer.sprite = right;
        }
        else if (bestMove.y == 1)
        {
            spriteRenderer.sprite = up;
        }
        else if (bestMove.y == -1)
        {
            spriteRenderer.sprite = down;
        }
    }
}
