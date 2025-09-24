using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public float baseGravityScale = 3f;
    public float jumpHeight = 3.5f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.12f;
    public float fallGravityMultiplier = 2.0f;
    public float maxFallSpeed = -20f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Facing / Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool spriteFacesRightByDefault = true;

    private int facing = 1;

    // 🔹 Rope state (set from PlayerRopeGrabToggle)
    [HideInInspector] public bool isOnRope = false;

    private Animator anim;
    private bool isDead;
    private Rigidbody2D rb;
    private PlayerPushPull pushScript;

    float inputX;
    float coyoteCounter, bufferCounter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.gravityScale = baseGravityScale;
        pushScript = GetComponent<PlayerPushPull>();

        anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (isDead) return;
        var k = Keyboard.current;

        // horizontal input
        inputX = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) inputX -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) inputX += 1f;

        // flip visuals
        if (inputX > 0.01f) SetFacing(1);
        else if (inputX < -0.01f) SetFacing(-1);

        // grounded check
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        coyoteCounter = grounded ? coyoteTime : Mathf.Max(0f, coyoteCounter - Time.deltaTime);

        // Jump input
        bool jumpPressed = k.spaceKey.wasPressedThisFrame;
        bool jumpReleased = k.spaceKey.wasReleasedThisFrame;

        if (!isOnRope) // 🔹 block jump logic when on rope
        {
            bufferCounter = jumpPressed ? jumpBuffer : Mathf.Max(0f, bufferCounter - Time.deltaTime);

            if (bufferCounter > 0f && coyoteCounter > 0f)
            {
                float g = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
                float vJump = Mathf.Sqrt(2f * g * jumpHeight);
                rb.linearVelocityY = vJump;
                bufferCounter = 0f;
                coyoteCounter = 0f;
            }

            if (jumpReleased && rb.linearVelocityY > 0f)
                rb.linearVelocityY *= 0.5f;
        }

        // --- Animator States ---
        if (isOnRope)
        {
            anim.SetBool("isRunning", false);
            anim.SetBool("isJumping", false);
            anim.SetBool("isGrabbing", true);
        }
        else
        {
            anim.SetBool("isRunning", Mathf.Abs(inputX) > 0.01f);
            anim.SetBool("isJumping", !grounded);
            anim.SetBool("isGrabbing", false);
            anim.SetBool("isPushing", pushScript && pushScript.isPushing);
        }
    }

    void FixedUpdate()
    {
        if (!isOnRope) // 🔹 disable ground movement when on rope
        {
            rb.linearVelocityX = inputX * moveSpeed;
        }

        rb.gravityScale = (rb.linearVelocityY < 0f)
            ? baseGravityScale * fallGravityMultiplier
            : baseGravityScale;

        if (rb.linearVelocityY < maxFallSpeed)
            rb.linearVelocityY = maxFallSpeed;
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    void SetFacing(int dir)
    {
        if (dir == facing) return;
        facing = dir;

        if (spriteRenderer)
        {
            bool lookLeft = (dir == -1);
            spriteRenderer.flipX = spriteFacesRightByDefault ? lookLeft : !lookLeft;
        }
        else
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * dir;
            transform.localScale = s;
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        anim.SetTrigger("die");
    }
}
