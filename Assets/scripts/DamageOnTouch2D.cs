using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageOnTouch2D : MonoBehaviour
{
    public int damage = 10;
    public bool debugLogs = true;

    void Reset()
    {
        // Triggers fire when something with a Rigidbody2D enters
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (debugLogs)
            Debug.Log($"[DamageOnTouch2D] {name} triggered by {other.name} (tag:{other.tag}, layer:{LayerMask.LayerToName(other.gameObject.layer)})");

        // Prefer your new Health component
        var hmv = other.GetComponent<Healthmyversion>();
        if (hmv != null)
        {
            hmv.TakeDamageFrom(damage, (Vector2)transform.position);
            if (debugLogs) Debug.Log($"[DamageOnTouch2D] Damaged {other.name} for {damage}");
            return;
        }

        // Fallback to any IDamageable (no knockback direction)
        var dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            if (debugLogs) Debug.Log($"[DamageOnTouch2D] Damaged (IDamageable) {other.name} for {damage}");
            return;
        }

        if (debugLogs)
            Debug.LogWarning($"[DamageOnTouch2D] {other.name} has no Healthmyversion or IDamageable.");
    }
}

