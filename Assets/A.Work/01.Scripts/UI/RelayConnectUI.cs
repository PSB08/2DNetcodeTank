using Scripts.Networking;
using Scripts.System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TankCode.UI
{
    public class RelayConnectUI : MonoBehaviour
    {
        [SerializeField] private Button hostBtn;
        [SerializeField] private Button joinBtn;
        [SerializeField] private Button enterLobbyBtn;
        [SerializeField] private TMP_InputField joinCodeInput;

        public UnityEvent OpenLobbyEvent;
        
        private void Awake()
        {
            hostBtn.onClick.AddListener(HandleRelayHostClick);
            joinBtn.onClick.AddListener(HandleJoinClick);
            enterLobbyBtn.onClick.AddListener(HandleEnterLobbyClick);
        }

        private void HandleEnterLobbyClick()
        {
            OpenLobbyEvent?.Invoke();
        }

        private async void HandleRelayHostClick()
        {
            bool result = await HostSingleton.Instance.GameManager.StartHostAsync();
            if (result)
            {
                HostSingleton.Instance.GameManager.ChangeNetworkScene(SceneNames.GameScene);
            }
            else
            {
                Debug.LogError("Relay 호스트 생성 중 오류가 발생했습니다.");
            }
        }

        private async void HandleJoinClick()
        {
            string joinCode = joinCodeInput.text;
            if (string.IsNullOrEmpty(joinCode)) return;

            bool result = await ClientSingleton.Instance.GameManager.StartClientWithJoinCode(joinCode);

            if (result == false)
            {
                Debug.LogError("Join실패");
            }
        }
        
        
        
        
    }
}