using System.Threading.Tasks;
using TankCode.System;
using UnityEngine;

namespace Scripts.Networking
{
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] private ClientSingleton clientPrefab;
        [SerializeField] private HostSingleton hostPrefab;
        
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject); //이녀석은 게임종료시까지 남아있는다.

            _ = LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        }

        private async Task LaunchInMode(bool isDedicated)
        {
            if (isDedicated == false)
            {
                //데디케이트 서버가 아닌 일반 클라이언트모드
                HostSingleton hostSingleton = Instantiate(hostPrefab, transform); //내 자식으로 추가
                hostSingleton.CreateHost();

                ClientSingleton clientSingleton = Instantiate(clientPrefab, transform);
                bool isAuthenticated = await clientSingleton.CreateClientAsync();

                //여기서 전환하는게 더 나을거 같다.
                if (isAuthenticated)
                {
                    clientSingleton.GameManager.ChangeScene(SceneNames.MenuScene);  //메뉴씬으로 전환함
                }
                else
                {
                    Debug.LogError("UGS Service error on now");
                }
                
            }
        }
        
        
    }
}