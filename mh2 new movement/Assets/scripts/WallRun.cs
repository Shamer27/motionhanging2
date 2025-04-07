using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WallRunning
{
    public enum PlayerState
    {
        Normal,
        Swinging,
        WallRunning
    }
}

public class WallRun : MonoBehaviour
{
    public WallRunning.PlayerState currentState;
[Header("Wall Movement")]
[SerializeField] private Transform orientation; // Single Transform reference

[Header("Wall Detection")]
[SerializeField] private float wallDistance = 0.5f; // Single float value
[SerializeField] private float minimumJumpHeight = 1.5f; // Single float value
[SerializeField] private LayerMask whatIsWallrunnable; // Single LayerMask reference

[Header("Wall Running")]
[SerializeField] private float wallRunGravity = 5f; // Single float value
[SerializeField] private float wallRunJumpForce = 20f; // Single float value
[SerializeField] private float wallJumpForceUp = 20f; // Single float value

[Header("Wall Camera")]
[SerializeField] private Camera cam; // Single Camera reference
[SerializeField] private float fov = 60f; // Field of view value (standard is around 60)
[SerializeField] private float wallRunfov = 90f; // Adjusted field of view during wall run
[SerializeField] private float wallRunfovTime = 0.5f; // Duration to interpolate FOV change
[SerializeField] private float camTilt = 10f; // Camera tilt angle during wall run
[SerializeField] private float camTiltTime = 0.5f; // Duration to interpolate camera tilt

// Example of a properly declared array if needed in your script:
//[SerializeField] private float[] wallRunSpeeds = new float[] { 5f, 10f, 15f }; // Array with predefined values



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
                Debug.Log("wall running on the left");
            }
            else if (wallRight)
            {
                StartWallRun();
                Debug.Log("wall running on the right");
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
    if (!wallLeft && !wallRight) return;  // Only tilt when on a wall
        float targetTilt = wallLeft ? -camTilt : camTilt;
        tilt = Mathf.Lerp(tilt, targetTilt, camTiltTime * Time.deltaTime);
}


    void StartWallRun()
    {
        playerMovement.currentState = PlayerState.WallRunning;
        rb.useGravity = false;
        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunfov, wallRunfovTime * Time.deltaTime);

        ApplyCameraTilt();

        if (wallLeft)
            tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
        else if (wallRight)     
            tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);




        if (Input.GetKeyDown(KeyCode.Space))
        //  && ((Input.GetKeyDown(KeyCode.A)) || (Input.GetKeyDown(KeyCode.D)))
        { 
            
            Vector3 wallRunJumpDirection = transform.up * (wallJumpForceUp + 2) ;

            if (wallLeft)
            
                wallRunJumpDirection += leftWallHit.normal * wallRunJumpForce;

            
            else if (wallRight)
            
                wallRunJumpDirection += rightWallHit.normal * wallRunJumpForce;

            
            rb.velocity = Vector3.zero;
            rb.AddForce(wallRunJumpDirection.normalized * (wallRunJumpForce * wallJumpForceUp), ForceMode.Impulse);
            Debug.Log("Wall jumping");
            
        }


    }

    void StopWallRun()
    {
        playerMovement.currentState = PlayerState.Normal;
        rb.useGravity = true;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallRunfovTime * Time.deltaTime);
        tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
    }
}