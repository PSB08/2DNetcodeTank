using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Players;
using Scripts.UI;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Combat
{
    public class KillFeedManager : NetworkBehaviour
    {
        //RPC로 보낼 직렬화 타입
        [Serializable]
        public struct KillEntry : INetworkSerializable
        {
            public FixedString64Bytes Name;
            public int Kills;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Name);
                serializer.SerializeValue(ref Kills);
            }
        }
        
        public static KillFeedManager Instance { get; private set; }
        public event Action<ulong> OnPlayerKill;

        // 서버 전용 상태
        private readonly Dictionary<ulong, Dictionary<ulong, int>> killLog = new(); // [killer][victim] = count
        private readonly Dictionary<ulong, int> totalKills = new(); // [killer] = total

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            killLog.Remove(clientId);
            
            foreach (var killer in killLog.Keys.ToList())
            {
                killLog[killer].Remove(clientId);
            }
            
            totalKills.Remove(clientId);

            // 다음 프레임에 브로드캐스트
            StartCoroutine(BroadcastNextFrame());
        }

        private IEnumerator BroadcastNextFrame()
        {
            yield return null;
            BroadcastKillFeed(); // 서버에서 계산해서 클라에 전송
        }
        
        //외부에서 호출 할 kill 메서드
        public void RequestLogKill(ulong killerId, ulong victimId)
        {
            if (IsServer)
            {
                LogKillServer(killerId, victimId);
            }
            else
            {
                ReportKillServerRpc(killerId, victimId);
            }
        }

        //RequireOwnership을 통해 어떤 클라이언트든 킬 로그를 보낼 수 있음
        [ServerRpc(RequireOwnership = false)]
        private void ReportKillServerRpc(ulong killerId, ulong victimId)
        {
            LogKillServer(killerId, victimId);
        }

        // 서버 전용 킬 기록
        private void LogKillServer(ulong killerId, ulong victimId)
        {
            if (!IsServer) return;

            if (!killLog.TryGetValue(killerId, out var perVictim))
            {
                perVictim = new Dictionary<ulong, int>();
                killLog[killerId] = perVictim;
            }

            perVictim.TryGetValue(victimId, out var v);
            perVictim[victimId] = v + 1;

            totalKills.TryGetValue(killerId, out var t);
            totalKills[killerId] = t + 1;

            BroadcastKillFeed(); // 서버에서 계산해서 전파
            OnPlayerKill?.Invoke(killerId);
        }

        public int GetTotalKills(ulong playerId)
        {
            return totalKills.TryGetValue(playerId, out var k) ? k : 0;
        }
        
        private void BroadcastKillFeed()
        {
            if (!IsServer) return;

            var players = FindObjectsOfType<PlayerController>()
                .OrderBy(p => p.OwnerClientId)
                .ToArray();

            var payload = new KillEntry[players.Length];
            
            for (int i = 0; i < players.Length; i++)
            {
                var name = players[i].playerName.Value.ToString();
                var id = players[i].OwnerClientId;
                
                totalKills.TryGetValue(id, out var k);
                
                payload[i] = new KillEntry
                {
                    Name = name,
                    Kills = k
                };
            }

            SendKillFeedClientRpc(payload);
        }
        
        [ClientRpc]
        private void SendKillFeedClientRpc(KillEntry[] entries)
        {
            if (KillFeedUI.Instance == null)
            {
                Debug.LogWarning("[KillFeed] KillFeedUI not ready on client.");
                return;
            }

            var uiList = new List<KillEntry>(entries.Length);
            
            foreach (var e in entries)
            {
                uiList.Add(new KillEntry
                {
                    Name = e.Name.ToString(), Kills = e.Kills
                });
            }

            KillFeedUI.Instance.RefreshUI(uiList);
        }

        // 초기화(서버 전용)
        public void InitializeKillFeed()
        {
            if (!IsServer) return;

            totalKills.Clear();

            var players = FindObjectsOfType<PlayerController>()
                .OrderBy(p => p.OwnerClientId)
                .ToArray();

            foreach (var p in players)
                totalKills[p.OwnerClientId] = 0;

            BroadcastKillFeed();
        }
        
        
    }
}
