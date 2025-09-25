using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRopeGrabToggle : MonoBehaviour
{
    [Header("Keys (New Input System)")]
    public Key grabToggleKey = Key.E;   // press once to grab, press again to detach
    public Key detachKey     = Key.Space;

    [Header("Detection")]
    public float grabRadius = 1.2f;
    public LayerMask ropeMask;

    [Header("Swing / Feel")]
    public bool enableCollisionWithRope = false;
    public float pumpForce = 12f;                // A/D while hanging

    [Header("Detach Launch")]
    [Tooltip("Guarantee at least this speed along the tangent when releasing.")]
    public float minTangentLaunch = 8f;
    [Tooltip("Extra upward boost when releasing (adds to your current vertical).")]
    public float upLaunch = 6f;
    [Tooltip("Also add the rope segment's velocity to the launch (helps long gaps).")]
    public bool carryRopeVelocity = true;
    [Tooltip("If no swing momentum, use input (A/D) to choose left/right on release.")]
    public bool useInputForZeroTangent = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string grabTrigger = "Grab";
    [SerializeField] private string releaseTrigger = "Release";
    [SerializeField] private string swingingBool = "IsSwinging";

    Rigidbody2D rb;
    HingeJoint2D joint;
    Rigidbody2D ropeRB;
    PlayerController2D move;

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        move = GetComponent<PlayerController2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        var k = Keyboard.current;
        if (k == null) return;

        if (k[grabToggleKey].wasPressedThisFrame)
        {
            if (joint == null) TryGrabNearestRope();
            else Detach();
        }

        if (joint != null && k[detachKey].wasPressedThisFrame)
            Detach();
    }

    void FixedUpdate()
    {
        if (joint == null || ropeRB == null || pumpForce <= 0f) return;

        var k = Keyboard.current;
        float inputX = 0f;
        if (k != null)
        {
            if (k.aKey.isPressed || k.leftArrowKey.isPressed)  inputX -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) inputX += 1f;
        }

        if (Mathf.Abs(inputX) < 0.01f) return;

        Vector2 pivot = ropeRB.worldCenterOfMass;
        Vector2 toPlayer = (Vector2)transform.position - pivot;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized;
        rb.AddForce(tangent * (pumpForce * inputX), ForceMode2D.Force);
    }

    void TryGrabNearestRope()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, grabRadius, ropeMask);
        if (hits == null || hits.Length == 0) return;

        Collider2D best = null;
        float bestDist = float.PositiveInfinity;

        foreach (var h in hits)
        {
            var segRB = h.attachedRigidbody;
            if (!segRB || segRB == rb) continue;
            Vector2 closest = (Vector2)h.bounds.ClosestPoint(transform.position);
            float d = (closest - (Vector2)transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = h; }
        }
        if (!best) return;

        ropeRB = best.attachedRigidbody;

        joint = gameObject.AddComponent<HingeJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = ropeRB;
        joint.enableCollision = enableCollisionWithRope;
        joint.anchor = Vector2.zero;

        Vector2 connectWorld = best.bounds.ClosestPoint(transform.position);
        joint.connectedAnchor = ropeRB.transform.InverseTransformPoint(connectWorld);

        if (move) move.enabled = false; // pause controller while hanging

        // 🔹 Animation hooks
        if (animator)
        {
            if (!string.IsNullOrEmpty(grabTrigger)) animator.SetTrigger(grabTrigger);
            if (!string.IsNullOrEmpty(swingingBool)) animator.SetBool(swingingBool, true);
        }
    }

    void Detach()
    {
        if (!joint) return;

        // Compute tangent direction at release
        Vector2 tangent = Vector2.right;
        float alongTangent = 0f;

        if (ropeRB)
        {
            Vector2 pivot = ropeRB.worldCenterOfMass;
            Vector2 toPlayer = (Vector2)transform.position - pivot;
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized;
                alongTangent = Vector2.Dot(rb.linearVelocity, tangent);
            }
        }

        if (Mathf.Abs(alongTangent) < 0.1f && useInputForZeroTangent)
        {
            var k = Keyboard.current;
            float dir = 0f;
            if (k != null)
            {
                if (k.aKey.isPressed || k.leftArrowKey.isPressed)  dir -= 1f;
                if (k.dKey.isPressed || k.rightArrowKey.isPressed) dir += 1f;
            }
            if (Mathf.Abs(dir) > 0.01f)
                alongTangent = Mathf.Sign(dir) * 0.1f;
            else
                alongTangent = 1f;
        }

        float sign = Mathf.Sign(alongTangent);
        float currentSpeedAlong = Mathf.Abs(alongTangent);
        float launchSpeed = Mathf.Max(currentSpeedAlong, minTangentLaunch);

        Vector2 v = tangent * (launchSpeed * sign) + Vector2.up * upLaunch;
        if (carryRopeVelocity && ropeRB)
            v += ropeRB.linearVelocity * 0.35f;

        rb.linearVelocity = v;

        Destroy(joint);
        joint  = null;
        ropeRB = null;

        if (move) StartCoroutine(ReenableMoveNextFixed());

        // 🔹 Animation hooks
        if (animator)
        {
            if (!string.IsNullOrEmpty(releaseTrigger)) animator.SetTrigger(releaseTrigger);
            if (!string.IsNullOrEmpty(swingingBool)) animator.SetBool(swingingBool, false);
        }
    }

    System.Collections.IEnumerator ReenableMoveNextFixed()
    {
        yield return new WaitForFixedUpdate();
        if (move) move.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}
