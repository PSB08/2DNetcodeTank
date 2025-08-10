using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TankCode.Core.Network.Server;
using TankCode.Core.Network.Shared;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TankCode.Networking
{
    public class HostGameManager : IDisposable
    {
        private Allocation _relayAllocation;
        private const int maxConnectionCount = 10;  //하나의 클라가 몇개까지 접속 받을건지
        private string _joinCode;
        private string _lobbyId;
        
        public NetworkServer NetworkServer { get; private set; }
        
        public string JoinCode => _joinCode ?? string.Empty;
        private bool _isOpenRoom = false;

        private void MakeNetworkServer()
        {
            NetworkServer = new NetworkServer(NetworkManager.Singleton);
        }
        
        public async Task<bool> StartHostAsync()
        {
            try
            {
                _relayAllocation = await RelayService.Instance.CreateAllocationAsync(maxConnectionCount);
                _joinCode = await RelayService.Instance.GetJoinCodeAsync(_relayAllocation.AllocationId);
                Debug.Log(_joinCode);
                
                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(_relayAllocation.ToRelayServerData("dtls"));
                //dtls는 udp의 보안 버전
                
                string playerName = ClientSingleton.Instance.GameManager.PlayerName;
                try
                {
                    //로비를 만들기 위한 옵션들을 넣는다.
                    CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
                    lobbyOptions.IsPrivate = false; //로비 옵션을 만들어서 넣어줘야 한다. 만약 이걸 true로 하면 조인코드로만 참석 가능
            
                    //해당 로비 옵션에 Join코드를 넣어준다. (커스텀데이터를 이런식으로 넣는다)
                    // Visbilty Member는 해당 로비의 멤버는 자유롭게 읽을 수 있다는 뜻.
                    lobbyOptions.Data = new Dictionary<string, DataObject>() 
                    {
                        {
                            "JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Member, value:_joinCode)
                        }
                    };
                    //로비 이름과 옵션을 넣어주도록 되어 있음.
                    Lobby lobby = await LobbyService.Instance.CreateLobbyAsync($"{playerName}'s Lobby", maxConnectionCount, lobbyOptions);
            
                    //로비는 만들어진후 활동이 없으면 파괴되도록되어 있다. 따라서 일정시간간격으로 ping을 보내야 한다.
                    _lobbyId = lobby.Id;
                    HostSingleton.Instance.StartCoroutine(HeartBeatLobby(15)); //15초마다 핑
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError(e);
                    return false;
                }
                MakeNetworkServer();

                UserData userData = new UserData()
                {
                    username = playerName,
                    userAuthId = AuthenticationService.Instance.PlayerId
                };
                string json = JsonUtility.ToJson(userData);
                byte[] payload = Encoding.UTF8.GetBytes(json);
                NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;

                NetworkServer.OnClientLeft += HandleClientLeft;
                _isOpenRoom = true;
                return NetworkManager.Singleton.StartHost();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
        
        public void ChangeNetworkScene(string sceneName)
            => NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        
        private IEnumerator HeartBeatLobby(float waitTimeSec)
        {
            var timer = new WaitForSecondsRealtime(waitTimeSec);
            while (true)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(_lobbyId); //로비로 핑 보내고
                yield return timer;
            }
        }
        
        private async void HandleClientLeft(string authID)
        {
            if (_lobbyId == null) return;
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_lobbyId, authID);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }


        public void Dispose()
        {
            Shutdown();
        }

        public async void Shutdown()
        {
            if (!_isOpenRoom) return;
            
            //하트비트 코루틴 꺼준다.
            HostSingleton.Instance.StopAllCoroutines();

            if (!string.IsNullOrEmpty(_lobbyId))
            {
                try
                {
                    await LobbyService.Instance.DeleteLobbyAsync(_lobbyId); //나올때 방삭제
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError(e);
                }
            }

            NetworkServer.OnClientLeft -= HandleClientLeft;
            _isOpenRoom = false;
            _lobbyId = string.Empty;
            NetworkServer?.Dispose();
        }

        
    }
}