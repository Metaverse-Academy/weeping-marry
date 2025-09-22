using UnityEngine;
using UnityEngine.InputSystem; // new input

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSwingGrab : MonoBehaviour
{
    [Header("Grab Settings")]
    public Key grabKey = Key.E;        // <-- Key (not KeyCode)
    public Key detachKey = Key.Space;  // <-- Key (not KeyCode)
    public float grabRadius = 1.2f;
    public LayerMask swingLayer;

    [Header("Swing Feel")]
    public float pumpForce = 10f;
    public float jumpOffSpeed = 7f;
    public bool enablePlayerCollisionWithSwing = false;

    Rigidbody2D rb;
    HingeJoint2D swingJoint;
    Rigidbody2D grabbedSwingRB;
    PlayerController2D moveScript;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveScript = GetComponent<PlayerController2D>();
    }

    void Update()
    {
        var k = Keyboard.current;

        // hold to grab
        if (k != null && k[grabKey].isPressed)
        {
            if (swingJoint == null) TryGrabSwing();
        }
        else
        {
            if (swingJoint != null) Detach(applyJump: true);
        }

        if (swingJoint != null && k != null && k[detachKey].wasPressedThisFrame)
            Detach(applyJump: true);
    }

    void FixedUpdate()
    {
        if (swingJoint == null || grabbedSwingRB == null) return;

        float inputX = 0f;
        var k = Keyboard.current;
        if (k != null)
        {
            if (k.aKey.isPressed || k.leftArrowKey.isPressed)  inputX -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) inputX += 1f;
        }

        if (Mathf.Abs(inputX) > 0.01f)
        {
            Vector2 pivot = swingJoint.connectedBody.worldCenterOfMass;
            Vector2 toPlayer = (Vector2)transform.position - pivot;
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized;
                rb.AddForce(tangent * (pumpForce * inputX), ForceMode2D.Force);
            }
        }
    }

    void TryGrabSwing()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, grabRadius, swingLayer);
        if (!hit) return;

        var swingRb = hit.attachedRigidbody;
        if (!swingRb) return;

        grabbedSwingRB = swingRb;

        swingJoint = gameObject.AddComponent<HingeJoint2D>();
        swingJoint.autoConfigureConnectedAnchor = false;
        swingJoint.connectedBody = grabbedSwingRB;
        swingJoint.enableCollision = enablePlayerCollisionWithSwing;
        swingJoint.anchor = Vector2.zero;

        Vector2 connectPoint = grabbedSwingRB.transform.InverseTransformPoint(
            hit.bounds.ClosestPoint(transform.position)
        );
        swingJoint.connectedAnchor = connectPoint;

        if (moveScript) moveScript.enabled = false;
    }

    void Detach(bool applyJump)
    {
        if (!swingJoint) return;

        Vector2 tangent = Vector2.right;
        if (grabbedSwingRB)
        {
            Vector2 pivot = grabbedSwingRB.worldCenterOfMass;
            Vector2 toPlayer = (Vector2)transform.position - pivot;
            if (toPlayer.sqrMagnitude > 0.0001f)
                tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized;
        }

        Destroy(swingJoint);
        swingJoint = null;

        if (moveScript) moveScript.enabled = true;

        if (applyJump)
            rb.linearVelocity = tangent * jumpOffSpeed + Vector2.up * 0.5f;

        grabbedSwingRB = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}
