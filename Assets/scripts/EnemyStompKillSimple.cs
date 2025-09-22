using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyStompKillSimple : MonoBehaviour
{
    [Header("Stomp")]
    public string playerTag = "Player";
    public float stompBounceForce = 12f;   // how high the player bounces
    [Tooltip("How far above the enemy's center counts as 'top' (helps reject side hits).")]
    public float topOffset = 0.15f;

    [Header("Squish Death")]
    public float squishTime = 0.12f;       // time to squash
    public Vector2 squishScale = new Vector2(1.2f, 0.25f); // x wider, y flatter
    public bool disableColliderOnDeath = true;

    Rigidbody2D rbEnemy;
    Collider2D colEnemy;
    bool dead;

    void Awake()
    {
        rbEnemy = GetComponent<Rigidbody2D>(); // optional
        colEnemy = GetComponent<Collider2D>();
        if (rbEnemy) rbEnemy.freezeRotation = true;
        if (colEnemy) colEnemy.isTrigger = false; // should be a solid collider
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (dead) return;
        if (!collision.collider.CompareTag(playerTag)) return;

        var playerRb = collision.rigidbody; // player's Rigidbody2D
        if (playerRb == null) return;

        // --- Is this a stomp? ---
        // 1) player moving downward (<= 0)
        bool playerFalling = playerRb.linearVelocityY <= 0f;

        // 2) player is above enemy by some margin
        bool playerAbove = collision.transform.position.y >
                           transform.position.y + topOffset;

        // 3) contact normal pointing mostly down (means hit from above)
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

            // Kill enemy with squish
            StartCoroutine(SquishAndDie());
            // IMPORTANT: do nothing else (don’t damage player here)
        }
        // else: do nothing — your enemy’s other script (EnemyDamageDealer2D)
        // can still hurt the player on side/bottom contact.
    }

    IEnumerator SquishAndDie()
    {
        dead = true;

        // Stop interacting while squishing
        if (rbEnemy)
        {
            rbEnemy.linearVelocity = Vector2.zero;
            rbEnemy.isKinematic = true;
        }
        if (disableColliderOnDeath && colEnemy) colEnemy.enabled = false;

        // Squish animation
        Vector3 start = transform.localScale;
        Vector3 end = new Vector3(start.x * squishScale.x, start.y * squishScale.y, start.z);

        float t = 0f;
        while (t < squishTime)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, end, t / squishTime);
            yield return null;
        }

        // tiny delay to let the squish be seen
        yield return new WaitForSeconds(0.06f);

        Destroy(gameObject);
    }
}

