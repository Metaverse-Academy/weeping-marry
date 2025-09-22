using UnityEngine;

/// Attach to the mob/enemy root. Requires a Collider2D on the same GameObject.
/// Damages the player on side/bottom contact; ignores top (stomp) contact.
[RequireComponent(typeof(Collider2D))]
public class EnemySideDamage : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;
    [Tooltip("Seconds between hits while the player stays in contact.")]
    public float hitCooldown = 0.5f;

    [Header("Detection")]
    public string playerTag = "Player";
    [Tooltip("Player must be above enemy by this margin to count as 'from above'.")]
    public float topOffset = 0.15f;
    [Tooltip("Require the player to be moving downward to count as 'from above'.")]
    public bool requireDownwardMotion = true;

    float nextHitTime = 0f;

    // ----- Collisions (solid colliders) -----
    void OnCollisionEnter2D(Collision2D col) => TryHitFromCollision(col);
    void OnCollisionStay2D(Collision2D col)  => TryHitFromCollision(col);

    // ----- Triggers (trigger hitboxes) -----
    void OnTriggerEnter2D(Collider2D other)  => TryHitFromTrigger(other);
    void OnTriggerStay2D(Collider2D other)   => TryHitFromTrigger(other);

    void TryHitFromCollision(Collision2D col)
    {
        if (!col.collider.CompareTag(playerTag)) return;
        if (Time.time < nextHitTime) return;

        // If contact appears to be from above (stomp), do NOT deal damage.
        if (IsFromAbove(col)) return;

        // Use contact point for correct knockback direction; fallback to player position
        Vector2 contact = col.contactCount > 0 ? col.GetContact(0).point
                                               : (Vector2)col.collider.transform.position;
        DealDamage(col.collider.gameObject, contact);
    }

    void TryHitFromTrigger(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time < nextHitTime) return;

        // With triggers we don't have contact normals, so we use position + velocity test
        if (IsFromAboveApprox(other)) return;

        Vector2 contact = other.bounds.ClosestPoint(transform.position);
        DealDamage(other.gameObject, contact);
    }

    void DealDamage(GameObject playerObj, Vector2 attackerWorldPos)
    {
        var health = playerObj.GetComponentInParent<Healthmyversion>();
        if (health == null) return;

        health.TakeDamageFrom(damage, attackerWorldPos);
        nextHitTime = Time.time + hitCooldown;
    }

    // --- Helpers ---
    bool IsFromAbove(Collision2D col)
    {
        var playerTr = col.transform;
        var playerRb = col.rigidbody;
        bool playerAbove = playerTr.position.y > transform.position.y + topOffset;

        bool movingDown = !requireDownwardMotion || (playerRb && playerRb.linearVelocityY <= 0f);

        // Contact normals: if any normal points mostly DOWN (normal.y < -0.5)
        // it means the player contacted us from ABOVE.
        bool topContact = false;
        for (int i = 0; i < col.contactCount; i++)
        {
            if (col.GetContact(i).normal.y < -0.5f) { topContact = true; break; }
        }

        return movingDown && (playerAbove || topContact);
    }

    bool IsFromAboveApprox(Collider2D other)
    {
        var playerTr = other.transform;
        var playerRb = other.attachedRigidbody;

        bool playerAbove = playerTr.position.y > transform.position.y + topOffset;
        bool movingDown = !requireDownwardMotion || (playerRb && playerRb.linearVelocityY <= 0f);

        return playerAbove && movingDown;
    }
}

