using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TrapDamage2D : MonoBehaviour
{
    public int damage = 20;            // how much to hurt the player
    public string playerTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var health = other.GetComponentInParent<Healthmyversion>();
        if (health == null || health.CurrentHealth <= 0) return;

        health.TakeDamageFrom(damage, transform.position);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag(playerTag)) return;

        var health = col.collider.GetComponentInParent<Healthmyversion>();
        if (health == null || health.CurrentHealth <= 0) return;

        // use the first contact point for nicer knockback direction if available
        Vector2 hitPos = col.contactCount > 0 ? col.GetContact(0).point : (Vector2)transform.position;
        health.TakeDamageFrom(damage, hitPos);
    }
}

