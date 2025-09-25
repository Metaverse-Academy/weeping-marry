using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BreakOnGrab : MonoBehaviour
{
    [Header("What holds this swing up")]
    [Tooltip("The joint that keeps the swing attached to the ceiling/parent. " +
             "If left empty, we try to find a Joint2D on THIS object.")]
    public Joint2D holdingJoint;

    [Header("Player detection")]
    public string playerTag = "Player";

    [Header("Behavior")]
    public float breakDelay = 0.05f;     // tiny delay for feel
    public bool addDropImpulse = true;
    public float dropImpulse = 2.5f;     // downward nudge when it breaks
    public bool debugLogs = false;

    Rigidbody2D rb;       // this bob’s RB
    bool broken;
    bool armed;           // we’ve seen a grab and scheduled a break

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!holdingJoint) holdingJoint = GetComponent<Joint2D>(); // auto-find if joint is on the bob
        if (!rb) Debug.LogError("BreakOnGrabAuto: needs a Rigidbody2D on the bob.");
        if (debugLogs && !holdingJoint) Debug.Log("BreakOnGrabAuto: No holdingJoint assigned; will only fall if a joint on this bob exists to destroy.");
    }

    void FixedUpdate()
    {
        if (broken || armed) return;
        if (rb == null) return;

        // Look for ANY HingeJoint2D whose connectedBody == THIS bob,
        // and the joint lives on a Rigidbody2D tagged 'Player'.
        var hinges = UnityEngine.Object.FindObjectsByType<HingeJoint2D>(FindObjectsSortMode.None);
        for (int i = 0; i < hinges.Length; i++)
        {
            var h = hinges[i];
            if (!h) continue;
            if (h.connectedBody != rb) continue;

            var ar = h.attachedRigidbody;
            if (ar && ar.CompareTag(playerTag))
            {
                if (debugLogs) Debug.Log("[BreakOnGrabAuto] Player hinge detected -> breaking soon", this);
                StartCoroutine(BreakSoon());
                armed = true;
                break;
            }
        }
    }

    IEnumerator BreakSoon()
    {
        if (breakDelay > 0f) yield return new WaitForSeconds(breakDelay);
        BreakNow();
    }

    void BreakNow()
    {
        if (broken) return;
        broken = true;

        if (holdingJoint)
        {
            if (debugLogs) Debug.Log("[BreakOnGrabAuto] Destroying holding joint", this);
            Destroy(holdingJoint); // detaches the swing from its support
        }
        else if (debugLogs)
        {
            Debug.LogWarning("[BreakOnGrabAuto] No holdingJoint to destroy. Assign the top joint (e.g., on Anchor) if you want the whole rope to fall.", this);
        }

        if (addDropImpulse && rb)
            rb.AddForce(Vector2.down * dropImpulse, ForceMode2D.Impulse);
    }
}


