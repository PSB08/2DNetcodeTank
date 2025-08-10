using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TankCode.Players;
using TankCode.UI;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TankCode.Combat
{
    public class KillFeedManager : NetworkBehaviour
    {
        public static KillFeedManager Instance { get; private set; }

        private Dictionary<ulong, Dictionary<ulong, int>> killLog = new();  // killLog[킬한 사람 ID][죽인 대상 ID] = 횟수
        private Dictionary<ulong, int> totalKills = new();  // totalKills[킬한 사람 ID] = 총 킬 수

        public NetworkVariable<FixedString512Bytes> syncedKillFeedText = new(writePerm: NetworkVariableWritePermission.Server);

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
            killLog.Remove(clientId);  // 나간 클라이언트가 킬한 기록 제거
            
            foreach (var killer in killLog.Keys)
            {
                killLog[killer].Remove(clientId);  // 다른 플레이어의 기록에서 나간 클라이언트를 죽인 기록 제거
            }
            
            totalKills.Remove(clientId);  // 총 킬 수 기록에서도 제거
            StartCoroutine(UpdateKillFeedNextFrame());  // 다음 프레임에서 UI 갱신
        }
        
        private IEnumerator UpdateKillFeedNextFrame()
        {
            yield return null;
            UpdateKillTextClientRpc();
        }
        
        public void LogKill(ulong killerId, ulong victimId)
        {
            // killLog에 killerId가 가진 킬 로그가 없으면 새로 Dictionary 생성
            if (!killLog.ContainsKey(killerId))
                killLog[killerId] = new Dictionary<ulong, int>();

            // killerId가 victimId를 죽인 횟수가 없으면 0으로 초기화 
            if (!killLog[killerId].ContainsKey(victimId))
                killLog[killerId][victimId] = 0;

            // 해당 victimId의 킬 수++
            killLog[killerId][victimId]++;

            // killerId의 총 킬 수 기록이 없으면 0으로 초기화
            if (!totalKills.ContainsKey(killerId))
                totalKills[killerId] = 0;

            // 총 킬수 초기화
            totalKills[killerId]++;

            UpdateKillTextClientRpc();  // 모든 클라이언트 UI 갱신
        }

        [ClientRpc]  // 클라이언트에게 킬 UI 수정 콜
        public void UpdateKillTextClientRpc()
        {
            StringBuilder header = new StringBuilder();  // 플레이어 이름
            StringBuilder kills = new StringBuilder();  // 각 플레이어의 킬 수
            
            var allPlayers = FindObjectsOfType<PlayerController>()
                .OrderBy(p => p.OwnerClientId)
                .ToArray();

            foreach (var player in allPlayers)
            {
                string name = player.playerName.Value.ToString();
                ulong id = player.OwnerClientId;

                header.Append(name).Append(" vs ");

                int killCount = totalKills.ContainsKey(id) ? totalKills[id] : 0;
                kills.Append($"{killCount}".PadRight(name.Length + 4));
                // 이름 길이에 맞춰 정렬
            }

            if (header.Length >= 4)
                header.Length -= 4; // 마지막 " vs " 제거

            StringBuilder final = new StringBuilder();
            final.AppendLine(header.ToString());
            final.AppendLine(kills.ToString());
            // 최종 출력 문자열 구성
            
            syncedKillFeedText.Value = final.ToString();  // 네트워크 동기화 변수에 저장 > UI에서 표시 가능
        }

        //초기화 메서드
        public void InitializeKillFeed()
        {
            totalKills.Clear();
            
            var players = FindObjectsOfType<PlayerController>()
                .OrderBy(p => p.OwnerClientId)
                .ToArray();

            //여기서 모든 플레이어의 킬 수를 초기화
            foreach (var player in players)
            {
                ulong id = player.OwnerClientId;
                totalKills[id] = 0;
            }

            UpdateKillTextClientRpc();
        }
        
        
    }
}