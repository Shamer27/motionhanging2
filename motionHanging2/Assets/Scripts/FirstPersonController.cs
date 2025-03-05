using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform orientation;
    public Transform cameraTransform;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float sprintSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    private Vector3 velocity;
    private bool isSprinting;
    private bool isSliding;

    [Header("Wall Running")]
    public float wallRunSpeed = 10f;
    public float wallJumpForce = 5f;
    public LayerMask wallMask;
    private bool isWallRunning;
    private RaycastHit wallHit;

    [Header("Sliding")]
    public float slideSpeed = 14f;
    public float slideDuration = 1f;
    private float slideTimer;

    private Vector3 moveDirection;
    private bool isGrounded;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        HandleMovement();
        HandleJumping();
        HandleWallRunning();
        HandleSliding();
        ApplyGravity();
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool sprintInput = Input.GetKey(KeyCode.LeftShift);
        
        isSprinting = sprintInput && isGrounded;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        moveDirection = orientation.forward * z + orientation.right * x;
        if (!isWallRunning)
            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
    }

    void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isWallRunning)
        {
            velocity.y = wallJumpForce;
            velocity += wallHit.normal * 5f;
            isWallRunning = false;
        }
    }

    void HandleWallRunning()
    {
        if (isGrounded || !Input.GetKey(KeyCode.W)) return;

        if (Physics.Raycast(transform.position, orientation.right, out wallHit, 1f, wallMask))
        {
            StartWallRun();
        }
        else if (Physics.Raycast(transform.position, -orientation.right, out wallHit, 1f, wallMask))
        {
            StartWallRun();
        }
        else
        {
            isWallRunning = false;
        }
    }

    void StartWallRun()
    {
        isWallRunning = true;
        velocity.y = 0f;
        controller.Move(orientation.forward * wallRunSpeed * Time.deltaTime);
    }

    void HandleSliding()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && isSprinting && isGrounded)
        {
            isSliding = true;
            slideTimer = slideDuration;
        }

        if (isSliding)
        {
            controller.Move(moveDirection * slideSpeed * Time.deltaTime);
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
                isSliding = false;
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
        else
            velocity.y += gravity * Time.deltaTime;
    }
}
