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

        playercontrols.Camera.Look.performed += OnLook;

    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }
    public bool isJumping;
    public bool isGrounded() => characterController.isGrounded;
    [SerializeField] private float jumpForce = 2f;
    public void OnJump(InputAction.CallbackContext context)
    {
        isJumping = context.ReadValueAsButton();
        if (context.started && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(-5.0f * fallGravity * jumpForce);
            //takes initial grounded gravity of -4.5 as ''5'' and ignoring fallgravity before applying.
            //to calculate amount of force required  to exit grounded gravity.
        }
    }

    // Update is called once per frame
    void Update()
    {
        ApplyGravity();
        SpeedChange();
        MoveCharacter();
        FpsCamera();
    }
    #endregion

    PlayerControls playercontrols;
    CharacterController characterController;
    private Vector3 velocity;
    private Vector3 direction;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float runSpeed = 9f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float crouchSpeed = 2.5f;
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
        else { targetSpeed = moveSpeed; }
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

    private void MoveCharacter()
    {
        direction = new Vector3(input.x, 0f, input.y).normalized;
        Vector3 movement = direction * currentSpeed;
        characterController.Move(movement * Time.deltaTime); //moves the character.
        
        
        characterController.Move(velocity * Time.deltaTime); //handles gravity calc.

    }
    [Header("Camera Settings")]
    public Transform fpsCamera;
    public float sensitivity = 2f;
    public Vector2 Look;
    public float xRotation = 0f;
    private void FpsCamera()
    {
        var MouseX = Look.x;
        var MouseY = Look.y;

        xRotation -= MouseY * sensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        fpsCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * MouseX * sensitivity * Time.deltaTime);
  
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
    public void OnLook(InputAction.CallbackContext context)
    {
        Look = context.ReadValue<Vector2>();
    }
}
