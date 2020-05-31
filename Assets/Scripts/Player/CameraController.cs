using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Used to create smooth camera movement

    public int glideFraction = 10;

    public Transform target;

    private Vector2 oldPlayerPosition;

    void Start()
    {
        oldPlayerPosition = target.position;
    }

    // Every frame, need to move the camera a glideFraction'th of the way closer to the player
    void FixedUpdate()
    {
        Vector3 cameraShift = (oldPlayerPosition - (Vector2)transform.position) / glideFraction;
        transform.position += cameraShift;

        oldPlayerPosition = target.position;
    }
}
