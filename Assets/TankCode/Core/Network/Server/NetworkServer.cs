using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TankCode.Core.Network.Shared;
using TankCode.Networking;
using Unity.Netcode;
using UnityEngine;

namespace TankCode.Core.Network.Server
{
    public class NetworkServer : IDisposable
    {
        private NetworkManager _networkManager;
        
        public Action<string> OnClientLeft;
        
        private Dictionary<ulong, string> _clientIdToAuthDict = new Dictionary<ulong, string>();
        private Dictionary<string, UserData> _authIdToUserDataDict = new Dictionary<string, UserData>();
        
        public NetworkServer(NetworkManager networkManager)
        {
            _networkManager = networkManager; //캐싱
            _networkManager.ConnectionApprovalCallback += HandleApprovalCheck; //승인요청시

            _networkManager.OnServerStarted += HandleServerStarted;
        }
        
        public void Dispose()
        {
            if (_networkManager == null) return;
            _networkManager.ConnectionApprovalCallback -= HandleApprovalCheck;
            _networkManager.OnServerStarted -= HandleServerStarted;

            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnect;

            if(_networkManager.IsListening)
            {
                _networkManager.Shutdown();
            }
        }

        private void HandleServerStarted()
        {
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnect;
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            if (_clientIdToAuthDict.TryGetValue(clientId, out string authId))
            {
                _clientIdToAuthDict.Remove(clientId);
                _authIdToUserDataDict.Remove(authId);
                OnClientLeft?.Invoke(authId);
            }
        }

        //클라이언트들이 서버에 접속할 때 실행시켜서 승인할지 말지를 결정하도록 한다. 이때 요청과 응답이 넘어온다.
        private void HandleApprovalCheck(
            NetworkManager.ConnectionApprovalRequest req, 
            NetworkManager.ConnectionApprovalResponse res)
        {
            string json = Encoding.UTF8.GetString(req.Payload);
            UserData data = JsonUtility.FromJson<UserData>(json);

            _clientIdToAuthDict[req.ClientNetworkId] = data.userAuthId;
            _authIdToUserDataDict[data.userAuthId] = data;
            
            Debug.Log(data.username);

            res.CreatePlayerObject = false;
            res.Approved = true;
            
            HostSingleton.Instance.StartCoroutine(CreatePanelWithDelay(0.5f, req.ClientNetworkId, data.username));
        }
        
        private IEnumerator CreatePanelWithDelay(float time, ulong clientID, string username)
        {
            yield return new WaitForSeconds(time);

            GameManager.Instance.CreateUIPanel(clientID, username);
        }
        
        public UserData GetUserDataByClientId(ulong clientId)
        {
            if (_clientIdToAuthDict.TryGetValue(clientId, out string authId))
            {
                if (_authIdToUserDataDict.TryGetValue(authId, out UserData data))
                {
                    return data;
                }
            }

            return null;
        }
        

    }
}