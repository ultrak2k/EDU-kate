using System;
using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour, IDamageable
{
    //interface stuff
    public int Health { get => _health; }
    public IDamageable.DamageTeam Team { get => _team; }


    [SerializeField] private readonly IDamageable.DamageTeam _team = IDamageable.DamageTeam.PLAYER;

    [Header("Health & Damage")]
    [SerializeField] private int _health;
    [SerializeField] private int _maxHealth = 10;
    [SerializeField] private float invulnTime = .5f;
    [SerializeField] private bool _canBeDamaged = true;


    [Header("Charge & Attacking")]
    [SerializeField] private int _charge;
    [SerializeField] private int _maxCharge = 10;
    [SerializeField] private int _parryRechargeAmount = 1;

    [Header("Melee")]
    //kind of hackjob attack method, full game should have seperate scripts for it
    [SerializeField] private DamageDealer _meleeTrigger;
    [SerializeField] private float _meleeStartTime = 5f;
    [SerializeField] private float _meleeLingerTime = 5f;
    [SerializeField] private int _meleeChargeUse = 1;
    [SerializeField] private float _maxScale = 1f;

    [Header("Ranged")]
    [SerializeField] private DamageDealer _bulletPrefab;
    [SerializeField] private int _rangedChargeUse = 1;
    [SerializeField] private float _rangedCooldown = .5f;
    [SerializeField] private bool _canFire = true;
    [SerializeField] private float bulletSpeed = 10f;

    private PlayerController _playerController;

    public event Action<int, int> OnHealthChange; //int 1: current health | int 2 : max health
    public event Action<int, int> OnChargeChange; //int 1: current mana [positive or negative] | int 2 : max mana
    public event Action OnInvulnEnd; //stops iframe flash



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _health = _maxHealth;

        OnHealthChange?.Invoke(_health, _maxHealth);
        OnChargeChange?.Invoke(_charge, _maxCharge);

        _playerController.OnParry += AddCharge;
    }

    void OnDestroy()
    {
        _playerController.OnParry -= AddCharge;
    }

    void AddCharge()
    {
        _charge += _charge + _parryRechargeAmount <= _maxCharge ? _parryRechargeAmount : 0;
        OnChargeChange?.Invoke(_charge, _maxCharge);
    }

    public void TakeDamage(int inDamage)
    {
        if (!_playerController.GetIsParrying() && _canBeDamaged)
        {
            _health -= inDamage;
            StartCoroutine(StartInvulnFrames());
            OnHealthChange?.Invoke(_health, _maxHealth);
            Die();
        }
    }

    public void PerformAttack()
    {
        _meleeTrigger.gameObject.SetActive(true);
    }

    void OnAttackMelee()
    {
        if (_charge >= _meleeChargeUse)
        {
            _charge -= _meleeChargeUse;
            StartCoroutine(ExpandShieldBubble());

        }
            
    }

    void OnAttackRanged()
    {
        if (_charge >= _rangedChargeUse && _canFire)
        {
            _charge -= _rangedChargeUse;
            StartCoroutine(FireShot());
        }
    }
    void Die()
    {
        if (_health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    IEnumerator FireShot()
    {
        _canFire = false;
        Rigidbody2D bulletRB = Instantiate(_bulletPrefab, transform.position, Quaternion.identity).gameObject.GetComponent<Rigidbody2D>();
        bulletRB.linearVelocityX = bulletSpeed * _playerController.GetFacingDirection();
        _bulletPrefab.GetComponent<SpriteRenderer>().flipX = true;
        yield return new WaitForSeconds(_rangedCooldown);
        _canFire = true;

    }

    IEnumerator ExpandShieldBubble()
    {
        _meleeTrigger.gameObject.SetActive(true);
        float movementFraction = 0;
        while (movementFraction < 1)
        {
            movementFraction += Time.deltaTime * 5;
            float newScale = Mathf.Lerp(0.01f, _maxScale, movementFraction);
            _meleeTrigger.gameObject.transform.localScale = new Vector3(newScale, newScale, 1);
            yield return null;
        }
        yield return new WaitForSeconds(_meleeLingerTime);
        _meleeTrigger.gameObject.transform.localScale = new Vector3(.01f, .01f, .01f);
        _meleeTrigger.gameObject.SetActive(false);
    }

    //makes player not take damage for specififed time        
    IEnumerator StartInvulnFrames()
    {
        _canBeDamaged = false;
        yield return new WaitForSeconds(invulnTime);
        _canBeDamaged = true;
        Debug.Log("stop");
        OnInvulnEnd?.Invoke();
    }
}
