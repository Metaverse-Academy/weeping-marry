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
    [SerializeField] private float patrolSpeed = 3;
    [SerializeField] private float chaseSpeed = 6;
    [SerializeField] private float chaseRange = 5f;



    private Rigidbody2D rb;
    private Transform targetPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPoint = pP1;
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
        if (distanceToPlayer < chaseRange)
        {
            currentState = enemyState.Chase;
        }
        else
        {
            currentState = enemyState.Patrol;
        }
    }


    void Patrol()
    {
        Vector2 targetPosition = new Vector2(targetPoint.position.x, transform.position.y);
        Vector2 newPos = Vector2.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.fixedDeltaTime);

        rb.MovePosition(newPos);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            targetPoint = targetPoint == pP1 ? pP2 : pP1;
        }
    }

    void Chase()
    {
        Vector2 playerPosition = new Vector2(player.position.x, transform.position.y);
        Vector2 newPos = Vector2.MoveTowards(transform.position, playerPosition, chaseSpeed * Time.fixedDeltaTime);

        rb.MovePosition(newPos);
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
