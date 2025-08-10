using System;
using Scripts.Combat;
using TMPro;
using Unity.Collections;
using UnityEngine;

namespace TankCode.UI
{
    public class KillFeedUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI killFeedText;

        private void Awake()
        {
            if (KillFeedManager.Instance != null)
            {
                killFeedText.text = KillFeedManager.Instance.syncedKillFeedText.Value.ToString();
            }
            else
            {
                killFeedText.text = "킬 피드 초기화 중...";
            }
        }

        private void OnEnable()
        {
            if (KillFeedManager.Instance != null)
            {
                KillFeedManager.Instance.syncedKillFeedText.OnValueChanged += OnKillFeedTextChanged;
                killFeedText.text = KillFeedManager.Instance.syncedKillFeedText.Value.ToString();
            }
        }

        private void OnDisable()
        {
            if (KillFeedManager.Instance != null)
            {
                KillFeedManager.Instance.syncedKillFeedText.OnValueChanged -= OnKillFeedTextChanged;
            }
        }

        private void OnKillFeedTextChanged(FixedString512Bytes oldValue, FixedString512Bytes newValue)
        {
            killFeedText.text = newValue.ToString();
        }
        
        
    }
}