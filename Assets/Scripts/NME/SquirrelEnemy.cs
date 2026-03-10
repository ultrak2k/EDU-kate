using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SquirrelEnemy : MonoBehaviour, IDamageable
{
    public int Health => _health;
    public IDamageable.DamageTeam Team => IDamageable.DamageTeam.ENEMY;

    public void TakeDamage(int inDamage)
    {
        if (_state == State.Dead) return;

        _health -= inDamage;

        if (_health <= 0)
            Die();
    }

    [Header("Health")]
    [SerializeField] private int _health = 2;

    [Header("Detection")]
    [SerializeField] private float _aggroRange = 10f;

    [Header("Throwing")]
    [SerializeField] private float _throwCooldownMin = 3f;
    [SerializeField] private float _throwCooldownMax = 5f;
    [SerializeField] private float _acornSpeed = 8f;
    [SerializeField] private float _aimSpread = 5f;
    [SerializeField][Range(0f, 1f)] private float _throwFirePoint = 0.9f;  // 0=start, 1=end of anim
    [SerializeField] private Transform _throwOrigin;
    [SerializeField] private GameObject _acornPrefab;

    [Header("Layers")]
    [SerializeField] private LayerMask _parryableLayer;

    private enum State { Idle, Chilling, Throwing, Dead }

    private State _state = State.Idle;
    private Animator _anim;
    private Transform _player;
    private float _lastThrowTime = -99f;

    void Awake()
    {
        _anim = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("[SquirrelEnemy] No GameObject tagged 'Player' found in scene.");
        }
    }

    void Update()
    {
        if (_state == State.Dead || _player == null) return;

        FacePlayer();

        if (_state == State.Idle && DistanceToPlayer() <= _aggroRange)
        {
            EnterChill();
        }
        else if (_state == State.Chilling && DistanceToPlayer() > _aggroRange)
        {
            // Player left range, stop throw loop
            StopAllCoroutines();
            _state = State.Idle;
            _anim.SetBool("IsChill", false);
        }
    }

    void EnterChill()
    {
        _state = State.Chilling;
        _anim.SetBool("IsChill", true);
        StartCoroutine(ThrowLoop());
    }

    IEnumerator ThrowLoop()
    {
        while (_state == State.Chilling || _state == State.Throwing)
        {
            if (_player == null || DistanceToPlayer() > _aggroRange) yield break;

            // Respect the global cooldown even if the player left and came back

            float timeSinceLast = Time.time - _lastThrowTime;
            if (timeSinceLast < _throwCooldownMin)
                yield return new WaitForSeconds(_throwCooldownMin - timeSinceLast);

            // Play throw animation
            _state = State.Throwing;
            _anim.SetBool("IsChill", false);
            _anim.SetTrigger("Throw");

            float clipLength = GetAnimationClipLength("Throw");

            // Wait until the release moment in the animation
            yield return new WaitForSeconds(clipLength * _throwFirePoint);

            SpawnAcorn();
            _lastThrowTime = Time.time;

            // Wait out the rest of the animation before chilling
            yield return new WaitForSeconds(clipLength * (1f - _throwFirePoint));

            // Now chill and wait before throwing again
            _state = State.Chilling;
            _anim.SetBool("IsChill", true);

            float waitTime = Random.Range(_throwCooldownMin, _throwCooldownMax);
            yield return new WaitForSeconds(waitTime);
        }
    }

    void SpawnAcorn()
    {
        if (_acornPrefab == null)
        {
            Debug.LogWarning("[SquirrelEnemy] No acorn prefab assigned.");
            return;
        }

        Vector3 origin = _throwOrigin != null ? _throwOrigin.position : transform.position;

        // Aim roughly at the player with a small random spread
        Vector2 toPlayer = (_player.position - origin).normalized;
        float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        float spreadAngle = Random.Range(-_aimSpread, _aimSpread);
        float finalAngle = (baseAngle + spreadAngle) * Mathf.Deg2Rad;

        Vector2 fireDirection = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));

        GameObject acorn = Instantiate(_acornPrefab, origin, Quaternion.identity);
        acorn.layer = _parryableLayer;

        Rigidbody2D acornRb = acorn.GetComponent<Rigidbody2D>();
        if (acornRb != null)
        {
            acornRb.linearVelocity = fireDirection * _acornSpeed;
        }
        else
        {
            Debug.LogWarning("[SquirrelEnemy] Acorn prefab has no Rigidbody2D, can't apply velocity.");
        }
    }

    void FacePlayer()
    {
        float dir = _player.position.x - transform.position.x;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * -Mathf.Sign(dir);
        transform.localScale = s;
    }

    void Die()
    {
        if (_state == State.Dead) return;

        StopAllCoroutines();
        _state = State.Dead;
        Destroy(gameObject, 0.5f);
    }

    float DistanceToPlayer()
    {
        return _player != null ? Vector2.Distance(transform.position, _player.position) : float.MaxValue;
    }

    float GetAnimationClipLength(string clipName)
    {
        if (_anim == null) return 0.5f;

        foreach (AnimationClip clip in _anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        Debug.LogWarning($"[SquirrelEnemy] Animation clip '{clipName}' not found, defaulting to 0.85s.");
        return 0.85f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _aggroRange);
    }
}