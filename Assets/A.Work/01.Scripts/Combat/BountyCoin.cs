using DG.Tweening;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Combat
{
    public class BountyCoin : Coin
    {
        [SerializeField] private CinemachineImpulseSource impulseSource;
        
        public override int Collect()
        {
            if (!IsServer)
            {
                SetVisible(false); //뽀개는건 서버가 할꺼야.
                return 0;    
            }

            if (isCollected) return 0;
        
            isCollected = true;
        
            Destroy(gameObject); //이러면 전체에서 뽀개져
            return coinValue;
        }

        public void SetCoinToVisible(float scale)
        {
            isActive.Value = true;
            CoinSpawnClientRpc(scale);
        }

        [ClientRpc]
        public void CoinSpawnClientRpc(float scale)
        {
            Vector3 destination = transform.position;
            transform.position = destination + new Vector3(0, 3f, 0);
            transform.localScale = Vector3.one * scale;
            SetVisible(true);
            transform.DOMove(destination, 0.6f).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                impulseSource.GenerateImpulse(0.3f);
            });
        }

        
    }
}