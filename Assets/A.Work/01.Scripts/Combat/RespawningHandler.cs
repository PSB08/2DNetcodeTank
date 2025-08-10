using TankCode.Core;
using TankCode.Players;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Combat
{
    public class RespawningHandler : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                HandlePlayerSpawned(player); 
            }

            PlayerController.OnPlayerSpawned += HandlePlayerSpawned;
            PlayerController.OnPlayerDeSpawned += HandlePlayerDespawned;
        }

        private void HandlePlayerSpawned(PlayerController player)
        {
            player.HealthCompo.OnDieEvent += () =>
            {
                ulong clientId = player.OwnerClientId;
                Color color = player.tankColor.Value;

                Destroy(player.gameObject);
                
                GameManager.Instance.SpawnTank(clientId, color, 1f);
            };
        }

        private void HandlePlayerDespawned(PlayerController controller)
        {
            
        }
        
    }
}