using System.Collections.Generic;
using DG.Tweening;
using Scripts.Combat;
using TMPro;
using UnityEngine;

namespace Scripts.UI
{
    public class KillFeedUI : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        
        public static KillFeedUI Instance { get; private set; }

        [SerializeField] private Transform killListParent; // Vertical Layout Group 붙여서 자동 정렬
        [SerializeField] private GameObject killRowPrefab; // TMP 하나만 있는 프리팹

        private bool _isOpen = false;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            Instance = this;
        }
        
        private void Update()
        {
            CheckSystemInput();
        }

        private void CheckSystemInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _isOpen = !_isOpen;
                OpenWindow(_isOpen);
            }
        }
        
        private void OpenWindow(bool value)
        {
            float fadeValue = value ? 1f : 0;
            _canvasGroup.DOFade(fadeValue, 0.5f);

            _canvasGroup.blocksRaycasts = value;
            _canvasGroup.interactable = value;
        }
        
        public void RefreshUI(List<KillFeedManager.KillEntry> entries)
        {
            foreach (Transform child in killListParent)
                Destroy(child.gameObject);

            foreach (var entry in entries)
            {
                var row = Instantiate(killRowPrefab, killListParent);
                var tmp = row.GetComponent<TextMeshProUGUI>();

                // 이름과 킬 수를 줄바꿈으로 표시
                tmp.text = $"{entry.Name}\n\n{entry.Kills}";
            }
        }
        
        
    }
}