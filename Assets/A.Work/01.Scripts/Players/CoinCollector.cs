using Scripts.Combat;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Players
{
    public class CoinCollector : NetworkBehaviour
    {
        [Header("Reference variables")] 
        [SerializeField] private BountyCoin bountyCoinPrefab;
        [SerializeField] private TankHealth health;
        [SerializeField] private float bountyRatio;
        
        public NetworkVariable<int> totalCoins = new NetworkVariable<int>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Coin coin))
            {
                int amount = coin.Collect();

                if (!IsServer) return;  //서버가 아닐 경우 totalCoins를 건드리지 않는다
                totalCoins.Value += amount;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            health.OnDieEvent += HandleDieEvent;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            health.OnDieEvent -= HandleDieEvent;
        }

        private void HandleDieEvent()
        {
            int bountyValue = Mathf.FloorToInt(totalCoins.Value * bountyRatio);

            float coinScale = Mathf.Clamp(bountyValue / 100.0f, 1f, 3f);
            
            BountyCoin coinInstance = Instantiate(bountyCoinPrefab, transform.position, Quaternion.identity);
            coinInstance.SetCoinValue(bountyValue);
            coinInstance.NetworkObject.Spawn();

            coinInstance.SetCoinToVisible(coinScale);
        }
        
        public void SpendCoin(int value)
        {
            totalCoins.Value -= value;
        }
        
    }
}