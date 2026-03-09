using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class WolfEnemy : MonoBehaviour, IDamageable
{
    public int Health => _health;
    public IDamageable.DamageTeam Team => IDamageable.DamageTeam.ENEMY;

    public void TakeDamage(int inDamage)
    {
        if (_state == State.Dead)
        {
            return;
        }

        _health -= inDamage;

        if (_health <= 0)
        {
            Die();
        }
    }

    [Header("Health")]
    [SerializeField] private int _health = 3;

    [Header("Detection")]
    [SerializeField] private float _aggroRange = 6f;
    [SerializeField] private float _morphRange = 1.5f;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _stopDistance = 0.1f;

    [Header("Orb / Parry")]
    [SerializeField] private float _orbLifetime = 2f;

    [Header("Explosion AOE")]
    [SerializeField] private float _explosionRadius = 2.5f;
    [SerializeField] private int _explosionDamage = 2;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private GameObject _explosion;

    [Header("Layers")]
    [SerializeField] private LayerMask _damageLayerName;
    [SerializeField] private LayerMask _parryableLayerName;

    private enum State { Idle, Chasing, Morphing, Orb, Dead }

    private State _state = State.Idle;
    private Rigidbody2D _rb;
    private Animator _anim;
    private Transform _player;
    private Collider2D _col;
    private float _orbTimer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _col = GetComponent<Collider2D>();

        gameObject.layer = _damageLayerName;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("[WolfEnemy] No GameObject tagged 'Player' found in scene.");
        }
    }

    void Update()
    {
        if (_state == State.Dead || _player == null)
        {
            return;
        }

        if (_state == State.Idle)
        {
            UpdateIdle();
        }
        else if (_state == State.Chasing)
        {
            UpdateChasing();
        }
        else if (_state == State.Orb)
        {
            UpdateOrb();
        }
    }

    void FixedUpdate()
    {
        if (_state == State.Chasing)
        {
            MoveTowardPlayer();
        }
    }

    void UpdateIdle()
    {
        if (DistanceToPlayer() <= _aggroRange)
        {
            EnterChase();
        }
    }

    void UpdateChasing()
    {
        FacePlayer();

        if (DistanceToPlayer() <= _morphRange)
        {
            StartCoroutine(MorphToOrb());
        }
    }

    void UpdateOrb()
    {
        _orbTimer -= Time.deltaTime;

        if (_orbTimer <= 0f)
        {
            Explode();
        }
    }

    void MoveTowardPlayer()
    {
        if (_state != State.Chasing)
        {
            return;
        }

        float dir = Mathf.Sign(_player.position.x - transform.position.x);
        _rb.linearVelocity = new Vector2(dir * _moveSpeed, _rb.linearVelocity.y);
    }

    void FacePlayer()
    {
        float dir = _player.position.x - transform.position.x;

        if (Mathf.Abs(dir) > _stopDistance)
        {
            Vector3 s = transform.localScale;
            // Negate the sign so the sprite faces toward the player
            s.x = Mathf.Abs(s.x) * -Mathf.Sign(dir);
            transform.localScale = s;
        }
    }

    void EnterChase()
    {
        _state = State.Chasing;
        _anim.SetBool("IsRunning", true);
    }

    IEnumerator MorphToOrb()
    {
        if (_state == State.Morphing || _state == State.Orb)
        {
            yield break;
        }

        _state = State.Morphing;

        _anim.SetBool("IsRunning", false);
        _anim.SetBool("IsMorphing", true);

        yield return new WaitForSeconds(GetAnimationClipLength("ToOrb"));

        _anim.SetBool("IsMorphing", false);

        gameObject.layer = _parryableLayerName;

        _state = State.Orb;
        _orbTimer = _orbLifetime;
    }

    void Explode()
    {
        if (_state == State.Dead)
        {
            return;
        }

        _state = State.Dead;

        _anim.SetTrigger("Explode");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, _playerLayer);

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable != null && damageable.Team != IDamageable.DamageTeam.ENEMY)
            {
                damageable.TakeDamage(_explosionDamage);
            }
        }

        if (_explosion != null)
        {
            Instantiate(_explosion, transform.position, Quaternion.identity);
        }

        _col.enabled = false;
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Static;

        Destroy(gameObject, GetAnimationClipLength("Explode"));
    }

    void Die()
    {
        if (_state == State.Dead)
        {
            return;
        }

        _state = State.Dead;
        _col.enabled = false;
        _rb.linearVelocity = Vector2.zero;

        Destroy(gameObject, 0.5f);
    }

    float DistanceToPlayer()
    {
        if (_player != null)
        {
            return Vector2.Distance(transform.position, _player.position);
        }
        else
        {
            return float.MaxValue;
        }
    }

    float GetAnimationClipLength(string clipName)
    {
        if (_anim == null)
        {
            return 0.5f;
        }

        foreach (AnimationClip clip in _anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }

        Debug.LogWarning($"[WolfEnemy] Animation clip '{clipName}' not found so default 0.5s.");
        return 0.5f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _aggroRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _morphRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}