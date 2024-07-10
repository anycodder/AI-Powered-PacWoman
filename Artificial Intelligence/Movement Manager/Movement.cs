using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public float speed = 4.0f;
    public float speedMultiplier = 1.0f;
    public Vector2 initialDirection;
    public LayerMask obstacleLayer;
    
    public new Rigidbody2D rigidbody { get; private set; }
    public Vector2 direction { get; private set; }
    public Vector2 nextDirection { get; private set; }
    public Vector3 startingPosition { get; private set; }
    
    public List<Vector3> path = new List<Vector3>(); 
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
    }

    private void Start()
    {
        ResetState();
    }

    public void ReverseDirection()
    {
	    direction *= -1;
	    nextDirection = Vector2.zero;
    }

    public void ResetState()
    {
        speedMultiplier = 1.0f;
        direction = initialDirection;
        nextDirection = Vector2.zero;
        transform.position = startingPosition;
        rigidbody.isKinematic = false;
        enabled = true;
    }

    private void Update()
    {
        if (nextDirection != Vector2.zero) {
            SetDirection(nextDirection);
        }
    }

    private void FixedUpdate()
    {
        Vector2 position = rigidbody.position;
        Vector2 translation = direction * speed * speedMultiplier * Time.fixedDeltaTime;
        
        rigidbody.MovePosition(position + translation);
        path.Add(transform.position);
    }
    
    public bool Occupied(Vector2 direction)
    {
	    RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.75f, 0.0f, direction, 1f, this.obstacleLayer);
	    return hit.collider != null;
    }
    
    public bool Occupied2(Vector2 direction, Vector2 currentPos)
    {
	    RaycastHit2D hit = Physics2D.BoxCast(currentPos, Vector2.one * 0.75f, 0.0f, direction, 1f, this.obstacleLayer);
	    return hit.collider != null;
    }
    
    public void UndoLastMove()
    {
        direction *= -1;
        nextDirection = Vector2.zero;
    }

    public void SetDirection(Vector2 direction, bool forced = false)
	{
    	if (forced || !Occupied(direction))
    	{
    	    this.direction = direction;
    	    nextDirection = Vector2.zero;
    	}
    	else
    	{
        	nextDirection = direction;
    	}
	}

	public Vector2[] GetAvailableDirections(Vector3 currentPos)
	{
    	List<Vector2> availableDirections = new List<Vector2>();
    	Vector2[] cardinalDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    	foreach (Vector2 direction in cardinalDirections)
    	{
    	    if (!Occupied2(direction, currentPos))
    	    {
    	        availableDirections.Add(direction);
    	    }
    	    else
    	    {
     	    }
    	}

    	return availableDirections.ToArray();
	}

}