using System;
using System.Collections;
using System.Collections.Generic;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tanks.Client
{
    //Responsible for allowing a client to enter a ip and port
    public class NetworkEnterPanel : MonoBehaviour
    {
        [SerializeField] public InputField m_IpInput;
        [SerializeField] public InputField m_PortInput;

        [SerializeField] public Button m_ConfirmButton;
        [SerializeField] public Button m_cancelButton;
        [SerializeField] public Button m_ChangeIPButton;
        [SerializeField] public Button m_ChangePortButton;

        private string m_IpHostText;
        public bool m_IsHost;
        private string m_defaultIpInput;
        private int m_Port;
        [SerializeField] public Action<string, int> m_ConfirmFunction;
        private void ChangeIp()
        {
        }

        private void ChangePort()
        {
        }

        public void GameSetUpDisplay(bool host, string IpInput, Action<string, int> confirmCallback,
            int defaultPort)
        {
            Reset();
            m_IsHost = host;
            m_defaultIpInput = IpInput;
            m_IpInput.text = m_defaultIpInput;
            m_Port = defaultPort;
            m_PortInput.text = m_Port.ToString();
            m_ConfirmFunction = confirmCallback;
            m_ChangeIPButton.onClick.AddListener(ChangeIp);
            m_ChangePortButton.onClick.AddListener(ChangePort);
            m_ConfirmButton.onClick.AddListener(OnConfirmClick);
            m_cancelButton.onClick.AddListener(OnCancelClick);
            m_ConfirmButton.gameObject.SetActive(true);
            m_cancelButton.gameObject.SetActive(true);
            m_IpInput.gameObject.SetActive(true);
            m_PortInput.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        private void OnConfirmClick()
        {
            int.TryParse(m_PortInput.text, out var port);
            if (port <= 0)
                port = m_Port;
            m_ConfirmFunction.Invoke(m_defaultIpInput, port);
        }

        private void OnCancelClick()
        {
            Reset();
        }

        private void Reset()
        {
            m_IpInput.text = string.Empty;
            m_PortInput.text = string.Empty;
            m_ConfirmButton.gameObject.SetActive(false);
            m_cancelButton.gameObject.SetActive(false);
            m_cancelButton.onClick.RemoveListener(OnCancelClick);
            m_ConfirmButton.onClick.RemoveListener(OnConfirmClick);
        }
    }
}