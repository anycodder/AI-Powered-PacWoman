using UnityEngine;

public class GhostFrightened : GhostBehavior
{
    public SpriteRenderer body;
    public SpriteRenderer eyes;
    public SpriteRenderer blue;
    public SpriteRenderer white;

    public bool eaten = false;

    private void Update()
    {
        if (ghost.scaredTimer > 0)
        {
            ghost.scaredTimer -= Time.deltaTime;  // Countdown the scared timer
            if (ghost.scaredTimer <= 0)
            {
                if (!eaten)  // Only disable if not already eaten
                {
                    ghost.run = false;
                    Disable();
                }
            }
        }
    }

    public override void Enable(float duration)
    {
        base.Enable(duration);
        ghost.scaredTimer = duration;
        body.enabled = false;
        eyes.enabled = false;
        blue.enabled = true;
        white.enabled = false;

        Invoke(nameof(Flash), duration / 2f);
    }

    public override void Disable()
    {
        base.Disable();

        body.enabled = true;
        eyes.enabled = true;
        blue.enabled = false;
        white.enabled = false;
    }

    private void Eaten()
    {
        ghost.scaredTimer = 0;
        eaten = true;
        ghost.SetPosition(ghost.home.inside.position);
        ghost.home.Enable(duration);
        // Disable visuals when eaten
        body.enabled = false;
        eyes.enabled = false;
        blue.enabled = false;
        white.enabled = false;

        // Debug the ghost's name and position
        Debug.Log($"Ghost '{ghost.name}' has been eaten and moved to inside position at {ghost.home.inside.position}");
    }

    private void Flash()
    {
        ghost.run = true;
        if (!eaten)
        {
            blue.enabled = false;
            white.enabled = true;
            white.GetComponent<AnimationOfPacwoman>().Restart();
        }
    }

    private void OnEnable()
    {
        blue.GetComponent<AnimationOfPacwoman>().Restart();
        ghost.movement.speedMultiplier = 0.5f;
        eaten = false;
    }

    private void OnDisable()
    {
        ghost.movement.speedMultiplier = 1f;
        eaten = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            Debug.Log("ok");
            if (enabled)
            {
                Debug.Log("o2");
                Eaten();
            }
        }
    }
}
