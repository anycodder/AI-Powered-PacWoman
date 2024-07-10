using UnityEngine;

[DefaultExecutionOrder(-10)]
public class Ghost : MonoBehaviour
{
    public float scaredTimer = 0f;
    public Movement movement { get; private set; }
    public GhostHome home { get; private set; }
    public GhostFrightened frightened { get; private set; }
    public Transform target;
    public int points = 200;
    public bool run = false;
    public GameManager gameManager;
    public bool isMinimaxControlled = false; // Add this flag to control the ghost state

    private void Awake()
    {
        movement = GetComponent<Movement>();
        home = GetComponent<GhostHome>();
        frightened = GetComponent<GhostFrightened>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        // Check if the ghost name contains "Minimax"
        if (name.Contains("Blinky-Minimax"))
        {
            isMinimaxControlled = true;
        }
        else
        {
            isMinimaxControlled = false;
        }
        ResetState();
    }

    public void ResetState()
    {
        gameObject.SetActive(true);
        movement.ResetState();
        frightened.Disable();

        // Check if the ghost is controlled by Minimax
        if (isMinimaxControlled)
        {
            home.Disable();
        }
        else
        {
            home.Enable();
            //SetPosition(home.inside.position);
        }
    }

    public void SetPosition(Vector3 position)
    {
        position.z = transform.position.z;
        transform.position = position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            if (frightened.enabled)
            {
                gameManager.GhostEaten(this);
            }
            else
            {
                gameManager.PacmanEaten();
            }
        }
    }
}
