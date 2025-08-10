using System;
using System.Text;
using System.Threading.Tasks;
using TankCode.Core.Network.Shared;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using NetworkClient = TankCode.Core.Network.Client.NetworkClient;

namespace TankCode.Networking
{
    public class ClientGameManager : IDisposable
    {
        private JoinAllocation _joinAllocation;
        private string _playerName;
        
        public string PlayerName => _playerName;
        
        private NetworkClient _networkClient;
        
        public void Dispose()
        {
            _networkClient?.Dispose();
        }

        public void Disconnect()
        {
            _networkClient.Disconnect();
        }
        
        public async Task<bool> InitManagerAsync()
        {
            //여기에 UGS 플레이어 인증이 들어감
            //UGS : Unity Game Service
            await UnityServices.InitializeAsync();  //유니티 서비스를 초기화 한다
            
            _networkClient = new NetworkClient(NetworkManager.Singleton);

            UGSAuthState authState = await UGSAuthWrapper.DoAuthAsync();  //5회 로그인 시도

            if (authState == UGSAuthState.Authenticated)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> StartClientWithJoinCode(string joinCode)
        {
            try
            {
                _joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(_joinAllocation.ToRelayServerData("dtls"));

                UserData userData = new UserData()
                {
                    username = _playerName,
                    userAuthId = AuthenticationService.Instance.PlayerId
                };
                  
                string json = JsonUtility.ToJson(userData);
                byte[] payload = Encoding.UTF8.GetBytes(json);
                
                NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;
                
                return NetworkManager.Singleton.StartClient();
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                return false;
            }
        }
        
        public void ChangeScene(string sceneName)
            => SceneManager.LoadScene(sceneName);

        public void SetPlayerName(string playerName)
        {
            _playerName = playerName;
        }

    }
}