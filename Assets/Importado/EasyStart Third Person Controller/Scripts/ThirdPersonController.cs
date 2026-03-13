
using UnityEditor.VersionControl;
using UnityEngine;

/*
    This file has a commented version with details about how each line works. 
    The commented version contains code that is easier and simpler to read. This file is minified.
*/


/// <summary>
/// Main script for third-person movement of the character in the game.
/// Make sure that the object that will receive this script (the player) 
/// has the Player tag and the Character Controller component.
/// </summary>
public class ThirdPersonController : MonoBehaviour
{

    [Tooltip("Speed ​​at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;
    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float sprintAdittion = 3.5f;
    [Tooltip("The higher the value, the higher the character will jump.")]
    public float jumpForce = 18f;
    [Tooltip("Stay in the air. The higher the value, the longer the character floats before falling.")]
    public float jumpTime = 0.85f;
    [Space]
    [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
    public float gravity = 9.8f;

    float jumpElapsedTime = 0;

    // Player states
    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;

    // Inputs
    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;

    public Animator animator;
    CharacterController cc;


    void Start()
    {
        /*cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Message informing the user that they forgot to add an animator
        if (animator == null)
            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");
        */
        cc = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>(); 

        if (animator == null)
            Debug.LogWarning("Hey buddy, the Animator component wasn't found in the player or its children.");
    }


    // Update is only being used here to identify keys and trigger animations
    void Update()
    {
        // Input checkers
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);

        // Check if you pressed the crouch input key
        if (inputCrouch)
            isCrouching = !isCrouching;

        // --- ANIMACIONES ---
        if (animator != null)
        {
            // Verificar si hay input del jugador
            Vector2 inputDir = new Vector2(inputHorizontal, inputVertical);
            bool hasMovementInput = inputDir.sqrMagnitude > 0.01f;
            
            // Estado en el suelo
            bool grounded = cc.isGrounded;
            
            // CROUCH - solo cuando está en el suelo
            animator.SetBool("crouch", grounded && isCrouching);
            
            // RUN - detecta movimiento tanto normal como agachado
            bool shouldRun = grounded && hasMovementInput;
            animator.SetBool("run", shouldRun);
            
            // SPRINT - solo cuando está corriendo Y presiona sprint Y NO está agachado
            isSprinting = shouldRun && inputSprint && !isCrouching;
            animator.SetBool("sprint", isSprinting);
            
            // AIR/JUMP - cuando NO está en el suelo
            animator.SetBool("air", !grounded);
            
            // Debug (puedes eliminarlo después)
            Debug.Log($"Run: {shouldRun}, Sprint: {isSprinting}, Crouch: {isCrouching}, Air: {!grounded}, Input: {hasMovementInput}");
        }

        // Handle jump
        if (inputJump && cc.isGrounded)
        {
            isJumping = true;
        }

        HeadHittingDetect();
    }


    // With the inputs and animations defined, FixedUpdate is responsible for applying movements and actions to the player
    private void FixedUpdate()
    {
        // Sprinting velocity boost or crounching desacelerate
        float velocityAdittion = 0;
        if (isSprinting)
            velocityAdittion = sprintAdittion;
        if (isCrouching)
            velocityAdittion = -(velocity * 0.50f); // -50% velocity

        // Jump handler
        float directionY = 0;
        if (isJumping)
        {
            // Apply inertia and smoothness when climbing the jump
            directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;

            // Jump timer
            jumpElapsedTime += Time.deltaTime;
            if (jumpElapsedTime >= jumpTime)
            {
                isJumping = false;
                jumpElapsedTime = 0;
            }
        }

        // Add gravity to Y axis
        directionY = directionY - gravity * Time.deltaTime;


        // --- Character rotation and movement --- 

        Vector3 horizontalDirection = Vector3.zero;

        // Solo calcular movimiento si hay input
        if (inputHorizontal != 0 || inputVertical != 0)
        {
            // Obtener la dirección de la cámara
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            // Proyectar en el plano horizontal
            cameraForward.y = 0;
            cameraRight.y = 0;

            // Normalizar
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calcular dirección de movimiento
            Vector3 moveDirection = (cameraForward * inputVertical + cameraRight * inputHorizontal).normalized;

            // Rotar el personaje hacia donde se mueve
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.15f);
            }

            // Aplicar velocidad a la dirección de movimiento
            horizontalDirection = moveDirection * (velocity + velocityAdittion) * Time.deltaTime;
        }

        // --- End rotation ---

        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 movement = verticalDirection + horizontalDirection;
        
        cc.Move(movement);
    }


    //This function makes the character end his jump if he hits his head on something
    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;

        // Uncomment this line to see the Ray drawed in your characters head
        // Debug.DrawRay(ccCenter, Vector3.up * headHeight, Color.red);

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            jumpElapsedTime = 0;
            isJumping = false;
        }
    }

}