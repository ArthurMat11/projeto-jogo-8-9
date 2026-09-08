using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    
    // Input Actions - using your PlayerActionMap asset
    private PlayerActionMap inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    
    // Movement state
    private Vector2 moveInput;
    private bool isJumping;
    private bool isGrounded;
    
    private void Awake()
    {
        // Get or add Rigidbody2D
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        
        // Initialize input actions using YOUR PlayerActionMap asset
        inputActions = new PlayerActionMap();
        
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
    }
    
    private void OnEnable()
    {
        // Enable input actions
        moveAction.Enable();
        jumpAction.Enable();
        
        // Subscribe to input events
        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from input events
        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;
        
        // Disable input actions
        moveAction.Disable();
        jumpAction.Disable();
    }
    
    private void Update()
    {
        // Read movement input from YOUR configured Move action
        moveInput = moveAction.ReadValue<Vector2>();
        
        // Check if grounded
        CheckGrounded();
        
        // Apply jump physics
        ApplyJumpPhysics();
    }
    
    private void FixedUpdate()
    {
        // Apply movement
        MovePlayer();
    }
    
    private void CheckGrounded()
    {
        if (groundCheckPoint != null)
        {
            // Ground check using circle cast at ground check point
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                groundCheckPoint.position, 
                groundCheckRadius, 
                groundLayer
            );
            isGrounded = colliders.Length > 0;
        }
        else
        {
            // Simple ground check using raycast from center
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, 
                Vector2.down, 
                1.1f,
                groundLayer
            );
            isGrounded = hit.collider != null;
        }
    }
    
    private void MovePlayer()
    {
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }
    
    private void ApplyJumpPhysics()
    {
        // Better jump physics (variable jump height)
        if (rb.linearVelocity.y < 0)
        {
            // Falling - increase gravity
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isJumping)
        {
            // Jump button released early - reduce jump height
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }
    
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            // Apply jump force
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
        }
    }
    
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isJumping = false;
    }
    
    // Visual debug for ground check
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
        else
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position, Vector2.down * 1.1f);
        }
    }
}