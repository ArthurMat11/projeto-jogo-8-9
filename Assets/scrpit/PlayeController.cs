using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Dash Settings")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    
    private bool canDash = true;
    private bool isDashing;
    private float dashDirection = 1f;
    private float facingDirection = 1f; // 1 = direita, -1 = esquerda

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Coin Settings")]
    private int coinCounter = 0;
    [SerializeField] private TextMeshProUGUI coinText;

    // Input Actions
    private PlayerActionMap inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;

    // Movement state
    private Vector2 moveInput;
    private bool isJumping;
    private bool isGrounded;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        inputActions = new PlayerActionMap();
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();

        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;

        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        // Leitura da movimentação
        moveInput = moveAction.ReadValue<Vector2>();

        // Atualiza a direção que o personagem está virado (baseado na movimentação ou na escala)
        if (moveInput.x != 0)
        {
            facingDirection = Mathf.Sign(moveInput.x);
        }
        else if (transform.localScale.x != 0)
        {
            facingDirection = Mathf.Sign(transform.localScale.x);
        }

        // Se estiver no Dash, ignora verificações normais de pulo
        if (isDashing) return;

        CheckGrounded();
        ApplyJumpPhysics();

        // Leitura do teclado para o Dash
        if (Keyboard.current != null)
        {
            bool shiftPressed = Keyboard.current.leftShiftKey.wasPressedThisFrame || 
                               Keyboard.current.rightShiftKey.wasPressedThisFrame;

            if (shiftPressed && canDash)
            {
                StartCoroutine(PerformDash());
            }
        }
    }

    private void FixedUpdate()
    {
        // Se estiver no meio do Dash, força a velocidade do Dash continuamente no FixedUpdate
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            return;
        }

        MovePlayer();
    }

    private void CheckGrounded()
    {
        if (groundCheckPoint != null)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                groundCheckPoint.position,
                groundCheckRadius,
                groundLayer
            );
            isGrounded = colliders.Length > 0;
        }
        else
        {
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
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isJumping)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // Salva a gravidade original e desativa temporariamente para o dash ser retilíneo
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Define a direção do dash no início da corrotina
        dashDirection = moveInput.x != 0 ? Mathf.Sign(moveInput.x) : facingDirection;

        // Se por algum motivo a direção ainda for 0, força para a direita (1)
        if (dashDirection == 0) dashDirection = 1f;

        // Espera o tempo de duração do dash (durante esse tempo, o FixedUpdate estará aplicando a velocidade)
        yield return new WaitForSeconds(dashDuration);

        // Restaura gravidade e encerra o estado de dash
        rb.gravityScale = originalGravity;
        isDashing = false;

        // Tempo de recarga do dash
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isJumping = false;
    }

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

    public void ChangeTextCoin()
    {
        coinCounter += 1;
        if (coinText != null) coinText.text = coinCounter.ToString();
    }

    public void Die()
    {
        Destroy(gameObject);
        ReloadCurrentScene();
    }

    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadSceneAsync(currentSceneName);
    }
}