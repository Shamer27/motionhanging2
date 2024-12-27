using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public static Movement Instance;

    [Header("Objects")] 
    public Transform cam;
    private Transform look;
    [HideInInspector] public Transform head;
    private Transform groundCheck;
    private Rigidbody rb;

    [Header("Movement")]
    public float speed = 55f;
    public float mouseSensitivity = 3.5f;

    [Space]
    [Tooltip("How much Movement Control in the Air: 0 = No Air Movement | 1 = Same as Ground")]
    [Range(0.0f, 1.0f)]
    public float airMovement = 0.6f;

    [Space]
    [Tooltip("Player Drag when grounded")]
    [Range(0.0f, 10.0f)]
    public float groundDrag = 4f;
    [Tooltip("Player Drag when not grounded")]
    [Range(0.0f, 10.0f)]
    public float airDrag = 3f;

    [Header("Jumping")] 
    public float jumpForce = 1300f;
    private bool readyToJump;

    [Header("Wallrunning")] 
    public bool useWallrun = true;

    [Header("Walljumping")]
    public float wallJumpUpForce = 5f; // vertical jump force
    public float wallJumpAwayForce = 5f; // push off from wall 

    [Space]
    public LayerMask wallrunlayer;
    public float wallRunCheckRange = 1f;
    private Vector3 wallNormal;

    [Space]
    [Tooltip("Minimum Y Velocity to start a wallrun")]
    public float maxYVel = -10;
    public float wallRunUp = 12;
    public long wallRunJump = 300;

    private float desiredX;
    private float xRotation;
    [Space]
    [Range(0.0f, 1.5f)]
    public float wallRunJumpUpMulti = 0.6f;
    [Range(0.0f, 1.5f)]
    public float wallRunMovementMulti = 0.7f;

    [Space]
    public bool blockDoubleWallrun = true;
    private GameObject lastWallRunObject;

    [Header("GroundCheck")] 
    public GroundCheckType checkType;
    public enum GroundCheckType
    {
        Spherecast, Raycast
    }
    
    public bool grounded;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
    private Vector3 groundNormal;
    private RaycastHit[] groundHits;

    private bool isWallrunning;
    private float vertical;
    private float horizontal;
    private bool jump;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        rb = GetComponent<Rigidbody>();

        if (transform.childCount >= 3)
        {
            look = transform.GetChild(0);
            head = transform.GetChild(1);
            groundCheck = transform.GetChild(2);
        }
        else
        {
            Debug.LogError("Ensure the player object has at least 3 child objects for 'look', 'head', and 'groundCheck'.");
        }

        groundNormal = Vector3.zero;
        lastWallRunObject = gameObject;
        wallNormal = Vector3.zero;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        readyToJump = true;
        groundHits = new RaycastHit[10];
        isWallrunning = false;
    }

    private void Start()
    {
        if (groundCheck != null)
            groundCheck.transform.localPosition = new Vector3(0f, -0.95f, 0f);
    }

    private void Update()
    {
        Look();

        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        jump = Input.GetKey(KeyCode.Space);

        GroundCheck();
    }

    private void FixedUpdate()
    {
        rb.drag = grounded ? groundDrag : airDrag;

        if (readyToJump && jump && (grounded || isWallrunning))
            Jump();

        if (useWallrun)
            CheckWallRun();

        if (isWallrunning && vertical == 1)
        {
            rb.AddForce(look.up * (wallRunUp * Time.fixedDeltaTime), ForceMode.Impulse);
        }

        if (vertical == 0 && horizontal == 0)
            return;

        float multi = grounded ? 1f : airMovement;
        if (isWallrunning) multi = wallRunMovementMulti;

        Vector3 moveDirection = look.forward * (vertical * speed * Time.fixedDeltaTime * multi) +
                                 look.right * (horizontal * speed * Time.fixedDeltaTime * multi);

        rb.AddForce(moveDirection, ForceMode.Impulse);
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        desiredX += mouseX;
        xRotation = Mathf.Clamp(xRotation - mouseY, -90, 90);

        cam.localRotation = Quaternion.Euler(xRotation, desiredX, 0f);
        look.localRotation = Quaternion.Euler(0f, desiredX, 0f);
    }

    private void GroundCheck()
    {
        int hitCount = 0;

        if (checkType == GroundCheckType.Spherecast)
        {
            hitCount = Physics.SphereCastNonAlloc(groundCheck.position, groundCheckRadius, -transform.up, groundHits,
                groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        }
        else if (checkType == GroundCheckType.Raycast)
        {
            hitCount = Physics.RaycastNonAlloc(groundCheck.position, -transform.up, groundHits, groundCheckRadius,
                groundLayer, QueryTriggerInteraction.Ignore);
        }

        grounded = hitCount > 0;
        groundNormal = grounded ? groundHits[0].normal : Vector3.zero;
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -10f, 10f), rb.velocity.z);

        if (rb.velocity.y < 0) rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (isWallrunning)
        {
            //stop wallrunning
            if (CameraController.Instance != null) CameraController.Instance.StopWallrun();
            isWallrunning = false;

            //apply jump forces
            rb.AddForce(wallNormal * wallJumpAwayForce * Time.fixedDeltaTime, ForceMode.Impulse); // Push away from the wall
            rb.AddForce(transform.up * wallJumpUpForce * Time.fixedDeltaTime, ForceMode.Impulse); // Jump upwards
        }
        else if (grounded)
        {
            if (groundNormal != Vector3.zero)
            {
                rb.AddForce(transform.up * jumpForce / 2, ForceMode.Impulse);
                rb.AddForce(groundNormal * jumpForce / 2, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
        }

        readyToJump = false;
        grounded = false;
        Invoke(nameof(ResetJump), 0.15f);
    }

    private void ResetJump() => readyToJump = true;

private void CheckWallRun()
{
    if (grounded)
    {
        if (isWallrunning)
        {
            CameraController.Instance.StopWallrun();
            isWallrunning = false;
        }
        return;
    }

    if (Physics.Raycast(transform.position, look.right, out RaycastHit righthit, wallRunCheckRange, wallrunlayer))
    {
        if (!isWallrunning && rb.velocity.y < maxYVel)
            return;

        if (!isWallrunning && blockDoubleWallrun && righthit.transform.gameObject == lastWallRunObject)
            return;

        lastWallRunObject = righthit.transform.gameObject;
        wallNormal = righthit.normal;
        CameraController.Instance.StartWallrun(true);
        isWallrunning = true;
    }
    else if (Physics.Raycast(transform.position, -look.right, out RaycastHit lefthit, wallRunCheckRange, wallrunlayer))
    {
        if (!isWallrunning && rb.velocity.y < maxYVel)
            return;

        if (!isWallrunning && blockDoubleWallrun && lefthit.transform.gameObject == lastWallRunObject)
            return;

        lastWallRunObject = lefthit.transform.gameObject;
        wallNormal = lefthit.normal;
        CameraController.Instance.StartWallrun(false);
        isWallrunning = true;
    }
    else if (isWallrunning)
    {
        CameraController.Instance.StopWallrun();
        isWallrunning = false;
    }
}
}