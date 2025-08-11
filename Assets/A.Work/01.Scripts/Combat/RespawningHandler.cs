using Scripts.Core;
using Scripts.Players;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Combat
{
    public class RespawningHandler : NetworkBehaviour
    {
        [SerializeField] private float keepCoinRatio;
        
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

                int remainCoin = Mathf.FloorToInt(player.CoinCompo.totalCoins.Value * keepCoinRatio);
                
                Destroy(player.gameObject);
                
                GameManager.Instance.SpawnTank(clientId, color, remainCoin, 1f);
            };
        }

        private void HandlePlayerDespawned(PlayerController controller)
        {
            
        }
        
    }
}