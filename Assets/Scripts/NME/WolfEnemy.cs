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
    [SerializeField] private float _aggroRangeX = 6f;    // horizontal reach either side
    [SerializeField] private float _aggroRangeY = 1.5f;  // vertical tolerance above/below
    [SerializeField] private float _morphRange = 1.5f;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _morphMoveSpeed = 2f;  // speed during the morph animation
    [SerializeField] private float _stopDistance = 0.1f;

    [Header("Orb / Parry")]
    [SerializeField] private float _orbLifetime = 2f;
    [SerializeField] private float _unMorphPause = 1f;

    [Header("Explosion AOE")]
    [SerializeField] private float _explosionRadius = 2.5f;
    [SerializeField] private int _explosionDamage = 2;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private GameObject _explosion;

    [Header("Layers")]
    [SerializeField] private LayerMask _damageLayerName;
    [SerializeField] private LayerMask _parryableLayerName;

    private enum State { Idle, Chasing, Morphing, Orb, UnMorphing, Dead }

    private State _state = State.Idle;
    private Rigidbody2D _rb;
    private Animator _anim;
    private Transform _player;
    private Collider2D _col;
    private float _orbTimer;
    private float _orbDirection;   // locked in when morph begins

    void Start()
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
        else if (_state == State.Morphing || _state == State.Orb)
        {
            MoveAsOrb();
        }
    }

    void UpdateIdle()
    {
        if (IsPlayerInDetectionZone())
        {
            EnterChase();
        }
    }

    void UpdateChasing()
    {
        FacePlayer();

        if (Mathf.Abs(_player.position.x - transform.position.x) <= _morphRange)
        {
            StartCoroutine(MorphToOrb());
        }
    }

    void UpdateOrb()
    {
        _orbTimer -= Time.deltaTime;

        if (_orbTimer <= 0f)
        {
            StartCoroutine(UnMorphToWolf());
        }
    }

    void MoveTowardPlayer()
    {
        float dir = Mathf.Sign(_player.position.x - transform.position.x);
        _rb.linearVelocity = new Vector2(dir * _moveSpeed, _rb.linearVelocity.y);
    }

    // Orb keeps rolling in the direction the wolf was already heading
    void MoveAsOrb()
    {
        float speed = _state == State.Morphing ? _morphMoveSpeed : _moveSpeed;
        _rb.linearVelocity = new Vector2(_orbDirection * speed, _rb.linearVelocity.y);
    }

    void FacePlayer()
    {
        float dir = _player.position.x - transform.position.x;

        if (Mathf.Abs(dir) > _stopDistance)
        {
            Vector3 s = transform.localScale;
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

        // Lock in the direction the wolf was running toward the player
        _orbDirection = Mathf.Sign(_player.position.x - transform.position.x);

        _anim.SetBool("IsRunning", false);
        _anim.SetBool("IsMorphing", true);

        yield return new WaitForSeconds(GetAnimationClipLength("ToOrb"));

        _anim.SetBool("IsMorphing", false);

        gameObject.layer = _parryableLayerName;

        _state = State.Orb;
        _orbTimer = _orbLifetime;
    }

    IEnumerator UnMorphToWolf()
    {
        if (_state != State.Orb)
        {
            yield break;
        }

        _state = State.UnMorphing;

        _rb.linearVelocity = Vector2.zero;

        gameObject.layer = _damageLayerName;

        _anim.SetTrigger("UnMorph");

        yield return new WaitForSeconds(GetAnimationClipLength("UnMorph"));

        yield return new WaitForSeconds(_unMorphPause);

        FacePlayer();
        EnterChase();
    }
    //ignore this shit
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

    bool IsPlayerInDetectionZone()
    {
        if (_player == null) return false;
        float dx = Mathf.Abs(_player.position.x - transform.position.x);
        float dy = Mathf.Abs(_player.position.y - transform.position.y);
        return dx <= _aggroRangeX && dy <= _aggroRangeY;
    }

    // Still used for morph range and gizmo reference
    float DistanceToPlayer()
    {
        return _player != null ? Vector2.Distance(transform.position, _player.position) : float.MaxValue;
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
        Gizmos.DrawWireCube(transform.position, new Vector3(_aggroRangeX * 2f, _aggroRangeY * 2f, 0f));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _morphRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}