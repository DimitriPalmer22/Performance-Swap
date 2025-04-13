using System;
using UnityEngine;

[Serializable]
public struct GridTile
{
    public TileType type;
    public Vector2Int position;
    public SnakeTileType snakeTileType;

    public enum TileType : byte
    {
        Empty,
        Wall,
    }

    public enum SnakeTileType : byte
    {
        None,
        LeftSnakeHead,
        LeftSnakeBody,
        RightSnakeHead,
        RightSnakeBody
    }
}