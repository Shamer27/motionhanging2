// PlayerMovement
using System;
using UnityEngine;

public class playerMovement : MonoBehaviour
{   
    [Header("Assignables")]
    //Assignables
	public Transform playerCam;
	public Transform orientation;
	private Collider playerCollider;
	public Rigidbody rb;

    [Space(10)]

	public LayerMask whatIsGround;
	public LayerMask whatIsWallrunnable;

    [Header("MovementSettings")]
    //Movement Settings 
	public float sensitivity = 50f;
	public float moveSpeed = 4500f;
	public float walkSpeed = 20f;
	public float runSpeed = 10f;
	public bool grounded;
	public bool onWall;

	[Header("Wall Running Settings")]
	[SerializeField] private float wallRunLiftForce = 100f;
	[SerializeField] private float wallRunDuration = 0.5f;
	[SerializeField] private float wallRunJumpBoost = 10f;

    //Private Floats
    private float wallRunGravity = 1f;
	private float maxSlopeAngle = 35f;
	private float wallRunRotation;
    private float slideSlowdown = 0.2f;
	private float actualWallRotation;
	private float wallRotationVel;
	private float desiredX;
	private float xRotation;
	private float sensMultiplier = 1f;
	private float jumpCooldown = 0.25f;
	private float jumpForce = 550f;
	private float x;
	private float y;
	private float vel;

    //Private bools
	private bool readyToJump;
	private bool jumping;
	private bool sprinting;
    private bool crouching;
	private bool wallRunning;
    private bool cancelling;
	private bool readyToWallrun = true;
    private bool airborne;
    private bool onGround;
	private bool surfing;
	private bool cancellingGrounded;
	private bool cancellingWall;
	private bool cancellingSurf;

    //Private Vector3's
	private Vector3 normalVector;
	private Vector3 wallNormalVector;
	private Vector3 wallRunPos;
	private Vector3 previousLookdir;

	//vars for swinging
	private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, player;
	public new Transform camera;
    private float maxDistance = 100f;
    private SpringJoint joint;

    private Vector3 currentGrapplePosition;

    //Private int
	private int nw;

	//Testing vars
	private float timeOnWall = 0.0f;
	public bool hasWallJumped = false;

    
    //Instance
	public static playerMovement Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		rb = GetComponent<Rigidbody>();
		lr = GetComponent<LineRenderer>();
	}

	private void Start()
	{
		playerCollider = GetComponent<Collider>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		readyToJump = true;
		wallNormalVector = Vector3.up;
	}

	private void LateUpdate()
	{
        //For wallrunning
	    WallRunning();
		//rope
		DrawRope();
	}

	private void FixedUpdate()
	{
        //For moving
		Movement();
	}

	private void Update()
	{
        //Input
		MyInput();
        //Looking around
		Look();

	// 	if (onWall)
	// 	{
	// 		timeOnWall += Time.deltaTime;
	// 		if (timeOnWall > 0.5f)
	// 		{
	// 			StopWall();
				
	// 		}
	// 	}
	}

    //Player input
	private void MyInput()
	{
		x = Input.GetAxisRaw("Horizontal");
		y = Input.GetAxisRaw("Vertical");
		jumping = Input.GetButton("Jump");
		crouching = Input.GetKey(KeyCode.LeftShift);
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			StartCrouch();
		}
		if (Input.GetKeyUp(KeyCode.LeftShift))
		{
			StopCrouch();
		}
		if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }
	}

    //Scale player down
	private void StartCrouch()
	{
		float num = 400f;
		base.transform.localScale = new Vector3(1f, 0.5f, 1f);
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y - 0.5f, base.transform.position.z);
		if (rb.velocity.magnitude > 0.1f && grounded)
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

    //Moving around with WASD
	private void Movement()
	{
		rb.AddForce(Vector3.down * Time.deltaTime * 10f);
		Vector2 mag = FindVelRelativeToLook();
		float num = mag.x;
		float num2 = mag.y;
		CounterMovement(x, y, mag);
		if (readyToJump && jumping)
		{
			Jump();
		}
		float num3 = walkSpeed;
		if (sprinting)
		{
			num3 = runSpeed;
		}
		if (crouching && grounded && readyToJump)
		{
			rb.AddForce(Vector3.down * Time.deltaTime * 3000f);
			return;
		}
		if (x > 0f && num > num3)
		{
			x = 0f;
		}
		if (x < 0f && num < 0f - num3)
		{
			x = 0f;
		}
		if (y > 0f && num2 > num3)
		{
			y = 0f;
		}
		if (y < 0f && num2 < 0f - num3)
		{
			y = 0f;
		}
		float num4 = 1f;
		float num5 = 1f;
		if (!grounded)
		{
			num4 = 0.5f;
			num5 = 0.5f;
		}
		if (grounded && crouching)
		{
			num5 = 0f;
		}
		if (wallRunning)
		{
			num5 = 0.3f;
			num4 = 0.3f;
		}
		if (surfing)
		{
			num4 = 0.7f;
			num5 = 0.3f;
		}
		rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * num4 * num5);
		rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * num4);
	}

    //Ready to jump again
	private void ResetJump()
	{
		readyToJump = true;
	}

    //Player go fly
	// private void Jump()
	// {
    //     if ((grounded || wallRunning || surfing) && readyToJump)
	// 	{
	// 	    MonoBehaviour.print("jumping");
	// 	    Vector3 velocity = rb.velocity;
	// 	    readyToJump = false;
	// 	    rb.AddForce(Vector2.up * jumpForce * 1.5f);
	// 	    rb.AddForce(normalVector * jumpForce * 0.5f);
	// 	    if (rb.velocity.y < 0.5f)
	// 	    {
	// 		    rb.velocity = new Vector3(velocity.x, 0f, velocity.z);
	// 	    }
	// 	    else if (rb.velocity.y > 0f)
	// 	    {
	// 		    rb.velocity = new Vector3(velocity.x, velocity.y / 2f, velocity.z);
	// 	    }
	// 	    if (wallRunning)
	// 	    {
	// 		    rb.AddForce(wallNormalVector * jumpForce * 0.05f);
	// 			wallRunning = false; // Ensure wallRunning is set to false after jumping
	// 	    }
	// 	    Invoke("ResetJump", jumpCooldown);
    //     }
	// }

	private void Jump()
	{
		if ((grounded || wallRunning || surfing) && readyToJump)
		{
			if (wallRunning && hasWallJumped) return; // Prevent multiple wall jumps

			Vector3 velocity = rb.velocity;
			readyToJump = false;
			rb.AddForce(Vector2.up * jumpForce * 1.5f);
			rb.AddForce(normalVector * jumpForce * 0.5f);

			if (rb.velocity.y < 0.5f)
			{
				rb.velocity = new Vector3(velocity.x, 0f, velocity.z);
			}
			else if (rb.velocity.y > 0f)
			{
				rb.velocity = new Vector3(velocity.x, velocity.y / 2f, velocity.z);
			}

			if (wallRunning)
			{
				hasWallJumped = true;  // Set flag when jumping from a wall
				rb.AddForce(wallNormalVector * jumpForce * 0.05f);
				wallRunning = false; // Exit wall-running state after jumping
			}

			Invoke("ResetJump", jumpCooldown);
		}
	}



// Ensure we reset hasWallJumped when the player lands on the ground

    //Looking around by using your mouse
	private void Look()
	{
		float num = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
		float num2 = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
		desiredX = playerCam.transform.localRotation.eulerAngles.y + num;
		xRotation -= num2;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);
		FindWallRunRotation();
		actualWallRotation = Mathf.SmoothDamp(actualWallRotation, wallRunRotation, ref wallRotationVel, 0.2f);
		playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, actualWallRotation);
		orientation.transform.localRotation = Quaternion.Euler(0f, desiredX, 0f);
	}

    //Make the player movement feel good 
	private void CounterMovement(float x, float y, Vector2 mag)
	{
		if (!grounded || jumping)
		{
			return;
		}
		float num = 0.16f;
		float num2 = 0.01f;
		if (crouching)
		{
			rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideSlowdown);
			return;
		}
		if ((Math.Abs(mag.x) > num2 && Math.Abs(x) < 0.05f) || (mag.x < 0f - num2 && x > 0f) || (mag.x > num2 && x < 0f))
		{
			rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * (0f - mag.x) * num);
		}
		if ((Math.Abs(mag.y) > num2 && Math.Abs(y) < 0.05f) || (mag.y < 0f - num2 && y > 0f) || (mag.y > num2 && y < 0f))
		{
			rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * (0f - mag.y) * num);
		}
		if (Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2f) + Mathf.Pow(rb.velocity.z, 2f)) > walkSpeed)
		{
			float num3 = rb.velocity.y;
			Vector3 vector = rb.velocity.normalized * walkSpeed;
			rb.velocity = new Vector3(vector.x, num3, vector.z);
		}
	}

	public Vector2 FindVelRelativeToLook()
	{
		float current = orientation.transform.eulerAngles.y;
		float target = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * 57.29578f;
		float num = Mathf.DeltaAngle(current, target);
		float num2 = 90f - num;
		float magnitude = rb.velocity.magnitude;
		return new Vector2(y: magnitude * Mathf.Cos(num * ((float)Math.PI / 180f)), x: magnitude * Mathf.Cos(num2 * ((float)Math.PI / 180f)));
	}

		private void FindWallRunRotation()
		{
			if (!wallRunning)
			{
				wallRunRotation = 0f;
				return;
			}

			float current = playerCam.transform.rotation.eulerAngles.y;
			float target = Vector3.SignedAngle(Vector3.forward, wallNormalVector, Vector3.up);
			float deltaAngle = Mathf.DeltaAngle(current, target);

			wallRunRotation = (0f - deltaAngle / 90f) * 15f;

			// Smoothly apply the rotation
			actualWallRotation = Mathf.SmoothDamp(actualWallRotation, wallRunRotation, ref wallRotationVel, 0.2f);
			playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, actualWallRotation);
		}

	private void CancelWallrun()
	{
		MonoBehaviour.print("cancelled");
		Invoke("GetReadyToWallrun", 0.1f);
		rb.AddForce(wallNormalVector * 300f + Vector3.up * 1f, ForceMode.Impulse);
		readyToWallrun = false;
		readyToJump = true; // Allow jumping after exiting wall run
	}

	private void GetReadyToWallrun()
	{
		readyToWallrun = true;
	}

	private void WallRunning()
	{
		if (wallRunning)
		{
			Debug.Log("Wall Running");
			readyToJump = true;
			timeOnWall += Time.deltaTime;

			// Apply forces for wall-running
			rb.AddForce(-wallNormalVector * Time.deltaTime * moveSpeed);
			float wallRunLift = Mathf.Clamp(Mathf.Lerp(100f, 10f, timeOnWall / wallRunDuration), 0f, 50f);
			rb.AddForce(Vector3.up * Time.deltaTime * rb.mass * 100f * wallRunLift);
			rb.AddForce(orientation.transform.forward * Time.deltaTime * moveSpeed * 2f);

			// Ensure wall-running continues as long as the player is on the wall
			if (timeOnWall > wallRunDuration)
			{
				StopWall();
			}
		}
	}

	private bool IsFloor(Vector3 v)
	{
		return Vector3.Angle(Vector3.up, v) < maxSlopeAngle;
	}

	private bool IsSurf(Vector3 v)
	{
		float num = Vector3.Angle(Vector3.up, v);
		if (num < 89f)
		{
			return num > maxSlopeAngle;
		}
		return false;
	}

	private bool IsWall(Vector3 v)
	{
		// Check if the surface is a wall (approximately 90 degrees from the up vector)
		float angle = Vector3.Angle(Vector3.up, v);
		return Mathf.Abs(90f - angle) < 5f; // Allow a small margin of error
	}

	private bool IsRoof(Vector3 v)
	{
		return v.y == -1f;
	}

	private void StartWallRun(Vector3 normal)
	{
		if (!grounded && readyToWallrun)
		{
			Debug.Log("Starting wall run");
			wallNormalVector = normal;
			timeOnWall = 0.0f;

			if (!wallRunning)
			{
				rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
				rb.AddForce(Vector3.up * wallRunJumpBoost, ForceMode.Impulse); // Apply jump boost
			}
			wallRunning = true;
		}
	}


	private void OnCollisionStay(Collision other)
	{
		int layer = other.gameObject.layer;
		Debug.Log($"Collided with layer: {LayerMask.LayerToName(layer)}");

		if (((1 << layer) & whatIsWallrunnable) != 0)
		{
			Debug.Log("Collided with a wallrunnable object");
			for (int i = 0; i < other.contactCount; i++)
			{
				Vector3 normal = other.contacts[i].normal;
				Debug.Log($"Normal angle: {Vector3.Angle(Vector3.up, normal)}");

				if (IsWall(normal))
				{
					Debug.Log("Wall detected, starting wall run");
					StartWallRun(normal);
					onWall = true;
					cancellingWall = false;
					break;
				}
			}
		}

		// Handle grounded and surf states
		for (int i = 0; i < other.contactCount; i++)
		{
			Vector3 normal = other.contacts[i].normal;
			if (IsFloor(normal))
			{
				grounded = true;
				normalVector = normal;
				cancellingGrounded = false;
				CancelInvoke("StopGrounded");

				// Reset wall jump flag when landing
				hasWallJumped = false;
			}
			if (IsSurf(normal))
			{
				surfing = true;
				cancellingSurf = false;
				CancelInvoke("StopSurf");
			}
		}

		// Cancel grounded/wall/surf states after a delay
		float num = 3f;
		if (!cancellingGrounded)
		{
			cancellingGrounded = true;
			Invoke("StopGrounded", Time.deltaTime * num);
		}
		if (!cancellingWall)
		{
			cancellingWall = true;
			Invoke("StopWall", Time.deltaTime * num);
		}
		if (!cancellingSurf)
		{
			cancellingSurf = true;
			Invoke("StopSurf", Time.deltaTime * num);
		}
	}
	private void StopGrounded()
	{
		grounded = false;
	}

	private void StopWall()
	{
		timeOnWall = 0.0f;
		onWall = false;
		wallRunning = false;
		hasWallJumped = false; // Reset wall jump flag
	}

	private void StopSurf()
	{
		surfing = false;
	}

	public Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public float GetFallSpeed()
	{
		return rb.velocity.y;
	}

	public Collider GetPlayerCollider()
	{
		return playerCollider;
	}

	public Transform GetPlayerCamTransform()
	{
		return playerCam.transform;
	}

	public bool IsCrouching()
	{
		return crouching;
	}

	public Rigidbody GetRb()
	{
		return rb;
	}
	
	void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            // Configure joint settings
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
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