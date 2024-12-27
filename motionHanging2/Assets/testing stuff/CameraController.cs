using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("General Settings")]
    public float wallRunSmoothing = 10f; // Smoothing speed for wallrun camera tilt
    public float wallRunTiltAngle = 15f; // Maximum tilt angle for wallrunning

    private Transform head; // Reference to the player's head transform
    private bool isWallrunning; // Tracks whether the player is wallrunning
    private bool wallrunDirection; // Direction of the wallrun (true = right, false = left)

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        isWallrunning = false;
        head = Movement.Instance.head; // Get the player's head reference from Movement script
    }

    private void LateUpdate()
    {
        // Update camera position to follow the player's head
        transform.position = head.position;

        // Handle wallrun camera tilt
        if (isWallrunning)
        {
            // Smoothly tilt the camera based on the wallrun direction
            float targetTilt = wallrunDirection ? wallRunTiltAngle : -wallRunTiltAngle;
            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y,
                Mathf.LerpAngle(transform.rotation.eulerAngles.z, targetTilt, Time.deltaTime * wallRunSmoothing)
            );
        }
        else
        {
            // Smoothly reset the tilt to 0 when not wallrunning
            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y,
                Mathf.LerpAngle(transform.rotation.eulerAngles.z, 0, Time.deltaTime * wallRunSmoothing * 3f)
            );
        }
    }

    public void StartWallrun(bool direction)
    {
        isWallrunning = true;
        wallrunDirection = direction; // Set wallrun direction (true = right, false = left)
    }

    public void StopWallrun()
    {
        isWallrunning = false; // Reset wallrun state
    }
}
