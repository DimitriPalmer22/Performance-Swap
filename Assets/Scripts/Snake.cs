using UnityEngine;
using UnityEngine.Events;

// Responsible for everything to do with our snake.
// Input
// Movement
// Collision

public class Snake : MonoBehaviour
{
    // Runtime
    PlayerControls pControls;
    [SerializeField] bool hasWon = false;
    private Vector3 nextHeadPos;
    private Vector3 nextSegmentPos;
    private Vector3 segmentTemp;
    private int segmentLength;

    [Header("Movement")]
    [SerializeField] bool isRight;
    [Tooltip("The last direction inputted for this snake. Will move this direction on next update.")]
    [SerializeField] Vector2 lastDir;

    [Header("Segments")]
    [SerializeField] Transform[] segments;

    [Header("Collision Checks")]
    [SerializeField] LayerMask obstacles;
    [SerializeField] LayerMask snake;
    [SerializeField] UnityEvent onCollision;


    void Awake()
    {
        // Setup our snake input.
        pControls = new PlayerControls();

        // Setup different controls based on left and right snake.
        if(!isRight)
        {
            pControls.LeftSnake.Up.started += ctx => lastDir = new Vector2(0, 1);
            pControls.LeftSnake.Down.started += ctx => lastDir = new Vector2(0, -1);
            pControls.LeftSnake.Left.started += ctx => lastDir = new Vector2(-1, 0);
            pControls.LeftSnake.Right.started += ctx => lastDir = new Vector2(1, 0);
        }
        else
        {
            pControls.RightSnake.Up.started += ctx => lastDir = new Vector2(0, 1);
            pControls.RightSnake.Down.started += ctx => lastDir = new Vector2(0, -1);
            pControls.RightSnake.Left.started += ctx => lastDir = new Vector2(-1, 0);
            pControls.RightSnake.Right.started += ctx => lastDir = new Vector2(1, 0);
        }

        // Get our segment length.
        segmentLength = segments.Length;
    }

    void Update()
    {
        // Perform our win check.
        hasWon = WinCheck(transform.position + new Vector3(lastDir.x, lastDir.y, 0));
    }

    public void Move()
    {
        // Update our next head position.
        nextHeadPos = transform.position + new Vector3(lastDir.x, lastDir.y, 0);

        // Perform a check for collisions.
        if(!CollisionCheck(nextHeadPos))
        {
            return;
        }

        // Save our head position to update segments later.
        nextSegmentPos = transform.position;

        // Move our head to this location.
        transform.position = nextHeadPos;

        // Update our segments.
        UpdateSegments();

        // Perform our win check.
        //hasWon = WinCheck(transform.position + new Vector3(lastDir.x, lastDir.y, 0));
    }

    // Update the positions of our segments.
    public void UpdateSegments()
    {
        // Our first segment needs to follow the head.
        segmentTemp = nextSegmentPos;
        nextSegmentPos = segments[0].position;
        segments[0].position = segmentTemp;

        // Now for the rest of the segments.
        for(int i=1; i<segmentLength; i++)
        {
            segmentTemp = nextSegmentPos;
            nextSegmentPos = segments[i].position;
            segments[i].position = segmentTemp;
        }
    }

    // Perform a collision check at the next head location.
    // Returns true if we should move and false if we should not move.
    private bool CollisionCheck(Vector3 checkPos)
    {
        // If we hit any obstacles, do not move here!
        if(Physics2D.OverlapCircle(checkPos, .40f, obstacles) != null)
        {
            onCollision.Invoke();
            return false;
        }

        // If we pass the checks, return true and move.
        return true;
    }

    // Perform a win check at the next head forward location.
    // Returns true if we are touching the other snakes head.
    private bool WinCheck(Vector3 checkPos)
    {
        // If we hit the other snakes head, we have won!
        if(Physics2D.OverlapCircle(checkPos, .40f, snake) != null)
        {
            return true;
        }

        return false;
    }

    public bool GetHasWon()
    {
        return hasWon;
    }

#region Enable/Disable
    void OnEnable()
    {
        pControls.Enable();
    }

    void OnDisable()
    {
        pControls.Disable();
    }
#endregion
}
