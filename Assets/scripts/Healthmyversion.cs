using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class Healthmyversion : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    public event Action<int,int> OnHealthChanged;
    public event Action OnDied;

    [Header("Knockback")]
    public float knockbackX = 6f;   // horizontal shove
    public float knockbackY = 4f;   // small hop

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // Existing API stays the same
    public void TakeDamage(int amount) => TakeDamageInternal(amount, null);

    // New: use when you know where the hit came from
    public void TakeDamageFrom(int amount, Vector2 attackerWorldPos) =>
        TakeDamageInternal(amount, attackerWorldPos);

    // All logic in one place
    void TakeDamageInternal(int amount, Vector2? attackerWorldPos)
    {
        if (amount <= 0 || CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        // ---- Knockback ----
        if (rb)
        {
            float dirX = attackerWorldPos.HasValue
                ? Mathf.Sign(transform.position.x - attackerWorldPos.Value.x)  // push away from attacker
                : -(Mathf.Sign(transform.localScale.x == 0 ? 1f : transform.localScale.x)); // opposite of facing

            Vector2 v = rb.linearVelocity;
            v.x = knockbackX * dirX;
            v.y = Mathf.Max(v.y, knockbackY);
            rb.linearVelocity = v;
        }

        if (CurrentHealth == 0)
        {
            Debug.Log($"{gameObject.name} health is zero");
            Die();
        }
    }
  

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        OnDied?.Invoke();
    }
}

