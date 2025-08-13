using System;
using System.Collections;
using DG.Tweening;
using PSB_Lib.Dependencies;
using Scripts.Core;
using Scripts.Networking;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.UI
{
    public class PlayerWinPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup winnerPanel;
        [SerializeField] private float slowMotionScale = 0.3f;
        [SerializeField] private float slowMotionDuration = 0.5f;
        [SerializeField] private float uiShowDuration = 0.5f;
        [SerializeField] private float uiScalePunch = 0.2f;
        [SerializeField] private float returnSpeed = 1f;

        [Inject] private GameResultManager _resultManager;

        private void Awake()
        {
            _resultManager.OnWinGame += HandleWinPlayerPanel;
            winnerPanel.alpha = 0;
            winnerPanel.interactable = false;
            winnerPanel.blocksRaycasts = false;
            winnerPanel.transform.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            _resultManager.OnWinGame -= HandleWinPlayerPanel;
        }

        private void HandleWinPlayerPanel(string winner)
        {
            Time.timeScale = slowMotionScale;

            winnerPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"{winner} 승리!";
            winnerPanel.interactable = true;
            winnerPanel.blocksRaycasts = true;

            winnerPanel.alpha = 0;
            winnerPanel.transform.localScale = Vector3.one * (1f - uiScalePunch);

            winnerPanel.DOFade(1f, uiShowDuration)
                .SetUpdate(true); // 타임스케일 영향을 받지 않도록

            winnerPanel.transform.DOScale(Vector3.one, uiShowDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, returnSpeed)
                        .SetEase(Ease.InOutQuad);
                    StartCoroutine(ExitGameCoroutine());
                });
        }

        private IEnumerator ExitGameCoroutine()
        {
            yield return new WaitForSecondsRealtime(3f);
            OnHandleExitGame();
        }
        
        private void OnHandleExitGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HostSingleton.Instance.GameManager.Shutdown();
            }

            ClientSingleton.Instance.GameManager.Disconnect();
        }
        
        
    }
}