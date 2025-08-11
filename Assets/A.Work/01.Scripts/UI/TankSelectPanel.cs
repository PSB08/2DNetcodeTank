using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class TankSelectPanel : NetworkBehaviour
    {
        [SerializeField] private RectTransform panelTrm;
        [SerializeField] private Button startButton;
        [SerializeField] private List<TankSelectUI> selectUIList;

        private void Awake()
        {
            selectUIList = new List<TankSelectUI>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsHost)
            {
                startButton.onClick.AddListener(HandleGameStart);
                HandleReadyChanged();
            }
            else
            {
                startButton.gameObject.SetActive(false);
            }
        }

        
        private void HandleGameStart()
        {
            GameManager.Instance.StartGame(selectUIList);
            GameStartClientRpc();//창닫으라는 명령
        }

        public void AddUI(TankSelectUI ui)
        {
            selectUIList.Add(ui);
            ui.OnDisconnected += HandleDisconnected;
            ui.OnReadyChanged += HandleReadyChanged;
        }

        public void RemoveUI(TankSelectUI ui)
        {
            selectUIList.Remove(ui);
        }

        private void HandleReadyChanged()
        {
            startButton.interactable = AllReady();
        }

        private void HandleDisconnected(TankSelectUI ui)
        {
            ui.OnDisconnected -= HandleDisconnected;
            RemoveUI(ui);
        }


        public bool AllReady()
        {
            //하나라도 false인게 없으면
            return selectUIList.Count > 0 && selectUIList.Any(x => x.isReady == false) == false;
        }

        [ClientRpc]
        private void GameStartClientRpc()
        {
            panelTrm.gameObject.SetActive(false);
        }
        
        
    }
}