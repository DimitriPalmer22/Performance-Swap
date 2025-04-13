using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

// Responsible for restarting our scene.

public class LevelManager : MonoBehaviour
{
    // Singleton pattern 
    public static LevelManager Instance { get; private set; }

    // Runtime
    [Header("Levels"), SerializeField] private Tilemap[] levels;
    [SerializeField] private Tilemap outsideWalls;

    private Tilemap _currentLevel;
    private GridTile[,] _levelGrid;

    private PlayerControls _pControls;

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        _pControls = new PlayerControls();
        _pControls.Menu.Restart.started += OnRestart;

        // Choose a random level.
        _currentLevel = levels[Random.Range(0, levels.Length)];
        _currentLevel.gameObject.SetActive(true);

        // Initialize the grid
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        // Initialize our grid.
        var xSize = _currentLevel.size.x;
        var ySize = _currentLevel.size.y;
        _levelGrid = new GridTile[xSize, ySize];

        var startX = -xSize / 2;
        var startY = -ySize / 2;

        // Go through the entire tilemap and initialize the level grid.
        for (var cY = startY; cY < startY + ySize; cY++)
        {
            for (var cX = startX; cX < startX + xSize; cX++)
            {
                // If there is a tile here, set the tile type
                var tileType = _currentLevel.HasTile(new Vector3Int(cX, cY, 0))
                    ? GridTile.TileType.Wall
                    : GridTile.TileType.Empty;

                // If there is no tile here, also check the outside walls
                if (tileType == GridTile.TileType.Empty)
                {
                    tileType = outsideWalls.HasTile(new Vector3Int(cX, cY, 0))
                        ? GridTile.TileType.Wall
                        : GridTile.TileType.Empty;
                }

                // Set the grid tile
                _levelGrid[cX - startX, cY - startY] = new GridTile
                {
                    type = tileType,
                    position = new Vector2Int(cX, cY),
                    snakeTileType = default
                };
            }
        }
    }

    private void OnRestart(InputAction.CallbackContext ctx)
    {
        // Reload the current scene
        SceneManager.LoadScene(0);
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

    private bool IsPositionInBounds(Vector2Int position)
    {
        var xSize = _currentLevel.size.x;
        var ySize = _currentLevel.size.y;

        // If the tile is out of bounds, return false
        if (position.x < 0 || position.x >= xSize ||
            position.y < 0 || position.y >= ySize)
        {
            return false;
        }

        return true;
    }

    public bool TryGetTile(Vector2Int position, out GridTile tile)
    {
        tile = default;

        // If the tile is out of bounds, return false
        if (!IsPositionInBounds(position))
            return false;

        // Set the tile to the tile at the position
        tile = _levelGrid[position.x, position.y];

        return true;
    }

    public bool TryGetPositionOnGrid(Vector2Int gridPosition, out Vector3 position)
    {
        position = default;

        // Try to get the tile at the position.
        // If we can't, return
        if (!TryGetTile(gridPosition, out var tile))
            return false;

        // Using the current tilemap, get the world position of the tile
        position = _currentLevel.GetCellCenterWorld(new Vector3Int(tile.position.x, tile.position.y, 0));

        return true;
    }

    public void SetSnakeInfo(Vector2Int gridPosition, bool isAny, bool isRight, bool isHead)
    {
        // Return if the position is out of bounds
        if (!IsPositionInBounds(gridPosition))
            return;
        
        if (!isAny)
        {
            _levelGrid[gridPosition.x, gridPosition.y].snakeTileType = GridTile.SnakeTileType.None;
            return;
        }

        if (isHead)
        {
            _levelGrid[gridPosition.x, gridPosition.y].snakeTileType = isRight
                ? GridTile.SnakeTileType.RightSnakeHead
                : GridTile.SnakeTileType.LeftSnakeHead;
        }
        else
        {
            _levelGrid[gridPosition.x, gridPosition.y].snakeTileType = isRight
                ? GridTile.SnakeTileType.RightSnakeBody
                : GridTile.SnakeTileType.LeftSnakeBody;
        }
    }

    private void OnDrawGizmos()
    {
        if (_currentLevel == null)
            return;

        // // Draw a red at 0, 0, 0 in the tilemap
        // Gizmos.color = Color.cyan;
        // Gizmos.DrawSphere(_currentLevel.GetCellCenterWorld(new Vector3Int(0, 0, 0)), 0.5f);

        // Draw spheres in every tile in the tilemap
        var xSize = _currentLevel.size.x;
        var ySize = _currentLevel.size.y;

        for (var cY = 0; cY < ySize; cY++)
        {
            for (var cX = 0; cX < xSize; cX++)
            {
                var currentTile = _levelGrid[cX, cY];

                var worldX = currentTile.position.x;
                var worldY = currentTile.position.y;

                switch (currentTile.type)
                {
                    case GridTile.TileType.Empty:
                        Gizmos.color = Color.green;
                        break;

                    case GridTile.TileType.Wall:
                        Gizmos.color = Color.red;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (currentTile.snakeTileType)
                {
                    case GridTile.SnakeTileType.None:
                        break;

                    case GridTile.SnakeTileType.LeftSnakeHead:
                        Gizmos.color = Color.magenta;
                        break;
                    
                    case GridTile.SnakeTileType.LeftSnakeBody:
                        Gizmos.color = Color.blue;
                        break;

                    case GridTile.SnakeTileType.RightSnakeHead:
                        Gizmos.color = Color.gray;
                        break;
                    
                    case GridTile.SnakeTileType.RightSnakeBody:
                        Gizmos.color = Color.yellow;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var cellCenter = _currentLevel.GetCellCenterWorld(new Vector3Int(worldX, worldY, 0));
                Gizmos.DrawSphere(cellCenter, 0.5f);
            }
        }
    }
}