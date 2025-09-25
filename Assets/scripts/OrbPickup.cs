using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OrbPickup : MonoBehaviour
{
    public string playerTag = "Player";

    [Header("Audio")]
    public AudioClip pickupSfx;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Tooltip("If true, the orb destroys immediately; the SFX is played with PlayClipAtPoint (safe).")]
    public bool destroyImmediately = true;

    [Header("Visuals")]
    public Renderer visualToHide;   // optional: assign if visuals are on a child; otherwise auto-find

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Awake()
    {
        if (!visualToHide) visualToHide = GetComponentInChildren<Renderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        // add to score
        OrbCounter.Add(1);

        // play SFX in a safe way that survives orb destroy
        if (pickupSfx)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position, sfxVolume);

        // hide/or destroy
        if (visualToHide) visualToHide.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        if (destroyImmediately || !pickupSfx)
            Destroy(gameObject);
        else
            Destroy(gameObject, pickupSfx.length); // let local SFX finish if you used your own AudioSource
    }
}
