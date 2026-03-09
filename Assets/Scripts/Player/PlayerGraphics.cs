using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerAttack))]

//handles player animation and particle effects alongside UI
public class PlayerGraphics : MonoBehaviour
{

    [Header("Particles")]
    [SerializeField] private ParticleSystem _dashParticles;
    [SerializeField] private ParticleSystem _parryParticles;

    [Header("UI")]
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _chargeBar;

    [Header("Sprite & Animation")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private float iFrameCycleTime = .1f;
    private bool _doFlicker = false;

    private PlayerController _playerController;
    private PlayerAttack _playerAttack;

    void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _playerAttack = GetComponent<PlayerAttack>();

        _playerController.OnDash += PlayDashParticles;
        _playerController.OnParry += PlayParryParticles;
        _playerController.OnChangeDirection += FlipSprite;
        _playerController.OnAnimatorMovement += OnMovement;
        _playerAttack.OnHealthChange += DoInvulnAlpha;
        _playerAttack.OnInvulnEnd += StopInvulnAlpha;
        _playerAttack.OnHealthChange += UpdateHealthBar;
        _playerAttack.OnChargeChange += UpdateChargeBar;
    }

    void OnDestroy()
    {
        _playerController.OnDash -= PlayDashParticles;
        _playerController.OnParry -= PlayParryParticles;
        _playerController.OnChangeDirection -= FlipSprite;
        _playerController.OnAnimatorMovement -= OnMovement;
        _playerAttack.OnHealthChange -= DoInvulnAlpha;
        _playerAttack.OnInvulnEnd -= StopInvulnAlpha;
        _playerAttack.OnHealthChange -= UpdateHealthBar;
        _playerAttack.OnChargeChange -= UpdateChargeBar;
    }

    #region Sprite Management

    void OnMovement(float inputValue)
    {
        _animator.SetFloat("speed", Mathf.Abs(inputValue));
    }

    void FlipSprite(float facingDirection)
    {
        _spriteRenderer.flipX = facingDirection < 0;
    }

    void DoInvulnAlpha(int changeHealth, int maxHealth)
    {
        //check if it's damage and not healing by comparing fill
        if (_healthBar.fillAmount > (float)changeHealth / maxHealth)
        {
            StartCoroutine(DoIFrameFlicker());
        }
    }

    void StopInvulnAlpha()
    {
        _doFlicker = false;
    }

    IEnumerator DoIFrameFlicker()
    {
        _doFlicker = true;
        while (_doFlicker)
        {
            _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, .5f);
            yield return new WaitForSeconds(iFrameCycleTime);
            _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1f);
            yield return new WaitForSeconds(iFrameCycleTime);
        }
    }

    #endregion

    #region UI mangement
    void UpdateHealthBar(int changeHealth, int maxHealth)
    {
        _healthBar.fillAmount = (float)changeHealth / maxHealth;
    }

    void UpdateChargeBar(int changeMana, int maxMana)
    {
        _chargeBar.fillAmount = (float)changeMana / maxMana;
    }

    #endregion

    #region  Particle System Management
    void PlayDashParticles()
    {
        _dashParticles.Play();
    }
    
    void PlayParryParticles()
    {
        _parryParticles.Play();
    }
    
    #endregion
}
