using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    private enum enemyState { Patrol, Chase };
    private enemyState currentState = enemyState.Patrol;

    [Header("References")]
    [SerializeField] private Transform pP1;
    [SerializeField] private Transform pP2;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed  = 6f;
    [SerializeField] private float chaseRange  = 5f;

    [Header("Visuals (optional)")]
    [SerializeField] private SpriteRenderer spriteRenderer;        // assign your mob sprite here
    [SerializeField] private bool spriteFacesRightByDefault = true; // uncheck if art faces left
    [SerializeField] private Animator animator;                    // optional; set a "Speed" float, "IsMoving" bool if you want

    private Rigidbody2D rb;
    private Transform targetPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPoint = pP1;

        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        switch (currentState)
        {
            case enemyState.Patrol:
                Patrol();
                break;
            case enemyState.Chase:
                Chase();
                break;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        currentState = (distanceToPlayer < chaseRange) ? enemyState.Chase : enemyState.Patrol;
    }

    void Patrol()
    {
        Vector2 targetPos = new Vector2(targetPoint.position.x, transform.position.y);

        // compute intended movement this frame
        Vector2 from = rb.position;
        Vector2 to   = Vector2.MoveTowards(from, targetPos, patrolSpeed * Time.deltaTime);
        float dx     = to.x - from.x;

        // flip visual toward movement
        SetFacingFromDelta(dx);
        UpdateAnim(Mathf.Abs(dx) / Mathf.Max(0.0001f, Time.deltaTime));

        rb.MovePosition(to);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
            targetPoint = (targetPoint == pP1) ? pP2 : pP1;
    }

    void Chase()
    {
        Vector2 targetPos = new Vector2(player.position.x, transform.position.y);

        Vector2 from = rb.position;
        Vector2 to   = Vector2.MoveTowards(from, targetPos, chaseSpeed * Time.deltaTime);
        float dx     = to.x - from.x;

        SetFacingFromDelta(dx);
        UpdateAnim(Mathf.Abs(dx) / Mathf.Max(0.0001f, Time.deltaTime));

        rb.MovePosition(to);
    }

    void SetFacingFromDelta(float dx)
    {
        if (Mathf.Abs(dx) < 0.001f) return; // no horizontal movement this frame

        int dir = dx > 0f ? 1 : -1;

        if (spriteRenderer)
        {
            // flipX = true means look left. Adjust for art’s default facing.
            bool lookLeft = (dir == -1);
            spriteRenderer.flipX = spriteFacesRightByDefault ? lookLeft : !lookLeft;
        }
        else
        {
            // fallback: flip the whole transform scale
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * dir;
            transform.localScale = s;
        }
    }

    void UpdateAnim(float horizontalSpeedPerSec)
    {
        if (!animator) return;

        // common setups:
        //  - "Speed" (float) to drive blend tree
        //  - "IsMoving" (bool) for simple walk/idle
        animator.SetFloat("Speed", horizontalSpeedPerSec);          // e.g., drive a blend tree
        animator.SetBool("IsMoving", horizontalSpeedPerSec > 0.01f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
