using Scripts.Combat;
using Unity.Netcode;
using UnityEngine;

namespace TankCode.Players
{
    public class CoinCollector : NetworkBehaviour
    {
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
        
        public void SpendCoin(int value)
        {
            totalCoins.Value -= value;
        }
        
    }
}