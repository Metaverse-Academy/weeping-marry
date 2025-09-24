using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyStompKillSimple : MonoBehaviour
{
    [Header("Stomp")]
    public string playerTag = "Player";
     [Header("Audio")]
    public AudioSource audioSource;   // assign or auto-create
    public AudioClip stompClip;  
    public float stompBounceForce = 12f;   
    [Tooltip("How far above the enemy's center counts as 'top' (helps reject side hits).")]
    public float topOffset = 0.15f;

    [Header("Death Animation")]
    public Animator animator;            // assign in Inspector
    public string deathTrigger = "Die";  // name of the trigger parameter in Animator
    public float deathAnimDuration = 0.6f; // fallback wait time if no Animation Event

    public bool disableColliderOnDeath = true;

    Rigidbody2D rbEnemy;
    Collider2D colEnemy;
    bool dead;

    void Awake()
    {
        rbEnemy = GetComponent<Rigidbody2D>(); 
        colEnemy = GetComponent<Collider2D>();
        if (rbEnemy) rbEnemy.freezeRotation = true;
        if (colEnemy) colEnemy.isTrigger = false; 

        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (dead) return;
        if (!collision.collider.CompareTag(playerTag)) return;

        var playerRb = collision.rigidbody; 
        if (playerRb == null) return;

        bool playerFalling = playerRb.linearVelocityY <= 0f;
        bool playerAbove = collision.transform.position.y > transform.position.y + topOffset;

        bool topContact = false;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y < -0.5f)
            {
                topContact = true;
                break;
            }
        }

        bool isStomp = playerFalling && (playerAbove || topContact);

        if (isStomp)
        {
            // Bounce player up
            Vector2 v = playerRb.linearVelocity;
            v.y = stompBounceForce;
            playerRb.linearVelocity = v;
            
            // Play sound 
            if (stompClip && audioSource)
            audioSource.PlayOneShot(stompClip);

            // Play death sequence
            StartCoroutine(DieWithAnimation());
        }
    }

    IEnumerator DieWithAnimation()
    {
        dead = true;

        // Disable movement + collision
        if (rbEnemy)
        {
            rbEnemy.linearVelocity = Vector2.zero;
            rbEnemy.isKinematic = true;
        }
        if (disableColliderOnDeath && colEnemy) colEnemy.enabled = false;

        if (animator)
        {
            animator.SetTrigger(deathTrigger);
            // Wait for animation length (or use Animation Event instead of this)
            yield return new WaitForSeconds(deathAnimDuration);
        }
        else
        {
            // fallback: destroy immediately if no animator
            yield return null;
        }

        Destroy(gameObject);
    }

    // Optional: if you add an Animation Event at the end of the "Die" clip,
    // call this method instead of waiting a fixed time.
    public void OnDeathAnimFinished()
    {
        Destroy(gameObject);
    }
}
