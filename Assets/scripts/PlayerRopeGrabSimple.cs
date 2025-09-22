using UnityEngine;
using UnityEngine.InputSystem; // NEW input system (Keyboard.current, Key)

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRopeGrabToggle : MonoBehaviour
{
    [Header("Keys (New Input System)")]
    public Key grabToggleKey = Key.E;   // press once to grab, press again to detach
    public Key detachKey     = Key.Space;

    [Header("Detection")]
    public float grabRadius = 1.2f;     // how close the player must be to a rope segment
    public LayerMask ropeMask;          // layers for rope segments (with RB2D + Collider2D)

    [Header("Swing / Feel")]
    public bool enableCollisionWithRope = false; // usually false to avoid jitter while hanging
    public float jumpOffSpeed = 0f;              // optional push on release (0 = none)
    public float pumpForce = 12f;                // A=left, D=right while hanging

    Rigidbody2D rb;
    HingeJoint2D joint;        // created on the player when grabbing
    Rigidbody2D ropeRB;        // rope segment we’re attached to
    PlayerController2D move;   // your movement script (optional, auto-found)

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        move = GetComponent<PlayerController2D>();
    }

    void Update()
    {
        var k = Keyboard.current;
        if (k == null) return;

        // Toggle with E
        if (k[grabToggleKey].wasPressedThisFrame)
        {
            if (joint == null) TryGrabNearestRope();
            else Detach();
        }

        // Optional separate detach key
        if (joint != null && k[detachKey].wasPressedThisFrame)
        {
            Detach();
        }
    }

    void FixedUpdate()
    {
        if (joint == null || ropeRB == null || pumpForce <= 0f) return;

        var k = Keyboard.current;
        float inputX = 0f;
        if (k != null)
        {
            if (k.aKey.isPressed || k.leftArrowKey.isPressed)  inputX -= 1f; // swing left
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) inputX += 1f; // swing right
        }
        if (Mathf.Abs(inputX) < 0.01f) return;

        // Tangent at the player's position around the rope pivot/segment
        Vector2 pivot = ropeRB.worldCenterOfMass;
        Vector2 toPlayer = (Vector2)transform.position - pivot;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        // Perpendicular (rotate by +90°) gives tangent direction
        Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized;

        // A = negative (left), D = positive (right)
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
            if (!segRB || segRB == rb) continue; // must be a rope segment with RB2D

            Vector2 closest = (Vector2)h.bounds.ClosestPoint(transform.position);
            Vector2 delta   = closest - (Vector2)transform.position;
            float d         = delta.sqrMagnitude;

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

        if (move) move.enabled = false; // let physics take over while hanging
    }

    void Detach()
    {
        if (!joint) return;

        // Optional: small launch along rope tangent
        if (jumpOffSpeed > 0f && ropeRB)
        {
            Vector2 pivot = ropeRB.worldCenterOfMass;
            Vector2 toPlayer = (Vector2)transform.position - pivot;
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized;
                rb.linearVelocity = tangent * jumpOffSpeed + Vector2.up * 0.25f;
            }
        }

        Destroy(joint);
        joint  = null;
        ropeRB = null;

        if (move) move.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}
