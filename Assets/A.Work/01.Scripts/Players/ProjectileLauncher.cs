using System;
using TankCode.Projectiles;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TankCode.Players
{
    public class ProjectileLauncher : NetworkBehaviour
    {
        [Header("references")] 
        [SerializeField] private PlayerInputSo playerInput;
        [SerializeField] private Transform projectileSpawnTrm;
        [SerializeField] private ProjectileBase serverPrefab;
        [SerializeField] private ProjectileBase clientPrefab;

        [SerializeField] private Collider2D playerCollider;

        [Header("Settings")] 
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float fireCooldown;
        
        public UnityEvent OnFire;

        private bool _isFire;
        private float _prevFireTime;

        public override void OnNetworkSpawn()
        {
            if (IsOwner == false) return;
            playerInput.OnFire += HandleFireKey;
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner == false) return;
            playerInput.OnFire -= HandleFireKey;
        }
        
        private void HandleFireKey(bool isFire)
        {
            _isFire = isFire;
        }

        private void Update()
        {
            if (IsOwner == false) return;
            if (_isFire == false) return;
            
            if (Time.time < _prevFireTime + fireCooldown) return;

            SpawnDummyProjectile(projectileSpawnTrm.position, projectileSpawnTrm.up);
            SpawnProjectileServerRpc(projectileSpawnTrm.position, projectileSpawnTrm.up);
            _prevFireTime = Time.time;
        }

        //클라가 서버에 요청을 해서 콜을 하는 거고, 오너만 실행할 수 있음
        [ServerRpc]
        private void SpawnProjectileServerRpc(Vector3 position, Vector3 direction)
        {
            ProjectileBase instance = Instantiate(serverPrefab, position, Quaternion.identity);
            
            instance.transform.up = direction;
            Physics2D.IgnoreCollision(playerCollider, instance.ColliderCompo);

            instance.FireProjectile(direction * projectileSpeed, OwnerClientId);
            
            SpawnProjectileClientRpc(position, direction);
        }

        //서버가 클라에 있는 Rpc를 호출할 때 쓴다
        [ClientRpc]
        private void SpawnProjectileClientRpc(Vector3 position, Vector3 direction)
        {
            if (IsOwner) return;  //소유자는 이미 더미 프로젝타일을 쐈기 때문에 소유자면 return
            
            SpawnDummyProjectile(position, direction);
        }

        private void SpawnDummyProjectile(Vector3 position, Vector3 direction)
        {
            ProjectileBase instance = Instantiate(clientPrefab, position, Quaternion.identity);
            
            instance.transform.up = direction;
            Physics2D.IgnoreCollision(playerCollider, instance.ColliderCompo);

            instance.FireProjectile(direction * projectileSpeed, NetworkManager.Singleton.LocalClientId);
            OnFire?.Invoke();
        }
        
        
    }
}