using TankCode.Combat;
using UnityEngine;

namespace TankCode.Projectiles
{
    public class ServerProjectile : ProjectileBase
    {
        [SerializeField] private int damage;

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (other.attachedRigidbody.TryGetComponent(out TankHealth health))
            {
                health.TakeDamage(damage, ownerClientId);
            }
            base.OnTriggerEnter2D(other);
        }
        
        
    }
}