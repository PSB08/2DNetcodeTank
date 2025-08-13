using System.Collections.Generic;
using Scripts.Combat;
using TMPro;
using UnityEngine;

namespace Scripts.UI
{
    public class KillFeedUI : MonoBehaviour
    {
        public static KillFeedUI Instance { get; private set; }

        [SerializeField] private Transform killListParent; // Vertical Layout Group 붙여서 자동 정렬
        [SerializeField] private GameObject killRowPrefab; // TMP 하나만 있는 프리팹

        private void Awake()
        {
            Instance = this;
        }

        public void RefreshUI(List<KillFeedManager.KillEntryDto> entries)
        {
            foreach (Transform child in killListParent)
                Destroy(child.gameObject);

            foreach (var entry in entries)
            {
                var row = Instantiate(killRowPrefab, killListParent);
                var tmp = row.GetComponent<TextMeshProUGUI>();

                // 이름과 킬 수를 줄바꿈으로 표시
                tmp.text = $"{entry.Name}\n{entry.Kills}";
            }
        }
        
        
    }
}