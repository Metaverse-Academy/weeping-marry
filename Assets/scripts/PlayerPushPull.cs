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
    public bool boxesKinematicUntilGrabbed = true;

    FixedJoint2D joint;
    Rigidbody2D grabbedRb;

    int playerLayer;
    int grabbableLayer;

    RigidbodyType2D grabbedOriginalType;
    bool grabbedOriginalFreezeRot;
    float grabbedOriginalGravity;

    // 🔹 New flag for PlayerController2D
    public bool isPushing { get; private set; }

    void Awake()
    {
        playerLayer    = LayerMask.NameToLayer("Player");
        grabbableLayer = LayerMask.NameToLayer("Grabbable");

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

        if (playerLayer >= 0 && grabbableLayer >= 0 && allowCollisionWhileGrabbed)
            Physics2D.IgnoreLayerCollision(playerLayer, grabbableLayer, false);

        grabbedOriginalType       = grabbedRb.bodyType;
        grabbedOriginalFreezeRot  = grabbedRb.freezeRotation;
        grabbedOriginalGravity    = grabbedRb.gravityScale;

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

        // 🔹 Start pushing
        isPushing = true;
    }

    public void Release()
    {
        if (joint) Destroy(joint);

        if (playerLayer >= 0 && grabbableLayer >= 0 && allowCollisionWhileGrabbed)
            Physics2D.IgnoreLayerCollision(playerLayer, grabbableLayer, true);

        if (grabbedRb)
        {
            grabbedRb.linearVelocity = Vector2.zero;
            grabbedRb.angularVelocity = 0f;

            grabbedRb.bodyType = boxesKinematicUntilGrabbed
                ? RigidbodyType2D.Kinematic
                : grabbedOriginalType;

            grabbedRb.freezeRotation = grabbedOriginalFreezeRot;
            grabbedRb.gravityScale   = grabbedOriginalGravity;
        }

        grabbedRb = null;
        joint = null;

        // 🔹 Stop pushing
        isPushing = false;
    }

    void OnDisable()
    {
        if (grabbedRb) Release();
    }

    void OnDrawGizmosSelected()
    {
        if (!grabOrigin) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(grabOrigin.position, grabRadius);
    }
}




