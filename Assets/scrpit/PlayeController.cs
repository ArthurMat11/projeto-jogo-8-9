
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
    private float facingDirection = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation Settings")]
    [SerializeField] private string walkingParameter = "isWalking";
    [SerializeField] private string onGroundParameter = "OnGround";

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

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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
        // Leitura da movimentacao
        moveInput = moveAction.ReadValue<Vector2>();

        // Atualiza a direcao do personagem
        if (moveInput.x > 0.01f)
        {
            facingDirection = 1f;
            FlipPlayer(false);
        }
        else if (moveInput.x < -0.01f)
        {
            facingDirection = -1f;
            FlipPlayer(true);
        }

        // Verifica se o personagem esta no chao
        CheckGrounded();

        // Atualiza as animacoes
        UpdateAnimations();

        // Durante o dash, ignora as verificacoes restantes
        if (isDashing)
            return;

        ApplyJumpPhysics();

        // Teclas de dash
        if (Keyboard.current != null)
        {
            bool shiftPressed =
                Keyboard.current.leftShiftKey.wasPressedThisFrame ||
                Keyboard.current.rightShiftKey.wasPressedThisFrame;

            if (shiftPressed && canDash)
            {
                StartCoroutine(PerformDash());
            }
        }
    }

    private void FlipPlayer(bool faceLeft)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = faceLeft;
        }
    }

    private void FixedUpdate()
    {
        // Movimento do dash
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(
                dashDirection * dashSpeed,
                0f
            );

            return;
        }

        MovePlayer();
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        // Ativa a caminhada ao pressionar uma direcao horizontal
        bool walking = Mathf.Abs(moveInput.x) > 0.01f;

        // Atualiza os parametros do Animator
        animator.SetBool(walkingParameter, walking);
        animator.SetBool(onGroundParameter, isGrounded);
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
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                (fallMultiplier - 1) *
                Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isJumping)
        {
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                (lowJumpMultiplier - 1) *
                Time.deltaTime;
        }
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // Salva a gravidade original
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Define a direcao do dash
        dashDirection = moveInput.x != 0
            ? Mathf.Sign(moveInput.x)
            : facingDirection;

        if (dashDirection == 0)
            dashDirection = 1f;

        // Vira o personagem na direcao do dash
        FlipPlayer(dashDirection < 0);

        yield return new WaitForSeconds(dashDuration);

        // Restaura a gravidade
        rb.gravityScale = originalGravity;
        isDashing = false;

        // Tempo de recarga
        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded && !isDashing)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isJumping = true;
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isJumping = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;

        if (groundCheckPoint != null)
        {
            Gizmos.DrawWireSphere(
                groundCheckPoint.position,
                groundCheckRadius
            );
        }
        else
        {
            Gizmos.DrawRay(
                transform.position,
                Vector2.down * 1.1f
            );
        }
    }

    public void ChangeTextCoin()
    {
        coinCounter += 1;

        if (coinText != null)
            coinText.text = coinCounter.ToString();
    }

    public void Die()
    {
        Destroy(gameObject);
        ReloadCurrentScene();
    }

    public void ReloadCurrentScene()
    {
        string currentSceneName =
            SceneManager.GetActiveScene().name;

        SceneManager.LoadSceneAsync(currentSceneName);
    }
}
