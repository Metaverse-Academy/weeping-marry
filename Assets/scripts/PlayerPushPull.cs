using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPushPull : MonoBehaviour
{
    [Header("Detect grabbables")]
    public Transform grabOrigin;
    public float grabRadius = 0.35f;
    public LayerMask grabbableMask;

    [Header("Behavior")]
    public float breakDistance = 2.0f;
    public bool allowCollisionWhileGrabbed = true;

    [Header("Idle boxes")]
    // 🔹 NEW: when true, boxes are Kinematic when not grabbed (so player won't push them)
    public bool boxesKinematicUntilGrabbed = true;

    FixedJoint2D joint;
    Rigidbody2D grabbedRb;

    int playerLayer;
    int grabbableLayer;

    // remember original settings so we can restore if needed
    RigidbodyType2D grabbedOriginalType;
    bool grabbedOriginalFreezeRot;
    float grabbedOriginalGravity;

    void Awake()
    {
        playerLayer    = LayerMask.NameToLayer("Player");
        grabbableLayer = LayerMask.NameToLayer("Grabbable");

        // By default, ignore Player<->Grabbable so walking into crates won't push them
        if (playerLayer >= 0 && grabbableLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, grabbableLayer, true);
    }

    void Update()
    {
        var k = Keyboard.current;
        if (k == null) return;

        if (k.eKey.wasPressedThisFrame)
        {
            if (grabbedRb) Release();
            else TryGrab();
        }

        if (grabbedRb && joint)
        {
            float side = Mathf.Sign(transform.localScale.x == 0 ? 1 : transform.localScale.x);
            joint.anchor = new Vector2(0.25f * side, 0f);

            if (Vector2.Distance(transform.position, grabbedRb.position) > breakDistance)
                Release();
        }
    }

    void TryGrab()
    {
        if (!grabOrigin) grabOrigin = transform;

        var hit = Physics2D.OverlapCircle(grabOrigin.position, grabRadius, grabbableMask);
        if (!hit) return;

        grabbedRb = hit.attachedRigidbody ? hit.attachedRigidbody : hit.GetComponentInParent<Rigidbody2D>();
        if (!grabbedRb) return;

        // Allow Player↔Grabbable collisions WHILE grabbing (so pushing/pulling works)
        if (playerLayer >= 0 && grabbableLayer >= 0 && allowCollisionWhileGrabbed)
            Physics2D.IgnoreLayerCollision(playerLayer, grabbableLayer, false);

        // 🔹 Store original settings
        grabbedOriginalType       = grabbedRb.bodyType;
        grabbedOriginalFreezeRot  = grabbedRb.freezeRotation;
        grabbedOriginalGravity    = grabbedRb.gravityScale;

        // 🔹 While held, make sure it’s Dynamic so physics + joint work
        if (boxesKinematicUntilGrabbed && grabbedRb.bodyType != RigidbodyType2D.Dynamic)
            grabbedRb.bodyType = RigidbodyType2D.Dynamic;

        grabbedRb.freezeRotation = true;
        grabbedRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        joint = gameObject.AddComponent<FixedJoint2D>();
        joint.connectedBody = grabbedRb;
        joint.autoConfigureConnectedAnchor = true;
        joint.enableCollision = allowCollisionWhileGrabbed;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;
    }

    public void Release()
    {
        if (joint) Destroy(joint);

        // Re-ignore collisions after dropping
        if (playerLayer >= 0 && grabbableLayer >= 0 && allowCollisionWhileGrabbed)
            Physics2D.IgnoreLayerCollision(playerLayer, grabbableLayer, true);

        if (grabbedRb)
        {
            // Stop any drift
            grabbedRb.linearVelocity = Vector2.zero;
            grabbedRb.angularVelocity = 0f;

            // 🔹 Return to Kinematic when not grabbed (or restore original if you prefer)
            grabbedRb.bodyType = boxesKinematicUntilGrabbed
                ? RigidbodyType2D.Kinematic
                : grabbedOriginalType;

            grabbedRb.freezeRotation = grabbedOriginalFreezeRot;
            grabbedRb.gravityScale   = grabbedOriginalGravity;
        }

        grabbedRb = null;
        joint = null;
    }

    void OnDisable()
    {
        // Safety: ensure state restored if object is disabled while holding something
        if (grabbedRb) Release();
    }

    void OnDrawGizmosSelected()
    {
        if (!grabOrigin) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(grabOrigin.position, grabRadius);
    }
}



