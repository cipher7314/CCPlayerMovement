using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("debug data feedback")]
    PlayerControls playercontrols;
    CharacterController controller;
    private Vector3 velocity; //gravity is a constant
    private Vector3 direction; //movement direction in horizontal plane
    private float currentSpeed; //variable for speedchange func block
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float speedChangeRate = 1f;
    [SerializeField] private float airSpeedChangeRate = 5f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 1.2f;

    [Header("Gravity settings")]
    [SerializeField] private float groundGravity = -4.5f;
    [SerializeField] private float fallGravity = -10f; 
    [SerializeField] private float terminalVelocity = -50f;
    //[SerializeField] private float airMultiplier = 0.8f;

    [Header("Input Actions = Keys")]
    public Vector2 input;
    public bool Running; 
    public bool Walking;
    public bool Crouching;
    public bool Jumping;
    public bool Grounded => controller.isGrounded;
    public bool CrouchOverHead;

    [Header("crouching settings")]
    public float heightAdjustRate = 10f;
    public float crouchHeight = 0.7f;
    public float standHeight = 2.0f;

    [Header("Camera Settings")]
    public Transform fpsCamera;
    public Vector2 look;
    public float sensitivity = 400f;
    public float xRotation = 0f;
    // there's a secret variable inside inputAction map under look>processor>XnY values 0.05


    #region Awake / update and functions
    private void Awake()
    {
        playercontrols = new PlayerControls();
        controller = GetComponent<CharacterController>();

        //playercontrols.Player.Look.performed += OnLook; buggy read raw values instead

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
        playercontrols.Player.Jump.canceled += OnJump;

    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controller.height = standHeight;

    }

    // Update is called once per frame
    void Update()
    {

        HandleJump();
        controller.Move(velocity * Time.deltaTime); //handles gravity calc.
        ApplyGravity();
        SpeedChange();
        //MoveCharacter();
        FpsCamera();
        FirstPersonMovement();
        CrouchHandler();
        

    }
    #endregion

    private void CrouchHandler()
    {   
        if (Crouching)
        {
            StartCoroutine(Crouch());
        }
    }

    private IEnumerator Crouch()
    {
        float targetHeight = Crouching ? crouchHeight : standHeight;
        // Check if crouching and there's an obstruction above
        if (Crouching && Physics.Raycast(controller.transform.position, Vector3.up, 1f))
        {
            CrouchOverHead = true;
            // If there's an obstruction, keep the crouch state and set targetSpeed to crouchSpeed
            targetHeight = crouchHeight;
            yield return null; // Continue checking for obstruction
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            CrouchOverHead = false;
            // If there's no obstruction, update to standing state
            targetHeight = standHeight;
        }
        
        float currentHeight = controller.height;
        // Adjust height
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, heightAdjustRate * Time.deltaTime);
        controller.height = currentHeight;

        // Ensure the final value is set
        controller.height = targetHeight;
    }
    private void HandleJump()
    {    if (Jumping && Grounded)
        {
            velocity.y = Mathf.Sqrt(-4.5f * fallGravity * jumpForce);
            //takes initial grounded gravity of -4.5 as ''5'' and ignoring fallgravity before applying.
            //to calculate amount of force required  to exit grounded gravity.
        }
    }

    private void SpeedChange()
    {   
        if(Grounded)
        {
        float targetSpeed;
        if (Walking) { targetSpeed = walkSpeed; }
        else if (Running) { targetSpeed = runSpeed; }
        else if (Crouching && CrouchOverHead) { targetSpeed = crouchSpeed; } // hopefully CrouchOverHead works
        else { targetSpeed = input.magnitude > 0.1f ? moveSpeed : 0f; }  
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate); 
        }   
        else {currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, Time.deltaTime * airSpeedChangeRate);}
        // Smoothly transition to air speed when not grounded //airMultiplier * 
    }

    private void ApplyGravity()
    {
        // Apply ground gravity when character controller is grounded
        if (Grounded && velocity.y < 0)
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
        controller.Move(movement * Time.deltaTime);
        }
    }

    //private void MoveCharacter()
    //{   
    //    direction = new Vector3(input.x, 0f, input.y).normalized;
    //    Vector3 movement = direction * currentSpeed;
    //    controller.Move(movement * Time.deltaTime); //moves the character.
    //
    //
    //}

    private void FpsCamera()
    {
        look = playercontrols.Player.Look.ReadValue<Vector2>();
        float mouseX = look.x;
        float mouseY = look.y;

        xRotation -= mouseY * sensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -85, 85); //looking straight down stops movement?

        fpsCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX * sensitivity * Time.deltaTime);
    }

    private void OnEnable()
    {
        playercontrols.Player.Enable();
    }
    private void OnDisable()
    {
        playercontrols.Player.Disable();
    }
    //public void OnLook(InputAction.CallbackContext context)
    //{
    //    look = context.ReadValue<Vector2>(); // semi redundant read raw values instead
    //}
    public void OnMove(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }
    public void OnRun(InputAction.CallbackContext context)
    {
        Running = context.ReadValueAsButton();
    }
    public void OnWalk(InputAction.CallbackContext context)
    {
        Walking = context.ReadValueAsButton();
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {
        Crouching = context.ReadValueAsButton();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        Jumping = context.ReadValueAsButton();
    }

}
