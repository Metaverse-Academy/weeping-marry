using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class OscillatePlatform2D : MonoBehaviour
{
    public float distance = 3f;   // total travel (centered on start)
    public float speed = 2f;      // cycles per second-ish

    Rigidbody2D rb;
    Vector2 startPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // replaces isKinematic
        rb.gravityScale = 0f;
        startPos = rb.position;
    }

    void FixedUpdate()
    {
        // PingPong gives 0→1→0…; center motion around startPos.x
        float half = distance * 0.5f;
        float offset = Mathf.Lerp(-half, half, Mathf.PingPong(Time.time * speed, 1f));
        rb.MovePosition(new Vector2(startPos.x + offset, startPos.y));
    }

    // (Optional) keep player stuck to platform
    void OnCollisionEnter2D(Collision2D c) { if (c.collider.CompareTag("Player")) c.collider.transform.SetParent(transform); }
    void OnCollisionExit2D(Collision2D c)  { if (c.collider.CompareTag("Player")) c.collider.transform.SetParent(null); }
}
