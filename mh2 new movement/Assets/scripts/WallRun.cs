using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRun : MonoBehaviour
{

    [Header("Wall Movement")]
    [SerializeField] private Transform orientation;

    [Header("Wall Detection")]
    [SerializeField] private float wallDistance = 0.5f;
    [SerializeField] private float minimumJumpHeight = 1.5f;
    [SerializeField] private LayerMask whatIsWallrunnable;

    [Header("Wall Running")]
    [SerializeField] private float wallRunGravity = 5;
    [SerializeField] private float wallRunJumpForce = 20f;
    [SerializeField] private float wallJumpForceUp = 20f;

    [Header("Wall Camera")]
    [SerializeField] private Camera cam;
    [SerializeField] private float fov = 60f;
    [SerializeField] private float wallRunfov = 90f;
    [SerializeField] private float wallRunfovTime = 0.5f;
    [SerializeField] private float camTilt = 10f;
    [SerializeField] private float camTiltTime = 0.5f;

    public float tilt { get; private set; }

    private bool wallLeft = false;
    private bool wallRight = false;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    private Rigidbody rb;
    private PlayerMovement playerMovement;

    public bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void CheckWall()
    {
        Debug.DrawRay(transform.position, -orientation.right * wallDistance, Color.red);
        Debug.DrawRay(transform.position, orientation.right * wallDistance, Color.blue);

        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance, whatIsWallrunnable);
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance, whatIsWallrunnable);
    }

    private void Update()
    {
        CheckWall();

        if (CanWallRun())
        {
            if (wallLeft)
            {
                StartWallRun();
                Debug.Log("Wall running on the left");
            }
            else if (wallRight)
            {
                StartWallRun();
                Debug.Log("Wall running on the right");
            }
            else
            {
                StopWallRun();
            }
        }
        else
        {
            StopWallRun();
        }
    }

    void ApplyCameraTilt()
    {
        if (!wallLeft && !wallRight) return; // Only tilt when on a wall
        float targetTilt = wallLeft ? -camTilt : camTilt;
        tilt = Mathf.Lerp(tilt, targetTilt, camTiltTime * Time.deltaTime);
    }

    void StartWallRun()
    {
        rb.useGravity = false;
        rb.AddForce(Vector3.down * -wallRunGravity, ForceMode.Force);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunfov, wallRunfovTime * Time.deltaTime);

        ApplyCameraTilt();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 wallRunJumpDirection = transform.up * wallJumpForceUp;

            if (wallLeft)
                wallRunJumpDirection += leftWallHit.normal * wallRunJumpForce;
            else if (wallRight)
                wallRunJumpDirection += rightWallHit.normal * wallRunJumpForce;

            rb.velocity = Vector3.zero;
            rb.AddForce(wallRunJumpDirection.normalized * wallRunJumpForce, ForceMode.Impulse);
            Debug.Log("Wall jumping");
        }
    }

    void StopWallRun()
    {
        rb.useGravity = true;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallRunfovTime * Time.deltaTime);
        tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
    }
}