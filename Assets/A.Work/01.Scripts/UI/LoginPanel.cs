using System;
using System.Text.RegularExpressions;
using DG.Tweening;
using Scripts.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TankCode.UI
{
    public class LoginPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_InputField _inputName;
        [SerializeField] private Button _btnLogin;

        private void Awake()
        {
            _btnLogin.interactable = false;
            _btnLogin.onClick.AddListener(HandleLoginBtnClick);
            _inputName.onValueChanged.AddListener(ValidateUserName);
        }

        private void ValidateUserName(string name)
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9]{2,12}$");
            bool success = regex.IsMatch(name);

            _btnLogin.interactable = success;
        }
        
        private void HandleLoginBtnClick()
        {
            ClientSingleton.Instance.GameManager.SetPlayerName(_inputName.text);
            _canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            });
        }

        
    }
}