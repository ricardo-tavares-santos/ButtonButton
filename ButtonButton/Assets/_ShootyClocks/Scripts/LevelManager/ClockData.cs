using UnityEngine;
using System.Collections;

[System.Serializable]
public enum ClockType
{
    NORMAL,
    BIG_BULLET,
    FILL_GRID,
    SPEED_UP,
    SPEED_DOWN,
    WALL
}

[System.Serializable]
public enum ArrowRotatingDirection
{
    CLOCKWISE = 0,
    COUNTER_CLOCKWISE = 1
}


[System.Serializable]
public class ClockData
{

    public bool isShootingClock;
    public ClockType clockType;
    public ArrowRotatingDirection arrowRoratingDirection;
    public float arrowRotatingSpeed;
    public Vector2 position;
    public Vector2 scale;
}
