using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public float speedMultiplier = 0.5f;
    private Transform playerTransform;
    private Camera mainCamera;
    private Vector3 lastCameraPosition;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = Camera.main;
        lastCameraPosition = mainCamera.transform.position;
    }

    void LateUpdate()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 deltaMovement = cameraPosition - lastCameraPosition;
        transform.position += deltaMovement * speedMultiplier;
        lastCameraPosition = cameraPosition;
    }
}