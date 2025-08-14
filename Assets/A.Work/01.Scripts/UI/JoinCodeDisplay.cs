using System.Collections;
using Scripts.Networking;
using TMPro;
using UnityEngine;

namespace Scripts.UI
{
    public class JoinCodeDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI joinCodeText;
        [SerializeField] private float refreshInterval = 1f;

        private void Start()
        {
            StartCoroutine(UpdateJoinCodeRoutine());
        }

        private IEnumerator UpdateJoinCodeRoutine()
        {
            WaitForSecondsRealtime wait = new WaitForSecondsRealtime(refreshInterval);

            while (true)
            {
                if (HostSingleton.Instance.GameManager != null)
                {
                    string code = HostSingleton.Instance.GameManager.JoinCode;
                    joinCodeText.text = string.IsNullOrEmpty(code) ? "" : $"{code}";
                }
                yield return wait;
            }
        }
        
    }
}