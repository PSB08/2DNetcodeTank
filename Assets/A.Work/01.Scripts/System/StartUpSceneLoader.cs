#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Scripts.System
{
    //Static 생성자를 실행해줌 - 실행 타이밍 : 컴파일 때 한 번, 플레이 모드 진입 시 한 번 (Awake 이전에)
    [InitializeOnLoad]
    public static class StartUpSceneLoader
    {
        static StartUpSceneLoader()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
        }

        private static void HandlePlayModeChanged(PlayModeStateChange changeState)
        {
            if (changeState == PlayModeStateChange.EnteredEditMode)
            {
                if (SceneManager.GetActiveScene().buildIndex != 0)
                {
                    EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(0));
                }
            }

            if (changeState == PlayModeStateChange.EnteredPlayMode)
            {
                if (SceneManager.GetActiveScene().buildIndex != 0)
                {
                    SceneManager.LoadScene(0);
                }
            }
            
        }
        
        
    }
}
#endif