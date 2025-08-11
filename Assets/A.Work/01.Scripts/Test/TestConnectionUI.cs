using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Test
{
    public class TestConnectionUI : MonoBehaviour
    {
        [SerializeField] private Button hostBtn;
        [SerializeField] private Button clientBtn;

        private void Awake()
        {
            hostBtn.onClick.RemoveAllListeners();
            hostBtn.onClick.AddListener(HandleHostClick);
            
            clientBtn.onClick.RemoveAllListeners();    
            clientBtn.onClick.AddListener(HandleClientClick);
        }

        private void HandleHostClick()
        {
            NetworkManager.Singleton.StartHost();
        }
        
        private void HandleClientClick()
        {
            NetworkManager.Singleton.StartClient();
        }
        
        
    }
}