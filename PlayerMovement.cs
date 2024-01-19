using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Awake / update and functions
    private void Awake()
    {
        playercontrols = new PlayerControls();
        characterController = GetComponent<CharacterController>();

        // Set the player control input callbacks
        playercontrols.Player.Move.started += OnMove;
        playercontrols.Player.Move.canceled += OnMove;
        playercontrols.Player.Move.performed += OnMove;

        playercontrols.Player.Run.started += OnRun;
        playercontrols.Player.Run.canceled += OnRun;

        playercontrols.Player.Walk.started += OnWalk;
        playercontrols.Player.Walk.canceled += OnWalk;

        playercontrols.Player.Crouch.started += OnCrouch;
        playercontrols.Player.Crouch.canceled += OnCrouch;
        
        playercontrols.Player.Jump.started += OnJump;

    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    // Update is called once per frame
    void Update()
    {
        Jumping();
        characterController.Move(velocity * Time.deltaTime); //handles gravity calc.
        ApplyGravity();
        SpeedChange();
        //MoveCharacter();
        FpsCamera();
        FirstPersonMovement();

    }
    #endregion

    public bool isJumping;
    public bool Grounded() => characterController.isGrounded;
    [SerializeField] private float jumpForce = 2f;
    private void Jumping()
    {    if (isJumping && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(-5.0f * fallGravity * jumpForce);
            //takes initial grounded gravity of -4.5 as ''5'' and ignoring fallgravity before applying.
            //to calculate amount of force required  to exit grounded gravity.
        }
    }

    PlayerControls playercontrols;
    CharacterController characterController;
    private Vector3 velocity;
    private Vector3 direction;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float runSpeed = 9f;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float crouchSpeed = 3f;
    private float currentSpeed;
    [SerializeField] private float speedChangeRate = 1.7f;

    public Vector2 input;
    public bool isRunning; 
    public bool isWalking;
    public bool isCrouching;

    private void SpeedChange()
    {   
        if(characterController.isGrounded)
        {
        float targetSpeed;
        if (isWalking) { targetSpeed = walkSpeed; }
        else if (isRunning) { targetSpeed = runSpeed; }
        else if (isCrouching) { targetSpeed = crouchSpeed; }
        else { targetSpeed = input.magnitude > 0.1f ? moveSpeed : 0f; }  
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate); 
        }   
        else {currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed * airMultiplier, Time.deltaTime * speedChangeRate);}
        // Smoothly transition to air speed when not grounded 
    }

    [Header("Gravity settings")]
    [SerializeField] private float groundGravity = -4.5f;
    [SerializeField] private float fallGravity = -10f; 
    [SerializeField] private float terminalVelocity = -50f;
    [SerializeField] private float airMultiplier = 0.7f;

    private void ApplyGravity()
    {
        // Apply ground gravity when character controller is grounded
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = groundGravity;
        }
        else
        {
            // Apply falling gravity when not grounded and multiply by 2/per second when falling
            // limit falling velocity to a capped value, determined by terminal velocity float
            velocity.y += fallGravity * 2 * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, terminalVelocity);
        }
    }

    private void FirstPersonMovement()
    {
        if(input.magnitude > 0.1)
        {
        Vector3 forward = fpsCamera.forward;
        Vector3 right = fpsCamera.right;

        forward.y = 0f;
        right.y = 0f;

        direction = (input.x * right + input.y * forward).normalized;
        Vector3 movement = direction * currentSpeed;
        characterController.Move(movement * Time.deltaTime);
        }
    }

    //private void MoveCharacter()
    //{   
    //    direction = new Vector3(input.x, 0f, input.y).normalized;
    //    Vector3 movement = direction * currentSpeed;
    //    characterController.Move(movement * Time.deltaTime); //moves the character.
//
//
    //}
    [Header("Camera Settings")]
    public Transform fpsCamera;
    public float sensitivity = 10f;
    public Vector2 Look;
    public float xRotation = 0f;
    private void FpsCamera()
    {
        Look = playercontrols.Player.Look.ReadValue<Vector2>();
        float mouseX = Look.x;
        float mouseY = Look.y;

        xRotation -= mouseY * sensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        fpsCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX * sensitivity * Time.deltaTime);
  
    }


    private void OnEnable()
    {
        playercontrols.Enable();
    }
    private void OnDisable()
    {
        playercontrols.Disable();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }
    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }
    public void OnWalk(InputAction.CallbackContext context)
    {
        isWalking = context.ReadValueAsButton();
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = context.ReadValueAsButton();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        isJumping = context.ReadValueAsButton();
    }

}
