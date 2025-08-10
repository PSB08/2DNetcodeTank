using System;
using UnityEngine;
using UnityEngine.UI;

namespace TankCode.UI
{
    public class GameExit : MonoBehaviour
    {
        private Button _exitBtn;

        private void Awake()
        {
            _exitBtn = GetComponent<Button>();
        }

        private void Start()
        {
            _exitBtn.onClick.AddListener(ExitGame);
        }

        private void OnDestroy()
        {
            _exitBtn.onClick.RemoveAllListeners();
        }

        private void ExitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
    }
}