using UnityEngine;

/// <summary>
/// Third-person character controller with improved movement and rotation system.
/// Requires: Character Controller component and Player tag on the GameObject.
/// </summary>
public class PersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed")]
    public float moveSpeed = 5f;
    
    [Tooltip("Additional speed when sprinting")]
    public float sprintSpeed = 3.5f;
    
    [Tooltip("Movement speed multiplier when crouching (0.5 = 50% speed)")]
    [Range(0.1f, 1f)]
    public float crouchSpeedMultiplier = 0.5f;

    [Header("Jump Settings")]
    [Tooltip("Initial jump velocity")]
    public float jumpForce = 5f;
    
    [Tooltip("How long the jump boost is applied")]
    public float jumpDuration = 0.8f;

    [Header("Physics")]
    [Tooltip("Gravity force applied to the character")]
    public float gravity = 9.8f;

    [Header("Rotation")]
    [Tooltip("How fast the character rotates (higher = faster)")]
    [Range(0.1f, 1f)]
    public float rotationSpeed = 0.5f;

    [Header("References")]
    public Animator animator;

    // Components
    private CharacterController characterController;

    // State tracking
    private bool isJumping = false;
    private bool isSprinting = false;
    private bool isCrouching = false;
    private float jumpTimeElapsed = 0f;
    private float verticalVelocity = 0f;

    // Input cache
    private float inputHorizontal;
    private float inputVertical;
    private bool inputJump;
    private bool inputSprint;

    void Start()
    {
        // Get required components
        characterController = GetComponent<CharacterController>();
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Validation
        if (characterController == null)
        {
            Debug.LogError("Character Controller component not found! This script requires a Character Controller.");
        }

        if (animator == null)
        {
            Debug.LogWarning("Animator component not found. Animations will not play.");
        }
    }

    void Update()
    {
        // Capture inputs
        CaptureInput();

        // Handle crouch toggle
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            isCrouching = !isCrouching;
        }

        // Update animations
        UpdateAnimations();

        // Handle jump input
        if (inputJump && characterController.isGrounded && !isJumping)
        {
            InitiateJump();
        }

        // Detect head collision during jump
        DetectHeadCollision();
    }

    void FixedUpdate()
    {
        // Calculate movement
        Vector3 movement = CalculateMovement();

        // Apply movement
        characterController.Move(movement);
    }

    /// <summary>
    /// Captures and caches input values
    /// </summary>
    private void CaptureInput()
    {
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetButtonDown("Jump");
        inputSprint = Input.GetButton("Fire3");
    }

    /// <summary>
    /// Updates animator parameters based on current state
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Check if there's movement input
        bool hasInput = inputHorizontal != 0 || inputVertical != 0;
        bool isGrounded = characterController.isGrounded;

        // Set animator parameters
        animator.SetBool("crouch", isGrounded && isCrouching);
        animator.SetBool("run", isGrounded && hasInput);
        animator.SetBool("air", !isGrounded);

        // Sprint only if moving, grounded, and not crouching
        isSprinting = isGrounded && hasInput && inputSprint && !isCrouching;
        animator.SetBool("sprint", isSprinting);
    }

    /// <summary>
    /// Calculates the movement vector for this frame
    /// </summary>
    private Vector3 CalculateMovement()
    {
        // === HORIZONTAL MOVEMENT ===
        Vector3 horizontalMovement = Vector3.zero;

        if (inputHorizontal != 0 || inputVertical != 0)
        {
            // Get camera-relative directions
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            // Flatten to horizontal plane
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate desired movement direction
            Vector3 desiredMoveDirection = (cameraForward * inputVertical + cameraRight * inputHorizontal).normalized;

            // Rotate character to face movement direction
            if (desiredMoveDirection.magnitude >= 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
            }

            // Calculate speed with modifiers
            float currentSpeed = moveSpeed;

            if (isSprinting)
            {
                currentSpeed += sprintSpeed;
            }
            else if (isCrouching)
            {
                currentSpeed *= crouchSpeedMultiplier;
            }

            // Apply speed to direction
            horizontalMovement = desiredMoveDirection * currentSpeed * Time.deltaTime;
        }

        // === VERTICAL MOVEMENT (Jump & Gravity) ===
        
        if (characterController.isGrounded)
        {
            // Reset vertical velocity when grounded
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f; // Small constant to keep grounded
            }
        }

        // Handle jumping
        if (isJumping)
        {
            // Apply jump force with smooth falloff
            float jumpProgress = jumpTimeElapsed / jumpDuration;
            verticalVelocity = Mathf.Lerp(jumpForce, jumpForce * 0.3f, jumpProgress);

            jumpTimeElapsed += Time.deltaTime;

            // End jump when duration expires
            if (jumpTimeElapsed >= jumpDuration)
            {
                isJumping = false;
                jumpTimeElapsed = 0f;
            }
        }

        // Apply gravity
        verticalVelocity -= gravity * Time.deltaTime;

        Vector3 verticalMovement = Vector3.up * verticalVelocity * Time.deltaTime;

        // === COMBINE AND RETURN ===
        return horizontalMovement + verticalMovement;
    }

    /// <summary>
    /// Initiates a jump
    /// </summary>
    private void InitiateJump()
    {
        isJumping = true;
        jumpTimeElapsed = 0f;
        verticalVelocity = jumpForce;
    }

    /// <summary>
    /// Detects if the character's head hits something during a jump
    /// </summary>
    private void DetectHeadCollision()
    {
        if (!isJumping) return;

        float detectionDistance = characterController.height * 0.55f;
        Vector3 rayOrigin = transform.TransformPoint(characterController.center);

        if (Physics.Raycast(rayOrigin, Vector3.up, detectionDistance))
        {
            // Hit head on something, cancel jump
            isJumping = false;
            jumpTimeElapsed = 0f;
            verticalVelocity = 0f;
        }

        // Debug visualization (optional)
        // Debug.DrawRay(rayOrigin, Vector3.up * detectionDistance, Color.red);
    }

    // === GIZMOS FOR DEBUGGING ===
    private void OnDrawGizmosSelected()
    {
        if (characterController != null)
        {
            // Draw movement direction
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(origin, transform.forward * 2f);
        }
    }
}