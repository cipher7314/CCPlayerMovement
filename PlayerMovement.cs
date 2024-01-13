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
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] public float runSpeed = 8f;
    [SerializeField] public float walkSpeed = 2.5f;
    [SerializeField] public float crouchSpeed = 1.5f;

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
    }

    private void Start()
    {
        velocity.y = groundGravity;


    }
    private void OnEnable()
    {
        playercontrols.Enable();
    }
    private void OnDisable()
    {
        playercontrols.Disable();
    }
    public void Input()
    {
        input = playercontrols.Player.Move.ReadValue<Vector2>();
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

    // Update is called once per frame
    void Update()
    {
        Input();
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
        //original code below before change. targetspeed variable serves next to no purpose.
        //targetSpeed = Mathf.Lerp(targetSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
        //currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate); 
        }
        else
        {
            //changes
        }
    }



    [Header("Gravity settings")]
    [SerializeField] private float groundGravity = -0.5f;
    [SerializeField] private float fallGravity = -9.81f; 
    [SerializeField] private float terminalVelocity = -50f;
    [SerializeField] private float airMultiplier = 0.7f;

    private void MoveCharacter()
    {

        direction = new Vector3(input.x, 0f, input.y).normalized;
        Vector3 movement = direction * currentSpeed;

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
        characterController.Move(movement * Time.deltaTime);
        characterController.Move(velocity * Time.deltaTime);
    }


}
