using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAudioSimple : MonoBehaviour
{
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Clips")]
    public AudioClip footstepsLoopClip; // <-- loops while walking
    public AudioClip jumpClip;          // <-- one-shot when leaving ground upwards

    [Header("Sources")]
    public AudioSource footstepsSource; // loop source for footsteps
    public AudioSource sfxSource;       // one-shots (jump)
    public AudioSource cryingSource;    // loop (always)
    public AudioSource bgmSource;       // loop (always)

    [Header("Tuning")]
    public float moveSpeedThreshold = 0.1f; // how fast counts as moving
    public bool muteFootstepsInAir = true;  // true = stop loop when airborne

    Rigidbody2D rb;
    bool wasGrounded;
    float lastVelY;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Auto-create sources if missing
        if (!footstepsSource)
        {
            var go = new GameObject("Footsteps Source");
            go.transform.SetParent(transform, false);
            footstepsSource = go.AddComponent<AudioSource>();
            footstepsSource.playOnAwake = false;
            footstepsSource.loop = true;
            footstepsSource.spatialBlend = 0f; // 2D sound
        }
        if (!sfxSource)
        {
            var go = new GameObject("SFX Source");
            go.transform.SetParent(transform, false);
            sfxSource = go.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }

        // Assign the loop clip (if set)
        if (footstepsLoopClip) footstepsSource.clip = footstepsLoopClip;

        // Start always-on loops
        if (cryingSource && !cryingSource.isPlaying) { cryingSource.loop = true; cryingSource.Play(); }
        if (bgmSource    && !bgmSource.isPlaying)    { bgmSource.loop    = true; bgmSource.Play(); }
    }

    void FixedUpdate()
    {
        // Grounded?
        bool grounded = groundCheck
            ? Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)
            : Mathf.Abs(rb.linearVelocityY) < 0.01f; // fallback

        // Moving on ground?
        float speedX = Mathf.Abs(rb.linearVelocityX);
        bool movingOnGround = grounded && speedX > moveSpeedThreshold;

        // --- Looping footsteps control ---
        if (footstepsSource && footstepsLoopClip)
        {
            bool shouldPlay = movingOnGround;
            if (!grounded && muteFootstepsInAir) shouldPlay = false;

            if (shouldPlay && !footstepsSource.isPlaying)
                footstepsSource.Play();
            else if (!shouldPlay && footstepsSource.isPlaying)
                footstepsSource.Stop();
        }

        // --- Jump one-shot (when leaving ground, upwards) ---
        bool leftGround = wasGrounded && !grounded;
        if (leftGround && lastVelY > 0.01f && sfxSource && jumpClip)
            sfxSource.PlayOneShot(jumpClip);

        wasGrounded = grounded;
        lastVelY    = rb.linearVelocityY;
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
