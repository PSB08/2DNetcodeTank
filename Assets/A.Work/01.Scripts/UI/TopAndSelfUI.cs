using System.Collections;
using System.Linq;
using DG.Tweening;
using Scripts.Players;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.UI
{
    public class TopAndSelfUI : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        
        public static TopAndSelfUI Instance { get; private set; }
        
        [Header("TopPlayer")]
        [SerializeField] private TextMeshProUGUI topRankText;
        [SerializeField] private TextMeshProUGUI topNameText;
        [SerializeField] private TextMeshProUGUI topKillText;

        [Header("Self")] 
        [SerializeField] private TextMeshProUGUI selfRankText;
        [SerializeField] private TextMeshProUGUI selfNameText;
        [SerializeField] private TextMeshProUGUI selfKillText;

        private bool _isOpen = false;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 1f;
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
            float fadeValue = value ? 0 : 1f;
            _canvasGroup.DOFade(fadeValue, 0.5f);

            _canvasGroup.blocksRaycasts = value;
            _canvasGroup.interactable = value;
        }
        
        public void SetData(string topName, int topKills, int topRank, string selfName, int selfKills, int selfRank)
        {
            topRankText.color = Color.yellow;
            topNameText.color = Color.yellow;
            topKillText.color = Color.yellow;
            
            topRankText.text = $"{topRank.ToString()}등";
            topNameText.text = topName;
            topKillText.text = topKills.ToString();
            
            selfRankText.text = $"{selfRank.ToString()}등";
            selfNameText.text = selfName;
            selfKillText.text = selfKills.ToString();
        }
        
        
    }
}