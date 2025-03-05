using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public Transform playerBody; // The Player GameObject (for left/right rotation)
    public Transform headTransform; // The Head object (for up/down rotation)
    public float mouseSensitivity = 100f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate player left/right
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotate head up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevents looking too far up/down
        headTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
