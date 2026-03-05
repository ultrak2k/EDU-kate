using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
public class ParryDetector : MonoBehaviour
{
    [Header("Colliders")]
    public Collider2D parryCollider;   // the hitbox that represents the parry zone
    public Collider2D playerCollider;  // the main player body collider

    [Header("Parry Bounce")]
    [Tooltip("Multiplier applied to PlayerController.jumpForce for the mini-jump (0.5 = half jump).")]
    public float miniJumpMultiplier = 0.5f;
    public GameObject Particle;

    [Tooltip("Layer(s) that can be parried � e.g. Enemy, Projectile.")]
    public LayerMask parryableLayer;


    private bool _isParryActive;
    private Rigidbody2D _rb;
    private PlayerController _pc;

    // cache of overlaps to avoid GC allocations � we only care about the first few hits anyway
    private readonly Collider2D[] _overlapBuffer = new Collider2D[8];

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _pc = GetComponent<PlayerController>();

        // Make sure the parry collider starts inactive
        if (parryCollider != null)
            parryCollider.enabled = false;
    }

    void Update()
    {
        if (!_isParryActive) return;
        CheckParryOverlap();
    }

    public void SetParryActive(bool active)
    {
        _isParryActive = active;

        if (parryCollider != null)
            parryCollider.enabled = active;
    }



    void CheckParryOverlap()
    {
        if (parryCollider == null) return;

        // ContactFilter so we only care about parryable layers
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(parryableLayer);
        filter.useTriggers = true;       // catch both triggers and solid colliders

        int hitCount = parryCollider.Overlap(filter, _overlapBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = _overlapBuffer[i];

            // Ignore self
            if (hit == parryCollider || hit == playerCollider) continue;

            // Valid parry hit � apply bounce and end the parry window immediately
            ApplyMiniJump();
            SetParryActive(false);
            Instantiate(Particle, transform.position, Quaternion.identity);
            return;
        }
    }

    void ApplyMiniJump()
    {
        if (_rb == null || _pc == null) return;

        float miniJumpForce = _pc.jumpForce * miniJumpMultiplier;

        // Override vertical velocity so the bounce feels consistent
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, miniJumpForce);

        Debug.Log($"[ParryDetector] Parry bounce! force = {miniJumpForce}");
    }
}