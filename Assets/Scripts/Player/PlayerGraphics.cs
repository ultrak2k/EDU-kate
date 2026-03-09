using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerAttack _playerAttack;

    void Start()
    {
        _playerController.OnDash += PlayDashParticles;
        _playerController.OnParry += PlayParryParticles;
        _playerController.OnChangeDirection += FlipSprite;
        _playerController.OnAnimatorMovement += OnMovement;
        _playerController.RealOnJump += OnJump;
        if (_playerAttack)
        {
            _playerAttack.OnHealthChange += DoInvulnAlpha;
            _playerAttack.OnInvulnEnd += StopInvulnAlpha;
            _playerAttack.OnHealthChange += UpdateHealthBar;
            _playerAttack.OnChargeChange += UpdateChargeBar;
        }
        
    }

    void OnDestroy()
    {
        _playerController.OnDash -= PlayDashParticles;
        _playerController.OnParry -= PlayParryParticles;
        _playerController.OnChangeDirection -= FlipSprite;
        _playerController.OnAnimatorMovement -= OnMovement;
        _playerController.RealOnJump -= OnJump;
        if (_playerAttack)
        {
            _playerAttack.OnHealthChange -= DoInvulnAlpha;
            _playerAttack.OnInvulnEnd -= StopInvulnAlpha;
            _playerAttack.OnHealthChange -= UpdateHealthBar;
            _playerAttack.OnChargeChange -= UpdateChargeBar;
        }
        
    }

    #region Sprite Management

    void OnMovement(float inputValue)
    {
        _animator.SetFloat("speed", Mathf.Abs(inputValue));
    }

    void OnJump()
    {
        _animator.SetTrigger("Jump");
    }

    void FlipSprite(float facingDirection)
    {
        _spriteRenderer.flipX = facingDirection < 0;
    }

    void DoInvulnAlpha(int changeHealth, int maxHealth)
    {
        if (_healthBar)
        {
            //check if it's damage and not healing by comparing fill
            if (_healthBar.fillAmount > (float)changeHealth / maxHealth)
            {
                StartCoroutine(DoIFrameFlicker());
            }
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
        if (_healthBar)
        {
            _healthBar.fillAmount = (float)changeHealth / maxHealth;
        } 
    }

    void UpdateChargeBar(int changeMana, int maxMana)
    {
        if (_chargeBar)
        {
            _chargeBar.fillAmount = (float)changeMana / maxMana;
        }
    }

    #endregion

    #region  Particle System Management
    void PlayDashParticles()
    {   
        if (_dashParticles)
        {
            _dashParticles.Play();
        }
    }
    
    void PlayParryParticles()
    {
        if (_parryParticles)
        {
            _parryParticles.Play();
        }
    }
    
    #endregion
}
