using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Movement

    [Header("Movement values")]
    PlayerControls playercontrols;
    CharacterController characterController;
    private Vector3 velocity;
    private Vector3 direction;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] public float runSpeed = 9f;
    [SerializeField] public float walkSpeed = 4f;
    [SerializeField] public float crouchSpeed = 3f;

    private float currentSpeed;
    [SerializeField] public float speedChangeRate = 1.7f;

    public Vector2 input;
    public bool isRunning; 
    public bool isWalking;
    public bool isCrouching;


    #endregion

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

    public bool isJumping;
    public bool isGrounded() => characterController.isGrounded;
    [SerializeField] private float jumpheight = 8f;
        public void OnJump(InputAction.CallbackContext context)
    {
        isJumping = context.ReadValueAsButton();
        if (context.started && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(-5.0f * fallGravity * jumpheight);
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
        
    }
    #endregion

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
        else // Smoothly transition to air speed when not grounded
        {currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed * airMultiplier, Time.deltaTime * speedChangeRate);}
        
        
        //{ currentSpeed = moveSpeed * airMultiplier;} old values


        //Smoothly transition to air speed when not grounded
        //currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed * airMultiplier, Time.deltaTime * speedChangeRate);
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
            // Apply air gravity when not grounded and multiply by 2/per second
            velocity.y += fallGravity * 2 * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, terminalVelocity);
        }
        //characterController.Move(velocity * Time.deltaTime);
        //this breaks gravity when in here. idk why. moved below to MoveCharacter now it works.
    }

    private void MoveCharacter()
    {
        direction = new Vector3(input.x, 0f, input.y).normalized;
        Vector3 movement = direction * currentSpeed;

        characterController.Move(movement * Time.deltaTime);
        characterController.Move(velocity * Time.deltaTime);

    }




}
