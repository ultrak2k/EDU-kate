using UnityEngine;

[RequireComponent(typeof(PlayerController))]

//handles player animation and particle effects
public class PlayerGraphics : MonoBehaviour
{

    [Header("Particles")]
    [SerializeField] private ParticleSystem _dashParticles;

    private PlayerController _playerController;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerController.OnDash += PlayDashParticles;
    }

    void OnDestroy()
    {
        _playerController.OnDash -= PlayDashParticles;
    }


    #region  Particle System Management
    void PlayDashParticles()
    {
        _dashParticles.Play();
    }
    
    #endregion
}
