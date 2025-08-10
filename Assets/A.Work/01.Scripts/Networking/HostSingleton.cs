using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace TankCode.Networking
{
    public class HostSingleton : MonoBehaviour
    {
        private static HostSingleton _instance;

        public static HostSingleton Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindFirstObjectByType<HostSingleton>();

                Debug.Assert(_instance != null, "No Client singleton");
                return _instance;
            }
        }

        public HostGameManager GameManager { get; private set; }
        
        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
        
        public void CreateHost()  //나중에 async가 들어가야 할 수도 있음
        {
            GameManager = new HostGameManager();
        }

        public string GetFirstIpAddress()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());  //Host는 내 컴퓨터를 말한다
            foreach (IPAddress ip in ipEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)  //IPV4 버전의 주소
                {
                    return ip.ToString();
                }
            }

            return null;
        }
        

    }
}