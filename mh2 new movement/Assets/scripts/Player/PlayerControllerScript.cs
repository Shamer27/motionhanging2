using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace ParkourFPS
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SoundPlayer))]

    [System.Serializable]
    public class ControlBindings
    {
        public KeyCode forwardKey = KeyCode.W;
        public KeyCode backwardKey = KeyCode.S;
        public KeyCode leftKey = KeyCode.A;
        public KeyCode rightKey = KeyCode.D;
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode slideKey = KeyCode.LeftControl;
        public KeyCode sprintKey = KeyCode.LeftShift;
        public int swingMouseKey = 0; // Left Click = 0, Right Click = 1, Middle = 2
    }

    public class PlayerControllerScript : MonoBehaviour
    {
        #region components
        // public bool freezeCameraRotation = false;
        private CapsuleCollider capsuleCollider;
        private Rigidbody playerRigidbody;
        private SoundPlayer soundPlayer;
        #endregion

        [Header("Camera")]
        [Tooltip("player camera transform")]
        [SerializeField] private Transform cameraTransform;
        [Tooltip("speed lines object")]
         [SerializeField] private GameObject speedLines;
        [Tooltip("camera field of view")]
        [SerializeField] private float fieldOfView = 80;


        private Camera cameraComponent; // the player camera component
        private static float lookXLimit = 90; // player upward rotation limit
        private float currRotationX = 0; // current player rotation




        [SerializeField] public ControlBindings bindings = new ControlBindings();
        [Tooltip("mouse look sensitivity")]
        [SerializeField] public float lookSensitivity = 2f; // mouse look sensitivity

        [Header("Walking")]
        [Tooltip("if to decrease movement speed when walking up a slope")]
        [SerializeField] private bool slopeSpeedDecrease = false;
        [Tooltip("base player walking speed")]
        [SerializeField] private float walkSpeed = 100;
        [Tooltip("velocity multiplier when player is in the air")]
        [SerializeField] private float airMultiplier = 1.1f;

        [Header("Running")]
        [Tooltip("if the player is able to run")]
        [SerializeField] private bool runningEnabled = true;
        [Tooltip("run button")]
        [SerializeField] private KeyCode runButton = KeyCode.LeftShift;
        [Tooltip("base player running speed")]
        [SerializeField] private float runSpeed = 130;
        [Tooltip("amount of fov increase when running")]
        [SerializeField] private float runFovIcrease = 10;

        private bool isRunning = false; // if currently running

        [Header("Stamina")]
        [Tooltip("if the player has a stamina limitation to running")]
        [SerializeField] private bool staminaEnabled = true;
        [Tooltip("stamina amount UI text")]
        [SerializeField] private Text staminaText;
        [Tooltip("the time before the stamina depletes")]
        [SerializeField] private float staminaDuration = 10;
        [Tooltip("minimum amount of stamina to regenerate before able to run again")]
        [SerializeField] private float staminaCooldown = 3;
        [Tooltip("amount of stamina to refill every second")]
        [SerializeField] private float staminaFillRate = 2;

        private float currStamina = 0; // current stamina amount
        private bool staminaEmpty = false; // if the stamina was emptied

        [Header("Wall Running")]
        [Tooltip("if the player is able to wall run")]
        [SerializeField] private bool wallrunningEnabled = true;
        [Tooltip("amount of camera lean during wall running")]
        [SerializeField] private int wallRunCameraLean = 10;
        [Tooltip("amount of gravity to reduce during wall running")]
        [SerializeField] private int wallRunGravityReduction = 70;

        private bool isWallrunning = false; // if currently wall running
        private int currentCameraLean = 0; // current camera lean

        [Header("Crouching")]
        [Tooltip("if the player is able to crouch")]
        [SerializeField] private bool crouchingEnabled = true;
        [Tooltip("crouch button")]
        [SerializeField] private KeyCode crouchButton = KeyCode.C;
        [Tooltip("base player crouching speed")]
        [SerializeField] private float crouchSpeed = 80;

        private bool isCrouching = false; // if currently crouching
        private bool changedPlayerHeight = false; // if player height was already changed

        [Header("Sliding")]
        [Tooltip("if the player is able to slide")]
        [SerializeField] private bool slidingEnabled = true;
        [Tooltip("slide button")]
        [SerializeField] private KeyCode slideButton = KeyCode.LeftControl;
        [Tooltip("sliding duration in seconds")]
        [SerializeField] private float slideDuration = 0.5f;
        [Tooltip("amount of camera lean during sliding")]
        [SerializeField] private int slideCameraLean = 15;
        [Tooltip("amount of fov increase when sliding")]
        [SerializeField] private float slideFovIncrease = 3;
        

        private bool isSliding = false; // if currently sliding

        [Header("Swinging / Grappling")]
        [SerializeField] private Transform gunTip;
        [SerializeField] private LayerMask grappleLayer;
        [SerializeField] private float grappleMaxDistance = 50f;
        [SerializeField] private float pullForce = 20f;
        [SerializeField] private LineRenderer grappleLine;
        [SerializeField] private float grappleSpring = 4.5f;
        [SerializeField] private float grappleDamper = 7f;
        [SerializeField] private float grappleMassScale = 4.5f;
        [SerializeField] private float exitForce = 5f;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float targetFOV = 60f; // Desired FOV
        [SerializeField] private float duration = 2f;   // Duration to keep the new FOV
        [SerializeField] private float fovTransitionTime = 1f; // Time to transition        
        private SpringJoint grappleJoint;
        private Vector3 grapplePoint;
        private Vector3 currentGrapplePosition;
        private bool isSwinging = false;
        private bool stoppingSwing = false;

        [Header("Jumping")]
        [Tooltip("if the player is able to jump")]
        [SerializeField] private bool jumpingEnabled = true;
        [Tooltip("jump button")]
        [SerializeField] private KeyCode jumpButton = KeyCode.Space;
        [Tooltip("the vertical force when jumping")]
        [SerializeField] private float jumpAmount = 80;
        [SerializeField] private float extraGravityForce = 3f;

        private static float jumpBufferTime = 0.2f; // the duration the player is still allowed to jump after leaving a surface
        private float lastJumpTime = 0f; // the time when last jumped
        private float jumpTryTime = 0f; // time of last failed jump attempt

        [Header("Wall Jumping")]
        [Tooltip("if the player is able to wall jump")]
        [SerializeField] private bool wallJumpingEnabled = true;
        [Tooltip("the horizontal force added when wall jumping")]
        [SerializeField] private float wallJumpForce = 40;

        [SerializeField] private LayerMask wallmoveLayer;

        [Header("Double Jump")]
        [Tooltip("if the player is able to double jump")]
        [SerializeField] private bool doubleJumpingEnabled = true;

        private bool hasDoubleJump = false; // if the player currently has a double jump remaining

        [Header("Momentum")]
        [Tooltip("if to use momentum to increase the player's movement speed")]
        [SerializeField] private bool useMomentum = true;
        [Tooltip("momentum amount UI text")]
        [SerializeField] private Text momentumText;
        [Tooltip("the minimum velocity required to maintain momentum")]
        [SerializeField] private float momentumResetThreshold = 5;
        [Tooltip("amount of momentum to decrease every second")]
        [SerializeField] private float momentumDecreaseRate = 0.01f;
        [Tooltip("amount of momentum increase per frame when running")]
        [SerializeField] private float runMomentumIncrease = 0.001f;
        [Tooltip("amount of momentum icrease per frame when wall running")]
        [SerializeField] private float wallRunMomentumIncrease = 0.005f;
        [Tooltip("amount of momentum increase when starting a slide")]
        [SerializeField] private float slideMomentumIncrease = 0.2f;

        private float momentum = 1; // current player momentum

        [Header("Drag & Gravity")]
        [Tooltip("drag while grounded")]
        [SerializeField] private float groundDrag = 5;
        [Tooltip("drag while in the air")]
        [SerializeField] private float airDrag = 5;
        [Tooltip("gravity amount, the higher the stronger gravity is")]
        [SerializeField] private float gravity = 100;

        [Header("Ground Detection")]
        [Tooltip("the layers of the ground objects")]
        [SerializeField] private LayerMask groundMask;
        [Tooltip("ground check location")]
        [SerializeField] private Transform groundCheck;
        [Tooltip("right wall check location")]
        [SerializeField] private Transform wallCheckRight;
        [Tooltip("left wall check location")]
        [SerializeField] private Transform wallCheckLeft;

        private static float groundDistance = 0.4f; // the max distance from the ground to detect touch
        private bool isGrounded; // if currently touching the ground
        private RaycastHit slopeHit; // raycast for slope detection

        // check if the player is standing on a slope
        private bool OnSlope()
        {
            // check collision with a raycast
            if (Physics.Raycast(groundCheck.position, Vector3.down, out slopeHit, groundDistance))
                if (slopeHit.normal != Vector3.up) // if surface is angled
                    return true;

            return false;
        }

        // Start is called before the first frame update
        private void Start()
        {
            #region initialize components
            capsuleCollider = GetComponent<CapsuleCollider>();
            playerRigidbody = GetComponent<Rigidbody>();
            soundPlayer = GetComponent<SoundPlayer>();
            cameraComponent = cameraTransform.GetComponent<Camera>();
            #endregion

            // set fov
            cameraComponent.fieldOfView = fieldOfView;

            

            // set gravity
            Physics.gravity = new Vector3(0, -gravity, 0);

            // set momentum
            if (useMomentum) // if using momentum
            {
                if (momentumDecreaseRate != 0) // if reducing momentum
                    StartCoroutine(ReduceMomentum()); // start reducing momentum over time

                StartCoroutine(CheckMomentumReset()); // check when momentum needs to reset
            }
            else if (momentumText != null)
                momentumText.gameObject.SetActive(false); // disable momnetum text

            // set stamina
            if (staminaEnabled) // if using stamina
                StartCoroutine(ControlStamina()); // start controlling the stamina amount
            else if (staminaText != null)
                staminaText.gameObject.SetActive(false); // disable stamina text

            // start checking if the player is grounded
            StartCoroutine(CheckGrounded());    
        }

        // Update is called once per frame
        private void Update()
        {
            /* handle player input */
            if (MenuManager.GameIsPaused) return;

            SetRotation(); // set player rotation

            CheckUserInput(); // check user key input
        }

        // FixedUpdate is called once per physics frame
        private void FixedUpdate()
        {
            /* handle physics changes */
            if (MenuManager.GameIsPaused) return;


            // set momentum text
            momentumText.text = "Momentum - " + ((momentum - 1f) * 10f).ToString("0.0");

            // check if touching a wall
            touchingWallRight = TouchingWallRight(); // set touching right wall status
            if (touchingWallRight) // if touching
                rightWallTouchTime = Time.time; // set touch time

            touchingWallLeft = TouchingWallLeft(); // set touching left wall status
            if (touchingWallLeft) // if touching
                leftWallTouchTime = Time.time; // set touch time

            // check if touching the ground
            touchingGround = TouchingGround();

            if (isSwinging && grappleJoint != null)
            {
                Vector3 directionToPoint = (grapplePoint - transform.position).normalized;

                // Always apply a smooth pull
                playerRigidbody.AddForce(directionToPoint * pullForce, ForceMode.Acceleration);

                // Optional: forward boost from input
                if (Input.GetKey(bindings.forwardKey))
                {
                    Vector3 forwardDir = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                    playerRigidbody.AddForce(forwardDir * (pullForce * 0.5f), ForceMode.Acceleration);
                }
            }

            // set speed lines
            if (speedLines != null) // if speedlines object set
            {
                if (isWallrunning || isSliding || stoppingSwing) // if wallrunning or sliding
                    speedLines.SetActive(true); // enable speed lines
                else // not wallrunning and not sliding
                    speedLines.SetActive(false); // disable speed lines
            }



            SetPlayerHeight(); // set player height

            SetFov(); // set player field of view

            SetDrag(); // set player drag

            MovePlayer(); // set player movement
        }

        private void LateUpdate()
        {
            if (isSwinging && grappleJoint != null)
                DrawGrappleRope();
        }
        // check user key presses and handle input
        private void CheckUserInput()
        {
            /* running and crouching */
            if (Input.GetKey(bindings.sprintKey) && runningEnabled && (!staminaEnabled || !staminaEmpty))
            {
                isRunning = true;
                isCrouching = false;
            }
            else // not running
            {
                isRunning = false; // set running false

                if (Input.GetKey(crouchButton) && crouchingEnabled) // if holding the crouch button
                    isCrouching = true; //set crouching true
                else // not crouching
                    isCrouching = false; //set crouching false
            }

            /* jumping */

            if (Input.GetKeyDown(bindings.jumpKey) && jumpingEnabled)
                Jump();

            /* sliding */
            // while (Input.GetKeyDown(slideButton) && !isSliding && slidingEnabled && isGrounded)
            if (Input.GetKeyDown(bindings.slideKey) && !isSliding && slidingEnabled)
                StartCoroutine(Slide());

            if (Input.GetMouseButtonDown(bindings.swingMouseKey))
                StartSwing();
            else if (Input.GetMouseButtonUp(bindings.swingMouseKey))
                StopSwing();
        }

        // set camera field of view depending on player state
        private void SetFov()
        {
            if (isRunning) // if running
                cameraComponent.fieldOfView = fieldOfView + runFovIcrease;
            else if (isSliding) // if sliding
                cameraComponent.fieldOfView = fieldOfView + slideFovIncrease;
            else
                cameraComponent.fieldOfView = fieldOfView;
        }

        // set player height depending on player state
        private void SetPlayerHeight()
        {
            if (isSliding || isCrouching) // if sliding or crouching
            {
                if (!changedPlayerHeight) // if height not changed yet
                {
                    changedPlayerHeight = true;

                    // decrease player height
                    capsuleCollider.height /= 2f;
                    capsuleCollider.center -= new Vector3(0, capsuleCollider.height / 2f, 0);
                    cameraTransform.localPosition /= 4f;
                }
            }
            else
            {
            if (changedPlayerHeight)
            {
                // 👇 Replace the whole old block here 👇
                float standHeight = capsuleCollider.height * 2f;
                float radius = capsuleCollider.radius;
                Vector3 bottom = transform.position + Vector3.up * radius;
                Vector3 top = bottom + Vector3.up * (standHeight - radius * 2f);

                if (!Physics.CheckCapsule(bottom, top, radius, groundMask))
                {
                    changedPlayerHeight = false;

                    capsuleCollider.center += new Vector3(0, capsuleCollider.height / 2f, 0);
                    capsuleCollider.height *= 2f;
                    cameraTransform.localPosition *= 4f;
                }
                else
                {
                    Debug.Log("Blocked above - can't uncrouch");
                }
            }
        }

        }

        #region sliding
        // try to make the player slide if able to
        private IEnumerator Slide(bool retry = false)
        {
            if (!isSliding)
            {
                if (!touchingWallRight && !touchingWallLeft)
                {
                    /* start sliding */
                    isSliding = true;
                    Debug.Log("is sliding");

                    if (slideMomentumIncrease != 0)
                        momentum += slideMomentumIncrease;

                    soundPlayer.PlaySound(soundPlayer.slidingSound);

                    // sliding loop
                    while (Input.GetKey(slideButton))
                    {
                        yield return null;
                    }

                    /* slide ends, now delay before resetting state */
                    float slideEndDelay = 0.3f; // tweak this value as desired
                    yield return new WaitForSeconds(slideEndDelay);

                    isSliding = false;
                }
                else if (!retry)
                {
                    float retryEndTime = Time.time + jumpBufferTime;

                    while (Time.time <= retryEndTime && !isSliding)
                    {
                        if (isGrounded && !touchingWallRight && !touchingWallLeft)
                        {
                            yield return Slide(retry: true);
                            break;
                        }

                        yield return new WaitForFixedUpdate();
                    }
                }
            }
        }


        #endregion

        #region jumping
        // make player jump if he is able to
        private void Jump(bool retry = false)
        {
            if ((isGrounded || hasDoubleJump) && (Time.time >= (lastJumpTime + jumpBufferTime + 0.01f))) // if player is touching the ground or able to double jump
            {
                if (!isGrounded) // if player used double jump
                    hasDoubleJump = false; // disable double jump

                isGrounded = false; // set not grounded
                lastJumpTime = Time.time; // set jump time

                float horizontalJumpForce = 0;

                if (wallJumpingEnabled && !touchingGround) // if wall jumping is enabled and not in the ground
                {
                    if (Time.time < (rightWallTouchTime + jumpBufferTime)) // if touching a wall to the right of the player
                        horizontalJumpForce = -wallJumpForce;
                    else if (Time.time < (leftWallTouchTime + jumpBufferTime)) // if touching a wall to the left of the player
                        horizontalJumpForce = wallJumpForce;

                    if (transform.right.x < 0) // if facing the opposite direction
                        horizontalJumpForce = -horizontalJumpForce; // reverse horizontal jump force direction
                }

                playerRigidbody.AddForce(new Vector3(horizontalJumpForce, jumpAmount, 0), ForceMode.VelocityChange); //add vertical jump force
    
                
                    playerRigidbody.AddForce(Vector3.down * extraGravityForce, ForceMode.Acceleration);
                

                // play jump sound
                soundPlayer.PlaySound(soundPlayer.jumpSound, volume: 0.6f);

                StartCoroutine(CheckLandingSound()); // queue playing landing sound after landing
            }
            else if (!retry) // not touching the floor
            {
                jumpTryTime = Time.time; // set last jump try time
            }
        }

        // play landing sound after jumping when player is touching the floor
        private IEnumerator CheckLandingSound()
        {
            yield return new WaitForSeconds(jumpBufferTime + 0.1f);

            // wait until player is grounded
            while (!isGrounded)
                yield return null;

            // play landing sound
            soundPlayer.PlaySound(soundPlayer.landingSound, volume: 0.6f);
        }
        #endregion
        #region swinging

        IEnumerator changeFOV()
        {
            float startFOV = mainCamera.fieldOfView;
            float elapsedTime = 0f;

            while (elapsedTime < fovTransitionTime)
            {
                stoppingSwing = true;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            //wait for the duration
            yield return new WaitForSeconds(duration);

            //set fov back
            elapsedTime = 0f;
            while (elapsedTime < fovTransitionTime)
            {
                stoppingSwing = false;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            mainCamera.fieldOfView = startFOV;
        }
        private void StartSwing()
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, grappleMaxDistance, grappleLayer))
            {
                grapplePoint = hit.point;

                grappleJoint = gameObject.AddComponent<SpringJoint>();
                grappleJoint.autoConfigureConnectedAnchor = false;
                grappleJoint.connectedAnchor = grapplePoint;

                float distance = Vector3.Distance(transform.position, grapplePoint);
                grappleJoint.maxDistance = distance * 0.8f;
                grappleJoint.minDistance = distance * 0.25f;

                grappleJoint.spring = grappleSpring;
                grappleJoint.damper = grappleDamper;
                grappleJoint.massScale = grappleMassScale;

                currentGrapplePosition = gunTip.position;
                grappleLine.positionCount = 2;
                isSwinging = true;
                Vector3 launchDir = (grapplePoint - transform.position).normalized;
                playerRigidbody.AddForce(launchDir * 8f, ForceMode.VelocityChange);
            }
        }

        private void StopSwing()
        {
            isSwinging = false;
            // StartCoroutine(changeFOV());
            grappleLine.positionCount = 0;
            
            if (grappleJoint != null)
                playerRigidbody.AddRelativeForce(Vector3.forward * exitForce);
                Destroy(grappleJoint);
            
        }

        private void DrawGrappleRope()
        {
            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);
            grappleLine.SetPosition(0, gunTip.position);
            grappleLine.SetPosition(1, currentGrapplePosition);
        }
        #endregion

        #region movement
        // control player movement drag in the ground/air
        private void SetDrag()
        {
            if (isGrounded)
                playerRigidbody.drag = groundDrag;
            else
                playerRigidbody.drag = airDrag;
        }

        // set player velocity according to input
        private void MovePlayer()
        {
            // get player input
            float verticalMoveAmount = Input.GetAxisRaw("Vertical");
            float horizontalMoveAmount = Input.GetAxisRaw("Horizontal");

            #region wall running
            if (wallrunningEnabled && !isCrouching && !touchingGround ) // if wallrunning is enabled and not currently crouching or touching the ground
            {
                // set wallrunning if touching a wall and moving forward
                if (!isSliding && (touchingWallRight || touchingWallLeft) && verticalMoveAmount > 0)
                    {
                        isWallrunning = true;
                        isSliding = false;
                    } // reset sliding status
                    
                isWallrunning = (touchingWallRight || touchingWallLeft) && verticalMoveAmount > 0;

                if (isWallrunning) // if currently wallrunning
                {
                    Physics.gravity = new Vector3(0, wallRunGravityReduction - gravity, 0); // reduce gravity

                    momentum += wallRunMomentumIncrease * 1.05f; // increase momentum

                    verticalMoveAmount = 1.1f; // move forward
                    horizontalMoveAmount *= 0.3f; // reduce side movement
                }
                else // not wallrunning
                    Physics.gravity = new Vector3(0, -gravity, 0); // reset gravity
            }
            else
                isWallrunning = false;
            #endregion

            // prevent the player from sticking to walls
            if (((touchingWallRight && horizontalMoveAmount > 0) || (touchingWallLeft && horizontalMoveAmount < 0))
                && !isWallrunning) // if trying to move into a wall and not wallrunning
                horizontalMoveAmount = 0; // zero horizontal move force

            // calculate movement direction based on player's input
            Vector3 moveDirection = transform.forward * verticalMoveAmount + transform.right * horizontalMoveAmount;

            // set default movement force for moving on the ground
            Vector3 moveForce = moveDirection.normalized;

            if (!isGrounded) // if moving in the air
                moveForce *= airMultiplier; // set air movement force
            #region slope
            else if (OnSlope()) // if moving on a slope
            {
                // set movement to the slope direction
                Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
                moveForce = slopeMoveDirection.normalized;

                if (!slopeSpeedDecrease) // if not decreasing movement speed on slopes
                    moveForce *= (1 + slopeMoveDirection.y); // set movement force accordingly
            }
            #endregion

            if (isRunning) // if currently running
            {
                if (runMomentumIncrease != 0) // if running increases momentum
                    momentum += runMomentumIncrease; // increase momentum

                moveForce *= runSpeed; // set speed to running speed
            }
            else if (isCrouching) // if currently crouching
                moveForce *= crouchSpeed; // set speed to walking speed
            else // not running or crouching
                moveForce *= walkSpeed; // set speed to walking speed

            if (useMomentum) // if momentum is enabled
                moveForce *= momentum; // increase movement speed by momentum amount

            // apply force to the rigidbody
            playerRigidbody.AddForce(moveForce, ForceMode.Acceleration);

            CheckFootstepSound(moveDirection.magnitude); // handle footsteps sound
        }

        // check if need to play or stop footsteps sound
        private void CheckFootstepSound(float movementAmount)
        {
            if (isGrounded && movementAmount > 0.1f && !isSliding) // if player is grounded and moving and not sliding
            {
                if (!soundPlayer.isPlaying || (soundPlayer.clip != soundPlayer.jumpSound && soundPlayer.clip != soundPlayer.landingSound)) // if sound is not already playing
                {
                    if (isRunning || isWallrunning) // if running
                    {
                        if (soundPlayer.clip != soundPlayer.runningSound || !soundPlayer.isPlaying) // if not already playing running sound
                            soundPlayer.PlaySound(soundPlayer.runningSound, loop: true); // play running sound
                    }
                    else if (soundPlayer.clip != soundPlayer.walkingSound || !soundPlayer.isPlaying) // walking and not already playing walking sound
                        soundPlayer.PlaySound(soundPlayer.walkingSound, loop: true); // play walking sound
                }
            }
            else if (soundPlayer.isPlaying && (soundPlayer.clip == soundPlayer.runningSound || soundPlayer.clip == soundPlayer.walkingSound)) // player stopped moving
                soundPlayer.Stop(); // stop footstep sounds
        }

        // increase or reduce stamina depending if the player is currently running
        private IEnumerator ControlStamina()
        {
            currStamina = staminaDuration; // set start stamina amount

            while (true)
            {
                staminaText.text = $"Stamina - {currStamina.ToString("0.0")}"; // set stamina text

                if (isRunning && currStamina > 0) // if currently running and stamina not empty
                    currStamina -= 0.1f; // decrease stamina
                else if (currStamina < staminaDuration) // not running and stamina not full
                {
                    if (currStamina + (staminaFillRate / 10f) > staminaDuration) // if exceeds max stamina
                        currStamina = staminaDuration; // set stamina to max
                    else
                        currStamina += (staminaFillRate / 10f); // increase stamina
                }

                if (currStamina <= 0.1f) // if stamina ran out
                    staminaEmpty = true; // set stamina empty
                else if (staminaEmpty && currStamina >= staminaCooldown) // if player has enough stamina
                    staminaEmpty = false; // set stamina not empty

                yield return new WaitForSeconds(0.1f);
            }
        }
        #endregion

        #region rotation
        // set player rotation based on mouse movement
        private void SetRotation()
        {
            if (MenuManager.GameIsPaused) return;
            
            if (isSliding) // if sliding
            {
                if (currentCameraLean < slideCameraLean)
                    currentCameraLean++; // lean left
            }
            else if (touchingWallRight && !touchingGround) // if touching a wall to the right of the player and not on the ground
            {
                if (currentCameraLean < wallRunCameraLean)
                    currentCameraLean++; // lean left
            }
            else if (touchingWallLeft && !touchingGround) // if touching a wall to the left of the player and not on the ground
            {
                if (-currentCameraLean < wallRunCameraLean)
                    currentCameraLean--; // lean right
            }
            else // not touching a wall and not sliding
            {
                // reset camera lean
                if (currentCameraLean > 0)
                    currentCameraLean--;
                else if (currentCameraLean < 0)
                    currentCameraLean++;
            }

            currRotationX -= Input.GetAxis("Mouse Y") * lookSensitivity;
            currRotationX = Mathf.Clamp(currRotationX, -lookXLimit, lookXLimit);
            cameraTransform.localRotation = Quaternion.Euler(currRotationX, 0, currentCameraLean);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSensitivity, 0);
        }
        #endregion

        #region momentum
        // check if the player is slowing down to reset momentum
        private IEnumerator CheckMomentumReset()
        {
            while (true)
            {
                // if player slowed down
                if (Mathf.Abs(playerRigidbody.velocity.x) < (momentumResetThreshold * momentum)
                        && Mathf.Abs(playerRigidbody.velocity.z) < (momentumResetThreshold * momentum)
                        && !touchingWallLeft && !touchingWallRight)
                    momentum = 1f; // reset momentum

                yield return new WaitForFixedUpdate(); // wait for next frame
            }
        }

        // continuously reduce the player's momentum over time
        private IEnumerator ReduceMomentum()
        {
            while (true)
            {
                yield return new WaitForSeconds(1); // wait 1 second

                // decrease momentum over time
                if ((momentum - momentumDecreaseRate) >= 1)
                    momentum -= momentumDecreaseRate;
            }
        }
        #endregion

        #region ground detection
        private bool touchingGround = false; // if the player is touching the floor
        private bool TouchingGround() => Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); // check if the player is touching the floor using collision sphere

        private bool touchingWallRight = false; // if the player is touching a wall to the right
        private bool TouchingWallRight() => Physics.CheckSphere(wallCheckRight.position, groundDistance * 2f, groundMask); // check if the player is touching a wall to the right using collision sphere
        private float rightWallTouchTime = 0; // the time when last touched a wall to the right

        private bool touchingWallLeft = false; // if the player is touching a wall to the left
        private bool TouchingWallLeft() => Physics.CheckSphere(wallCheckLeft.position, groundDistance * 2f, groundMask); // check if the player is touching a wall to the left using collision sphere
        private float leftWallTouchTime = 0; // the time when last touched a wall to the left

        // turn off grounded status after a buffer time
        private IEnumerator DisableGrounded()
        {
            yield return new WaitForSeconds(jumpBufferTime); // delay

            isGrounded = false; // set grounded false
        }

        // check if the player is touching a ground object
        private IEnumerator CheckGrounded()
        {
            while (true)
            {
                if (touchingGround || (wallJumpingEnabled && (touchingWallRight || touchingWallLeft))) // touching a ground surface
                {
                    isGrounded = true; // set grounded true

                    // refresh double jump when player is grounded
                    if (doubleJumpingEnabled)
                        hasDoubleJump = true;

                    if (Time.time <= (jumpTryTime + jumpBufferTime) && jumpTryTime != 0) // if player tried to jump lately before hitting the ground
                        Jump(retry: true); // try to jump
                }
                else // not touching the ground
                {
                    StartCoroutine(DisableGrounded()); // turn off grounded
                }

                yield return new WaitForFixedUpdate();
            }
        }
        #endregion
    }
}