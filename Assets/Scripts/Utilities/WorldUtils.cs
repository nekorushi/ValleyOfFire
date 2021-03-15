using System;
using UnityEngine;

public static class WorldUtils
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public static float AngleBetweenPoints(Vector3 originPos, Vector3 targetPos)
    {
        originPos.z = 0;
        targetPos.z = 0;

        float angle = Vector3.Angle(Vector3.up, targetPos - originPos);
        bool isCounterClockwise = Vector3.Cross(Vector3.up, targetPos - originPos).z > 0;

        return isCounterClockwise ? -angle : angle;
    }

    public static Direction DirectionToTarget(Vector3 originPos, Vector3 targetPos)
    {
        float angleToTarget = AngleBetweenPoints(originPos, targetPos);


        if (Mathf.Abs(angleToTarget) <= 45)
        {
            return Direction.Up;
        }
        else if (angleToTarget > 135)
        {
            return Direction.Down;
        }

        return angleToTarget > 0 ? Direction.Right : Direction.Left;
    }
}
