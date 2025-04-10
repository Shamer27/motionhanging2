using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float playerHeight = 2f;

    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float airMultiplier = 0.4f;
    float movementMultiplier = 10f;

    [Header("Sprinting")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;

    [Header("Jumping")]
    public float jumpForce = 5f;


    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 0.5f;
    [SerializeField] float swingDrag = 2f;
    [SerializeField] float extraGrav = 20f;
 

    float horizontalMovement;
    float verticalMovement;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;
    public bool isGrounded { get; private set; }
    

    [Header("Swinging")]
    [SerializeField] LayerMask whatIsGrappleable;
    [SerializeField] Transform gunTip, player;
	[SerializeField] new Transform camera;
    [SerializeField] float maxDistance = 100f;

    private LineRenderer lr;
    private Vector3 grapplePoint;
    private SpringJoint joint;
    private Vector3 currentGrapplePosition;

    [Header("Sliding")]


    Vector3 moveDirection;
    Vector3 slopeMoveDirection;

    Rigidbody rb;

    RaycastHit slopeHit;

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        MyInput();
        ControlDrag();
        ControlSpeed();

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

		if (Input.GetKeyDown(crouchKey))
		{
			StartCrouch();
		}
        
        if (Input.GetKeyUp(crouchKey))
		{
			StopCrouch();
		}

        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    private void StartCrouch()
	{
		float num = 400f;
		base.transform.localScale = new Vector3(1f, 0.5f, 1f);
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y - 0.5f, base.transform.position.z);
		if (rb.velocity.magnitude > 0.1f && isGrounded)
		{
			rb.AddForce(orientation.transform.forward * num);
		}
	}

    //Scale player to original size
	private void StopCrouch()
	{
		base.transform.localScale = new Vector3(1f, 1.5f, 1f);
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + 0.5f, base.transform.position.z);
	}

    public void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

    }

    void ControlSpeed()
    {
        if (Input.GetKey(sprintKey) && isGrounded)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
        }
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }

        else if (IsGrappling())
        {
            rb.drag = swingDrag;
            rb.drag = 0;
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
           // Apply extra gravity when not grounded and not swinging
        if (!isGrounded && !IsGrappling())
        {
            Vector3 extraGravityForce = Vector3.down * extraGrav; // Adjust intensity of the extra gravity
            rb.AddForce(extraGravityForce, ForceMode.Acceleration);
        }
    }

    void MovePlayer()
    {
        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
    }


//Swinging
    void StartGrapple()
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable))
            {
                grapplePoint = hit.point;
                joint = player.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapplePoint;
                extraGrav = 15f;

                float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
                rb.AddForce(camera.forward * moveSpeed * 2.5f, ForceMode.Force); // Boost forward momentum during swing

                // Configure joint settings
                joint.maxDistance = distanceFromPoint * 0.8f;
                joint.minDistance = distanceFromPoint * 0.25f;

                joint.spring = 4.5f;
                joint.damper = 7f;
                joint.spring = 8f;
                joint.damper = 4f;
                joint.massScale = 4.5f;

                // Start drawing the rope
                lr.positionCount = 2;
                currentGrapplePosition = gunTip.position;
            }
        }

        void StopGrapple()
        {
            lr.positionCount = 0;
            if (joint != null)
            {
                Destroy(joint);
            }
        }

        void DrawRope()
        {
            if (!joint) return;

            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, currentGrapplePosition);
        }

        public bool IsGrappling()
        {
            return joint != null;
        }

        public Vector3 GetGrapplePoint()
        {
            return grapplePoint;
        }
}