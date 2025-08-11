using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class TankSelectUI : NetworkBehaviour
    {
        [SerializeField] private Image tankImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button[] colorButtons;
        [SerializeField] private Button readyBtn;
        [SerializeField] private Image statusImage;

        [SerializeField] private Sprite readyImage;
        [SerializeField] private Sprite notReadyImage;
        [SerializeField] private Sprite readyBtnImage;
        [SerializeField] private Sprite notReadyBtnImage;

        public bool isReady = false;
        public Color selectedColor;

        public delegate void ReadyStatusChanged();

        public delegate void Disconnected(TankSelectUI ui);

        private TextMeshProUGUI _readyBtnText;
        private Image _readyBtnAttachedImage;
        private NetworkVariable<FixedString32Bytes> playerName = new();

        public event ReadyStatusChanged OnReadyChanged;
        public event Disconnected OnDisconnected;


        private void Awake()
        {
            _readyBtnText = readyBtn.GetComponentInChildren<TextMeshProUGUI>();
            _readyBtnAttachedImage = readyBtn.GetComponent<Image>();

            playerName.OnValueChanged += HandlePlayerNameChanged;
        }
        
        private void HandlePlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            nameText.text = newValue.ToString();
        }

        public void SetTankName(string name)
        {
            playerName.Value = name;
        }

        public override void OnNetworkSpawn()
        {
            nameText.text = playerName.Value.ToString();
            
            if (!IsOwner) return;
            
            readyBtn.onClick.AddListener(HandleReadyBtnClick);

            isReady = false;
            SetReadyStatusVisual();

            foreach (Button button in colorButtons)
            {
                Image buttonImage = button.GetComponent<Image>();
                button.onClick.AddListener(() =>
                {
                    SetTankColor(buttonImage.color);
                });
            }
        }
        
        public override void OnNetworkDespawn()
        {
            if(IsServer)
            {
                OnDisconnected?.Invoke(this);
            }

            if (!IsOwner) return;
        
            readyBtn.onClick.RemoveListener(HandleReadyBtnClick);

            foreach (Button button in colorButtons)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        
        
        private void HandleReadyBtnClick()
        {
            isReady = !isReady;

            SetReadyStatusVisual();
            SetReadyClaimToServerRpc(isReady);
        }

        private void SetTankColor(Color color)
        {
            SetTankVisualize(color);
            SetColorClaimToServerRpc(color);
        }

        private void SetTankVisualize(Color color)
        {
            tankImage.color = color;
        }

        private void SetReadyStatusVisual()
        {
            if (isReady)
            {
                statusImage.sprite = readyImage;
                _readyBtnAttachedImage.sprite = readyBtnImage;
                _readyBtnText.text = "준비완료";
            }
            else
            {
                statusImage.sprite = notReadyImage;
                _readyBtnAttachedImage.sprite = notReadyBtnImage;
                _readyBtnText.text = "준비";
            }
        }

        #region RPC Region
        [ServerRpc]
        public void SetColorClaimToServerRpc(Color color)
        {
            selectedColor = color; //서버의 컬러 변경
            SetColorClientRpc(color);
        }
        [ClientRpc]
        public void SetColorClientRpc(Color color)
        {
            if (IsOwner) return;
            SetTankVisualize(color);
        }

        [ServerRpc]
        public void SetReadyClaimToServerRpc(bool value)
        {
            isReady = value;
            SetReadyClientRpc(isReady);
            OnReadyChanged?.Invoke();
        }

        [ClientRpc]
        public void SetReadyClientRpc(bool value)
        {
            if (IsOwner) return;
            isReady = value;
            SetReadyStatusVisual();
        }

        #endregion
        
        
        
    }
}