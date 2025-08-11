using System.Text.RegularExpressions;
using Scripts.Networking;
using TankCode.System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TankCode.UI
{
    public class IpConnectUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField ipInputField;
        [SerializeField] private TMP_InputField portInputField;

        [SerializeField] private Button hostBtn;
        [SerializeField] private Button clientBtn;
        
        private void Start()
        {
            if (NetworkManager.Singleton == null) return;
            
            portInputField.text = "7777";
            string firstIp = HostSingleton.Instance.GetFirstIpAddress();
            ipInputField.text = string.IsNullOrEmpty(firstIp) ? string.Empty : firstIp;
            
            hostBtn.onClick.AddListener(HandleHostBtnClick);
            clientBtn.onClick.AddListener(HandleClientBtnClick);
        }

        private void HandleHostBtnClick()
        {
            if (InputValidation() == false)
            {
                Debug.Log("올바르지 않은 IP와 Port입니다.");
                return;
            }

            SetUpTransport();

            if (NetworkManager.Singleton.StartHost())
            {
                NetworkManager.Singleton.SceneManager.LoadScene(SceneNames.GameScene, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("호스트 생성실패");
                NetworkManager.Singleton.Shutdown();  //연결 하던 거 종료 -> 그래야 다음에 다시 열 수 있음
            }
        }

        private void HandleClientBtnClick()
        {
            if (InputValidation() == false)
            {
                Debug.Log("올바르지 않은 IP와 Port입니다.");
                return;
            }

            SetUpTransport();

            if (NetworkManager.Singleton.StartClient())
            {
                return;
            }
            
            NetworkManager.Singleton.Shutdown();  //실패 시 셧다운 처리
        }

        private void SetUpTransport()
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            ushort portNum = ushort.Parse(portInputField.text);
            transport.SetConnectionData(ipInputField.text, portNum);
        }

        private bool InputValidation()
        {
            string ip = ipInputField.text;
            string port = portInputField.text;

            Regex ipReg = new Regex(@"^[0-9]{3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
            Regex portReg = new Regex(@"[0-9]{3,5}");

            Match ipMatch = ipReg.Match(ip);
            Match portMatch = portReg.Match(port);

            return ipMatch.Success && portMatch.Success;
        }
        
        
    }
}