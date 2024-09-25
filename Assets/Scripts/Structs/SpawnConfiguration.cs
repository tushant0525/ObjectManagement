using UnityEngine;

[System.Serializable]
public struct SpawnConfiguration
{

    public enum MovementDirection
    {
        Forward,
        Upward,
        Outward,
        Random
    }

    public MovementDirection movementDirection;

    public FloatRange speed;

    public FloatRange angularSpeed;

    public FloatRange scale;

    public ColorRangeHSV color;
}
