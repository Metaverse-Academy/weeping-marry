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

    [Header("Audio")]
    public AudioSource audioSource;   // assign an AudioSource (or leave empty to auto-create)
    public AudioClip damageClip;      // play when taking damage
    public AudioClip deathClip; 

    [Tooltip("Assign the crying and background music AudioSources here; they'll be stopped on death.")]
    public AudioSource[] stopWhenDying;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // 2D
        }
    }

    // Existing API stays the same
    public void TakeDamage(int amount) => TakeDamageInternal(amount, null);
    public void TakeDamageFrom(int amount, Vector2 attackerWorldPos) =>
        TakeDamageInternal(amount, attackerWorldPos);

    // All logic in one place
    void TakeDamageInternal(int amount, Vector2? attackerWorldPos)
    {
        if (amount <= 0 || CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        // damage SFX
        if (CurrentHealth > 0)
        {
            if (damageClip && audioSource) audioSource.PlayOneShot(damageClip);
        }


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
            
            if (stopWhenDying != null)
            {
                for (int i = 0; i < stopWhenDying.Length; i++)
                {
                    if (stopWhenDying[i]) stopWhenDying[i].Stop();
                }
            }

            // 🔊 play death clip
            if (deathClip && audioSource)
                audioSource.PlayOneShot(deathClip);
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

