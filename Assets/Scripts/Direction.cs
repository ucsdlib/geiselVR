using System;
using UnityEngine;

public enum Direction
{
    Right,
    Left,
    Identity,
    Null,
}

static class DirectionVectorExtension
{
    public static Vector3 ToVector(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Right:
                return Vector3.right;
            case Direction.Left:
                return Vector3.left;
            case Direction.Null:
                return Vector3.zero;
            case Direction.Identity:
                return Vector3.one;
            default:
                throw new ArgumentOutOfRangeException("direction", direction, null);
        }
    }
}