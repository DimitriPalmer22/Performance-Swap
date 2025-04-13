using System;
using UnityEngine;
using UnityEngine.Events;

// Responsible for everything to do with our snake.
// Input
// Movement
// Collision

public class Snake : MonoBehaviour
{
    private const float COLLISION_CHECK_RADIUS = 0.40f;

    // Runtime
    [SerializeField] private bool hasWon = false;

    [Header("Movement"), SerializeField] private bool isRight;

    [Tooltip("The last direction inputted for this snake. Will move this direction on next update."), SerializeField]
    private Vector2 lastDir;

    [Header("Segments"), SerializeField] private Transform[] segments;

    [Header("Collision Checks"), SerializeField]
    private LayerMask obstacles;

    [SerializeField] private LayerMask snake;
    [SerializeField] private UnityEvent onCollision;

    private PlayerControls _pControls;
    private Vector3 _nextHeadPos;
    private Vector3 _nextSegmentPos;
    private Vector3 _segmentTemp;
    private int _segmentLength;

    private void Awake()
    {
        // Setup our snake input.
        _pControls = new PlayerControls();

        // Initialize our input.
        InitializeInput();

        // Get our segment length.
        _segmentLength = segments.Length;
    }

    private void InitializeInput()
    {
        // Setup different controls based on left and right snake.
        if (!isRight)
        {
            _pControls.LeftSnake.Up.started += ctx => lastDir = new Vector2(0, 1);
            _pControls.LeftSnake.Down.started += ctx => lastDir = new Vector2(0, -1);
            _pControls.LeftSnake.Left.started += ctx => lastDir = new Vector2(-1, 0);
            _pControls.LeftSnake.Right.started += ctx => lastDir = new Vector2(1, 0);
        }
        else
        {
            _pControls.RightSnake.Up.started += ctx => lastDir = new Vector2(0, 1);
            _pControls.RightSnake.Down.started += ctx => lastDir = new Vector2(0, -1);
            _pControls.RightSnake.Left.started += ctx => lastDir = new Vector2(-1, 0);
            _pControls.RightSnake.Right.started += ctx => lastDir = new Vector2(1, 0);
        }
    }

    #region Enable/Disable

    private void OnEnable()
    {
        _pControls.Enable();
    }

    private void OnDisable()
    {
        _pControls.Disable();
    }

    #endregion

    private void Update()
    {
        // Perform our win check.
        hasWon = WinCheck(transform.position + new Vector3(lastDir.x, lastDir.y, 0));
    }

    public void Move()
    {
        // Update our next head position.
        _nextHeadPos = transform.position + new Vector3(lastDir.x, lastDir.y, 0);

        // Perform a check for collisions.
        if (!CollisionCheck(_nextHeadPos))
            return;

        // Save our head position to update segments later.
        _nextSegmentPos = transform.position;

        // Move our head to this location.
        transform.position = _nextHeadPos;

        // Update our segments.
        UpdateSegments();
    }

    // Update the positions of our segments.
    public void UpdateSegments()
    {
        // Our first segment needs to follow the head.
        _segmentTemp = _nextSegmentPos;
        _nextSegmentPos = segments[0].position;
        segments[0].position = _segmentTemp;

        // Now for the rest of the segments.
        for (var i = 1; i < _segmentLength; i++)
        {
            _segmentTemp = _nextSegmentPos;
            _nextSegmentPos = segments[i].position;
            segments[i].position = _segmentTemp;
        }
    }

    // Perform a collision check at the next head location.
    // Returns true if we should move and false if we should not move.
    private bool CollisionCheck(Vector3 checkPos)
    {
        // If we hit any obstacles, do not move here!
        if (Physics2D.OverlapCircle(checkPos, COLLISION_CHECK_RADIUS, obstacles) != null)
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
        if (Physics2D.OverlapCircle(checkPos, .40f, snake) != null)
            return true;

        return false;
    }

    public bool GetHasWon() => hasWon;

    #region Debugging

    private void OnDrawGizmos()
    {
        // Draw the collision check radius.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(lastDir.x, lastDir.y, 0), COLLISION_CHECK_RADIUS);
    }

    #endregion
}