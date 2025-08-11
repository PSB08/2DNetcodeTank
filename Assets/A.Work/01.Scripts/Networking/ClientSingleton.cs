using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Networking
{
    public class ClientSingleton : MonoBehaviour
    {
        private static ClientSingleton _instance;

        public static ClientSingleton Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindFirstObjectByType<ClientSingleton>();

                Debug.Assert(_instance != null, "No Client singleton");
                return _instance;
            }
        }

        public ClientGameManager GameManager { get; private set; }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
        
        public async Task<bool> CreateClientAsync()
        {
            GameManager = new ClientGameManager();

            bool result = await GameManager.InitManagerAsync();

            if (result)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnectCallback;    
            }

            return result;
        }

        private void HandleClientDisconnectCallback(ulong clientID)
        {
            Debug.Log($"{clientID} is disconnected");
        }
        
    }
}