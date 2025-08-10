using Unity.Netcode;
using UnityEngine;

namespace TankCode.Combat
{
    public abstract class Coin : NetworkBehaviour
    {
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected CircleCollider2D circleCollider;
        [SerializeField] protected int coinValue = 10;
        [SerializeField] protected bool isCollected;

        public NetworkVariable<bool> isActive = new NetworkVariable<bool>();

        public override void OnNetworkSpawn()
        {
            if (IsClient)
                SetVisible(isActive.Value);
        }

        public void SetVisible(bool isActiveValue)
        {
            circleCollider.enabled = isActiveValue; 
            spriteRenderer.enabled = isActiveValue;
        }

        public void SetCoinValue(int value)
        {
            coinValue = value;
        }

        public abstract int Collect();

    }
}