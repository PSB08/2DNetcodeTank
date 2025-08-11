using System;
using Scripts.System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Scripts.Core.Network.Client
{
    public class NetworkClient : IDisposable
    {
        private NetworkManager _networkManager;

        public NetworkClient(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnect;
        }

        public void Dispose()
        {
            if(_networkManager != null)
            {
                _networkManager.OnClientDisconnectCallback -= HandleClientDisconnect;
            }
        }
        
        private void HandleClientDisconnect(ulong clientId)
        {
            if (clientId != 0 && clientId != _networkManager.LocalClientId) return;

            Disconnect();
        }

        public void Disconnect()
        {
            if(SceneManager.GetActiveScene().name != SceneNames.MenuScene)
            {
                SceneManager.LoadScene(SceneNames.MenuScene);
            }

            if(_networkManager.IsConnectedClient)
            {
                _networkManager.Shutdown();
            }
        }
        
    }
}