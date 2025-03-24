using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRun : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform orientation;

    [Header("Detection")]
    [SerializeField] private float wallDistance = .5f;
    [SerializeField] private float minimumJumpHeight = 1.5f;
    [SerializeField] private LayerMask whatIsWallrunnable;

    [Header("Wall Running")]
    [SerializeField] private float wallRunGravity;
    [SerializeField] private float wallRunJumpForce;
    // [SerializeField] private float wallJumpForce = 300f;
    [SerializeField] private float wallJumpForceUp = 20f;
    // [SerializeField] private float WJForceBack = 20f;
    // [SerializeField] private float WJForceForward = 20f;

    [Header("Camera")]
    [SerializeField] private Camera cam;
    [SerializeField] private float fov;
    [SerializeField] private float wallRunfov;
    [SerializeField] private float wallRunfovTime;
    [SerializeField] private float camTilt;
    [SerializeField] private float camTiltTime;

    [Header("Testing variables")]
    [SerializeField] private bool isWallRunning;
    // [SerializeField] private float wallRunCooldown = 0.2f;
    [SerializeField] private float lastWallRunTime;


    public float tilt { get; private set; }

    private bool wallLeft = false;
    private bool wallRight = false;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    private Rigidbody rb;

  

    bool CanWallRun()
    {

        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
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
            
            Vector3 wallRunJumpDirection = transform.up * wallJumpForceUp;

            if (wallLeft)
            
                wallRunJumpDirection += leftWallHit.normal * wallRunJumpForce;
                // Vector3 wallRunJumpDirection = transform.up + leftWallHit.normal;
                // rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                // rb.AddForce(wallRunJumpDirection * wallRunJumpForce * wallRunJumpForce * wallRunJumpForce , ForceMode.Force);
                // Debug.Log("Wall jumping");
            
            else if (wallRight)
            
                wallRunJumpDirection += rightWallHit.normal * wallRunJumpForce;
                // Vector3 wallRunJumpDirection = transform.up + rightWallHit.normal;
                // rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); 
                // rb.AddForce(wallRunJumpDirection * wallRunJumpForce * wallJumpForce * wallRunJumpForce, ForceMode.Force);
                // Debug.Log("Wall jumping");
            
            rb.velocity = Vector3.zero;
            rb.AddForce(wallRunJumpDirection.normalized * wallRunJumpForce, ForceMode.Impulse);
            Debug.Log("Wall jumping");
            
        }

        // if (Time.time < lastWallRunTime + wallRunCooldown)  return;
        
        // isWallRunning = true;
        // rb.useGravity = false;
        // rb.velocity = Vector3.zero;
        // lastWallRunTime = Time.time;

        // rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);   
        


    }

    void StopWallRun()
    {
        rb.useGravity = true;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallRunfovTime * Time.deltaTime);
        tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
    }
}