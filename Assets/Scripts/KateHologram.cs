using UnityEngine;

public class KateHologram : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    void Start()
    {
        //_playerController.OnChangeDirection += FlipSprite;
        //_playerController.OnAnimatorMovement += OnMovement;
        //_playerController.RealOnJump += OnJump;
    }

    void OnDestroy()
    {
        //_playerController.OnChangeDirection -= FlipSprite;
        //_playerController.OnAnimatorMovement -= OnMovement;
        //_playerController.RealOnJump -= OnJump;
    }
}
