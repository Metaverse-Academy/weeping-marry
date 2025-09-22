using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HazardDamage : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;
    [Tooltip("0 = once on touch; >0 = repeat every X seconds while in range")]
    public float repeatInterval = 0f;

    [Header("Detection")]
    public bool useProximity = false;
    public float proximityRadius = 0.6f;
    public Transform proximityOrigin;
    public LayerMask playerMask;    // leave 0 to check all layers + tag
    public string playerTag = "Player";

    [Header("Knockback (used for fallback path)")]
    public float knockbackX = 6f;
    public float knockbackY = 4f;

    float nextDamageTime;
    bool hasHitOnce;
    Collider2D col;

    void Reset()
    {
        col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        proximityOrigin = transform;
    }

    void Awake()
    {
        if (!proximityOrigin) proximityOrigin = transform;
        col = GetComponent<Collider2D>();
        if (!useProximity && col != null) col.isTrigger = true;
    }

    void Update()
    {
        if (!useProximity) return;
        if (repeatInterval > 0f && Time.time < nextDamageTime) return;

        Vector2 pos = proximityOrigin ? (Vector2)proximityOrigin.position : (Vector2)transform.position;
        int mask = (playerMask.value == 0) ? Physics2D.AllLayers : playerMask.value;
        Collider2D hit = Physics2D.OverlapCircle(pos, proximityRadius, mask);

        if (hit && IsPlayer(hit))
            TryDamage(hit);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (useProximity) return;
        TryDamage(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (useProximity) return;
        if (repeatInterval <= 0f) return;
        if (Time.time >= nextDamageTime)
            TryDamage(other);
    }

    bool IsPlayer(Collider2D c)
    {
        if (playerMask.value != 0)
            return ((1 << c.gameObject.layer) & playerMask.value) != 0;
        return c.CompareTag(playerTag);
    }

    void TryDamage(Collider2D target)
    {
        if (!IsPlayer(target)) return;

        // 1) Preferred: your new component with directional knockback API
        var hmv = target.GetComponent<Healthmyversion>();
        if (hmv != null)
        {
            hmv.TakeDamageFrom(damage, (Vector2)transform.position);
            AfterHit();
            return;
        }

        // 2) Fallback: any IDamageable + apply knockback here
        var dmg = target.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);

            var rb = target.attachedRigidbody;
            if (rb)
            {
                float dir = Mathf.Sign(target.transform.position.x - transform.position.x); // push away
                Vector2 v = rb.linearVelocity;
                v.x = knockbackX * dir;
                v.y = Mathf.Max(v.y, knockbackY);
                rb.linearVelocity = v;
            }

            AfterHit();
        }
    }

    void AfterHit()
    {
        if (repeatInterval > 0f)
            nextDamageTime = Time.time + repeatInterval;
        else
            enabled = true; // keep enabled; change to false if you want single-use hazards
    }

    void OnDrawGizmosSelected()
    {
        if (!useProximity) return;
        Gizmos.color = Color.red;
        Vector3 pos = proximityOrigin ? proximityOrigin.position : transform.position;
        Gizmos.DrawWireSphere(pos, proximityRadius);
    }
}

