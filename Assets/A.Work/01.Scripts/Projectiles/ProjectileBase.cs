using System;
using UnityEngine;

namespace TankCode.Projectiles
{
    public class ProjectileBase : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 2f;
        [field: SerializeField] public Rigidbody2D RBCompo { get; private set; }
        [field: SerializeField] public Collider2D ColliderCompo { get; private set; }
        
        private float _currentLifeTime;

        protected ulong ownerClientId;
        
        protected virtual void Update()
        {
            _currentLifeTime += Time.deltaTime;

            if (_currentLifeTime > lifeTime)
            {
                DestroyObject();    
            }
        }

        public void FireProjectile(Vector2 velocity, ulong ownerId)
        {
            RBCompo.linearVelocity = velocity;
            ownerClientId = ownerId;
        }
        
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            DestroyObject();
        }

        protected virtual void DestroyObject()
        {
            Destroy(gameObject);
        }
        
    }
}