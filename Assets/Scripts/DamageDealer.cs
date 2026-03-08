using UnityEngine;

public class DamageDealer : MonoBehaviour
{   

    public enum OnHitBehaviour
    {
        DESTROY, //for bullet type projectiles
        PHASE, //for projectiles that can go through walls
        PERSIST //for things like melee attack bubbles or static obstacles that dissapear through other means
    }

    [SerializeField] private IDamageable.DamageTeam _team = IDamageable.DamageTeam.NONE; //if not the same team ,damages the hit object
    [SerializeField] private OnHitBehaviour _hitBehaviour = OnHitBehaviour.DESTROY;
    [SerializeField] private int _damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {

        //on collision with ground 
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (_hitBehaviour == OnHitBehaviour.DESTROY)
            {
                Destroy(gameObject);
            }
        }

        //check if we can damage the hit thing and if it's on a different team
        else
        {
            Debug.Log(collision.gameObject.name);  
            if (collision.TryGetComponent(out IDamageable damageable))
            {
                Debug.Log("Orb Hit");
                if (_team != damageable.Team)
                {
                    damageable.TakeDamage(_damageAmount);
                    if (_hitBehaviour == OnHitBehaviour.DESTROY)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
