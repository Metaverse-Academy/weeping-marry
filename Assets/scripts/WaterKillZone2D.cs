using UnityEngine;

/// Kill the player instantly when they touch this water.
/// Works best if the water collider is a Trigger.
[RequireComponent(typeof(Collider2D))]
public class WaterKillZone2D : MonoBehaviour
{
    public string playerTag = "Player";

    void Reset()
    {
        // auto-set trigger for convenience
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Find the player's health script (on the same object or a parent)
        var health = other.GetComponentInParent<Healthmyversion>();
        if (health == null) return;

        // Deal exactly the remaining HP as damage so death fires immediately
        int remaining = health.CurrentHealth;
        if (remaining <= 0) return;

        // Apply damage "from" the water position (for correct knockback direction),
        // then immediately zero velocity so there is no bounce/pop.
        Vector2 waterPos = (Vector2)transform.position;
        health.TakeDamageFrom(remaining, waterPos);

        // stop any knockback the health script may have applied
        var rb = other.attachedRigidbody ?? other.GetComponentInParent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // If your water collider is NOT a trigger, also handle collision:
    void OnCollisionEnter2D(Collision2D col)
    {
        var hit = col.collider;
        if (!hit.CompareTag(playerTag)) return;

        var health = hit.GetComponentInParent<Healthmyversion>();
        if (health == null || health.CurrentHealth <= 0) return;

        Vector2 contactPos = col.contactCount > 0 ? col.GetContact(0).point : (Vector2)transform.position;
        health.TakeDamageFrom(health.CurrentHealth, contactPos);

        var rb = hit.attachedRigidbody ?? hit.GetComponentInParent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
}

