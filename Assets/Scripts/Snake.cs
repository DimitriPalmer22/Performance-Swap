using System;
using System.Collections.Generic;
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

    [SerializeField] private Vector2Int initialPosition;

    [Tooltip("The last direction inputted for this snake. Will move this direction on next update."), SerializeField]
    private Vector2Int lastDir;

    [Header("Segments"), SerializeField] private Transform[] segments;

    [Header("Collision Checks"), SerializeField]
    private LayerMask obstacles;

    [SerializeField] private LayerMask snake;
    [SerializeField] private UnityEvent onCollision;

    private PlayerControls _pControls;

    private Vector3 _nextSegmentPos;
    private Vector3 _segmentTemp;
    private int _segmentLength;

    // Converting to the grid-based system
    private Vector2Int _currentGridPosition;
    private Vector2Int _nextHeadPos;
    private Vector2Int _previousHeadPos;
    private Vector2Int[] _segmentPositions;

    private void Awake()
    {
        // Setup our snake input.
        _pControls = new PlayerControls();

        // Initialize our input.
        InitializeInput();

        // Get our segment length.
        _segmentLength = segments.Length;

        // Initialize our segment positions.
        InitializeSegmentPositions();
    }

    private void InitializeSegmentPositions()
    {
        // Set the initial position of the snake.
        _currentGridPosition = initialPosition;

        _segmentPositions = new Vector2Int[_segmentLength];

        // Set the segment positions.
        for (var i = 0; i < _segmentLength; i++)
            _segmentPositions[i] = initialPosition;
    }

    private void Start()
    {
        // Move each of the segments to the correct position.
        LevelManager.Instance.TryGetPositionOnGrid(initialPosition, out var worldPos);

        foreach (var segment in segments)
            segment.position = worldPos;

        // Update our head position.
        transform.position = worldPos;
    }

    private void InitializeInput()
    {
        // Setup different controls based on left and right snake.
        if (!isRight)
        {
            _pControls.LeftSnake.Up.performed += _ => SetMovementDirection(new Vector2Int(0, 1));
            _pControls.LeftSnake.Down.performed += _ => SetMovementDirection(new Vector2Int(0, -1));
            _pControls.LeftSnake.Left.performed += _ => SetMovementDirection(new Vector2Int(-1, 0));
            _pControls.LeftSnake.Right.performed += _ => SetMovementDirection(new Vector2Int(1, 0));
        }
        else
        {
            _pControls.RightSnake.Up.started += _ => SetMovementDirection(new Vector2Int(0, 1));
            _pControls.RightSnake.Down.started += _ => SetMovementDirection(new Vector2Int(0, -1));
            _pControls.RightSnake.Left.started += _ => SetMovementDirection(new Vector2Int(-1, 0));
            _pControls.RightSnake.Right.started += _ => SetMovementDirection(new Vector2Int(1, 0));
        }
    }

    private void SetMovementDirection(Vector2Int direction)
    {
        // Return if the game manager says the game is over
        if (GameManager.Instance.IsGameOver)
            return;

        // Set the last direction to the new direction.
        lastDir = direction;
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

    public void Move()
    {
        // Update our next head position.
        _nextHeadPos = _currentGridPosition + lastDir;

        // Perform a check for collisions.
        if (!CollisionCheck(_nextHeadPos))
        {
            onCollision.Invoke();
            return;
        }

        // Save our head position to update segments later.
        _previousHeadPos = _currentGridPosition;
        _nextSegmentPos = transform.position;

        // Move our head to this new location.
        MoveSegment(transform, _nextHeadPos);
        _currentGridPosition = _nextHeadPos;
        LevelManager.Instance.SetSnakeInfo(_currentGridPosition, true, isRight, true);

        // Update our segments.
        UpdateSegments();

        // Perform our win check.
        // hasWon = WinCheck(transform.position + new Vector3(lastDir.x, lastDir.y, 0));
        hasWon = WinCheck(_currentGridPosition + lastDir);
    }

    private void MoveSegment(Transform segment, Vector2Int nextPos)
    {
        // Move the segment to the new position
        if (LevelManager.Instance.TryGetPositionOnGrid(nextPos, out var worldPos))
            segment.position = worldPos;
    }

    // Update the positions of our segments.
    private void UpdateSegments()
    {
        // First clear the snake info on the grid for all segments
        foreach (var position in _segmentPositions)
            LevelManager.Instance.SetSnakeInfo(position, false, false, false);

        // Our first segment needs to follow the head.
        var nextPos = _previousHeadPos;
        MoveSegment(segments[0], nextPos);
        _segmentPositions[0] = _previousHeadPos;

        // Now for the rest of the segments.
        // Go through each segment and move it to the next position.
        // Go through backwards so we don't overwrite the positions.
        for (var i = _segmentPositions.Length - 1; i >= 1; i--)
        {
            _segmentPositions[i] = _segmentPositions[i - 1];
            MoveSegment(segments[i], _segmentPositions[i]);
        }

        // Lastly, update the snake info on the grid for all segments
        foreach (var position in _segmentPositions)
            LevelManager.Instance.SetSnakeInfo(position, true, isRight, false);
    }

    private bool CollisionCheck(Vector2Int checkPos)
    {
        // If we hit any obstacles, do not move here!
        if (LevelManager.Instance.TryGetTile(checkPos, out var tile))
        {
            if (tile.type == GridTile.TileType.Wall)
                return false;

            if (tile.snakeTileType == GridTile.SnakeTileType.LeftSnakeBody ||
                tile.snakeTileType == GridTile.SnakeTileType.RightSnakeBody)
                return false;
        }

        // If we pass the checks, return true and move.
        return true;
    }

    // Perform a win check at the next head forward location.
    // Returns true if we are touching the other snakes head.
    private bool WinCheck(Vector2Int checkPos)
    {
        // If we hit the other snakes head, we have won!
        if (LevelManager.Instance.TryGetTile(checkPos, out var tile))
        {
            return (tile.snakeTileType == GridTile.SnakeTileType.RightSnakeHead && !isRight) ||
                   (tile.snakeTileType == GridTile.SnakeTileType.LeftSnakeHead && isRight);
        }

        return false;
    }

    public bool GetHasWon() => hasWon;

    #region Debugging

    private void OnDrawGizmos()
    {
        // Draw the collision check radius.
        Gizmos.color = Color.red;

        var nextPos = _currentGridPosition + lastDir;

        // Get the world position of the next head position.
        if (LevelManager.Instance?.TryGetPositionOnGrid(nextPos, out var worldPos) ?? false)
            Gizmos.DrawWireSphere(worldPos, COLLISION_CHECK_RADIUS);
    }

    #endregion
}