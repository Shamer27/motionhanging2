using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float PlayerHeight = 2f;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    [SerializeField] float airMultiplier = 0.6f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    public float groundDrag = 6f;
    public float airDrag = 2f;
    public float movementMultiplier = 10f;


    float horizontalMovement;
    float verticalMovement;

    bool isGrounded;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, PlayerHeight / 2 + 0.1f);

       

        MyInput();
        ControlDrag();

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
           Jump();
        }
    }

    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
    
        moveDirection = transform.forward * verticalMovement + transform.right * horizontalMovement;
    }

    void Jump()
    {

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
        
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        if (isGrounded)
        {
            rb.AddForce(moveDirection * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
    }


}
