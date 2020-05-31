using UnityEngine;

public class TimeWaypoint
{
    public Vector2 position;
    public Quaternion rotation;
    public Vector2 velocity;

    public TimeWaypoint(Vector2 _positon, Quaternion _rotation, Vector2 _velocity)
    {
        position = _positon;
        rotation = _rotation;
        velocity = _velocity;
    }
}
