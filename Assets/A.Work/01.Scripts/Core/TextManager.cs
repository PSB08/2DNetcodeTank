using Scripts.UI;
using UnityEngine;

namespace Scripts.Core
{
    public class TextManager : MonoBehaviour
    {
        [SerializeField] private GameText _textPrefab;

        private static TextManager _instance;
        public static TextManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = GameObject.Find("GameManager").GetComponent<TextManager>();
                }

                if(_instance == null)
                {
                    Debug.LogWarning("There are no TextManager in GameScene");
                }
                return _instance;
            }
        }

        public void PopupText(string value, Vector3 pos, Color color)
        {
            var text = Instantiate(_textPrefab, pos, Quaternion.identity);
            text.SetPopup(value, pos, color);
        }


    }
}