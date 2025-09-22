using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRopeGrabToggle : MonoBehaviour
{
    [Header("Keys (New Input System)")]
    public Key grabToggleKey = Key.R;   // press once to grab, press again to detach
    public Key detachKey     = Key.Space;

    [Header("Detection")]
    public float grabRadius = 1.2f;     
    public LayerMask ropeMask;          

    [Header("Swing / Feel")]
    public bool enableCollisionWithRope = false; 
    public float jumpOffSpeed = 0f;              
    public float pumpForce = 12f;                

    Rigidbody2D rb;
    HingeJoint2D joint;        
    Rigidbody2D ropeRB;        
    PlayerController2D move;   
    Animator anim;

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        move = GetComponent<PlayerController2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        var k = Keyboard.current;
        if (k == null) return;

        // Toggle with R
        if (k[grabToggleKey].wasPressedThisFrame)
        {
            if (joint == null) TryGrabNearestRope();
            else Detach();
        }

        // Detach with Space
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
            if (k.aKey.isPressed || k.leftArrowKey.isPressed)  inputX -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) inputX += 1f;
        }
        if (Mathf.Abs(inputX) < 0.01f) return;

        // swing force
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

        if (move) move.isOnRope = true;
        if (anim) anim.SetBool("isGrabbing", true);
    }

    void Detach()
    {
        if (!joint) return;

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

        if (move) move.isOnRope = false;
        if (anim) anim.SetBool("isGrabbing", false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}
