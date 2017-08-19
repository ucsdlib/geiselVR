using System;
using UnityEngine;

public enum Direction
{
    Right,
    Left,
    Identity
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
            case Direction.Identity:
                throw new ArgumentOutOfRangeException("direction", direction, null);
            default:
                throw new ArgumentOutOfRangeException("direction", direction, null);
        }
    }
}
