using System;
using Scripts.Combat;
using Scripts.Core.Network.Shared;
using Scripts.Networking;
using TMPro;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Players
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("References")] 
        [SerializeField] private CinemachineCamera followCam;
        [SerializeField] private TextMeshPro nameText;

        [Header("Setting Values")] 
        [SerializeField] private int ownerCamPriority;

        public static event Action<PlayerController> OnPlayerSpawned;
        public static event Action<PlayerController> OnPlayerDeSpawned;
        
        public NetworkVariable<Color> tankColor;

        public PlayerVisual VisualCompo { get; private set; }
        public PlayerMovement MovementCompo { get; private set; }
        public TankHealth HealthCompo { get; private set; }
        public CoinCollector CoinCompo { get; private set; }
        public NetworkVariable<FixedString32Bytes> playerName;

        private void Awake()
        {
            tankColor = new NetworkVariable<Color>();
            playerName = new NetworkVariable<FixedString32Bytes>();
            VisualCompo = GetComponent<PlayerVisual>();
            MovementCompo = GetComponent<PlayerMovement>();
            HealthCompo = GetComponent<TankHealth>();
            CoinCompo = GetComponent<CoinCollector>();
        }

        public void SetTankData(Color color)
        {
            tankColor.Value = color;
        }

        public override void OnNetworkSpawn()
        {
            tankColor.OnValueChanged += HandleColorChanged;
            playerName.OnValueChanged += HandlePlayerNameChange;
            if(IsOwner)
            {
                followCam.Priority = ownerCamPriority;
            }
            
            if (IsServer)
            {
                UserData data = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
                playerName.Value = data.username;
                
                OnPlayerSpawned?.Invoke(this);
            }
            HandlePlayerNameChange(string.Empty, playerName.Value); //처음한번
        }

        private void HandleColorChanged(Color previousColor, Color newValue)
        {
            VisualCompo.SetTankColor(newValue);
        }
        
        public override void OnNetworkDespawn()
        {
            tankColor.OnValueChanged -= HandleColorChanged;
            playerName.OnValueChanged -= HandlePlayerNameChange;
            
            if(IsServer)
            {
                OnPlayerDeSpawned?.Invoke(this);
            }
        }

        private void HandlePlayerNameChange(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            nameText.text = newValue.ToString();
        }
        
    }
}