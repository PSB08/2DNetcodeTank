using DG.Tweening;
using Scripts.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class GameMenuPanel : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _exitButton;

        private bool _isOpen = false;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            _exitButton.onClick.AddListener(OnHandleExitGame);
            _closeButton.onClick.AddListener(OnHandleCloseWindow);
        }

        private void Update()
        {
            CheckSystemInput();
        }

        private void CheckSystemInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _isOpen = !_isOpen;
                OpenWindow(_isOpen);
            }
        }

        private void OpenWindow(bool value)
        {
            float fadeValue = value ? 1f : 0;
            _canvasGroup.DOFade(fadeValue, 0.5f);

            _canvasGroup.blocksRaycasts = value;
            _canvasGroup.interactable = value;
        }
        
        private void OnHandleExitGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HostSingleton.Instance.GameManager.Shutdown();
            }

            ClientSingleton.Instance.GameManager.Disconnect();
        }

        private void OnHandleCloseWindow()
        {
            _isOpen = !_isOpen;
            OpenWindow(_isOpen);
        }

    }
}