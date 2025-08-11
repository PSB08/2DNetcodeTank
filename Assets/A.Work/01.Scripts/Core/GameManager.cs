using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts.Combat;
using Scripts.Players;
using Scripts.UI;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Core
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private TankSelectUI selectUIPrefab;
        [SerializeField] private RectTransform selectPanelTrm;
        
        private TankSelectPanel _selectPanel;
        
        private bool _isGameStart = false;
        public bool GameStarted => _isGameStart;
        
        
        private void Awake()
        {
            Instance = this;
            _selectPanel = selectPanelTrm.GetComponent<TankSelectPanel>();
        }

        public void CreateUIPanel(ulong clientID, string userName)
        {
            TankSelectUI ui = Instantiate(selectUIPrefab);
            ui.SetTankName(userName);
            ui.NetworkObject.SpawnAsPlayerObject(clientID);
            ui.transform.SetParent(selectPanelTrm);
            ui.transform.localScale = Vector3.one;
            
            
            _selectPanel.AddUI(ui);
        }

        public void StartGame(List<TankSelectUI> list)
        {
            foreach (TankSelectUI ui in list)
            {
                ulong clientID = ui.OwnerClientId;
                Color color = ui.selectedColor;
                
                SpawnTank(clientID, color);
            }
            
            if (KillFeedManager.Instance != null)
            {
                KillFeedManager.Instance.InitializeKillFeed();
            }
            _isGameStart = true;
        }
        
        public async void SpawnTank(ulong clientID, Color selectedColor, float delay = 0)
        {
            if(delay > 0 )
            {
                await Task.Delay(Mathf.CeilToInt(delay * 1000));
            }

            Vector3 position = TankSpawnPoint.GetRandomSpawnPos();

            PlayerController controller = Instantiate(_playerPrefab, position, Quaternion.identity);
            controller.NetworkObject.SpawnAsPlayerObject(clientID);
            controller.SetTankData(selectedColor);
        }


    }
}