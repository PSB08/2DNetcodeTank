using System;
using System.Collections;
using System.Linq;
using PSB_Lib.Dependencies;
using Scripts.Combat;
using Scripts.Players;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Core
{
    [Provide]
    public class GameResultManager : NetworkBehaviour, IDependencyProvider
    {
        [SerializeField] private int killLimit = 15;

        public event Action<string> OnWinGame;

        private void Awake()
        {
            StartCoroutine(AwakeCoroutine());
        }

        private IEnumerator AwakeCoroutine()
        {
            yield return null;
            
            KillFeedManager.Instance.OnPlayerKill += HandlePlayerKill;
        }

        public override void OnDestroy()
        {
            if (KillFeedManager.Instance != null)
                KillFeedManager.Instance.OnPlayerKill -= HandlePlayerKill;
        }

        private void HandlePlayerKill(ulong killerId)
        {
            int kills = KillFeedManager.Instance.GetTotalKills(killerId);

            if (kills >= killLimit)
            {
                EndGame(killerId);
            }
        }

        private void EndGame(ulong winnerId)
        {
            GameOverClientRpc(winnerId);
        }

        [ClientRpc]
        private void GameOverClientRpc(ulong winnerId)
        {
            var winner = FindObjectsOfType<PlayerController>()
                .FirstOrDefault(p => p.OwnerClientId == winnerId);

            string winnerName = winner != null ? winner.playerName.Value.ToString() : "Unknown";
            OnWinGame?.Invoke(winnerName);
        }
        
        
    }
}