using System;
using System.Collections;
using System.Collections.Generic;
using Tanks.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Tanks.Client
{
    //Allows a client to choose to host or join a game
    public class NetworkSetUpPanel : MonoBehaviour
    {
        [SerializeField] public NetworkEnterPanel m_enterPanel;
        [SerializeField] public Button m_JoinGame;
        [SerializeField] public Button m_HostGame;
        private Action<string, int> m_ConfirmFunction;
        [SerializeField] private string m_DefaultIP;
        [SerializeField] private int m_DefaultPort;
        public string MDefaultIp
        {
            set => m_DefaultIP = value;
        }
        public int MDefaultPort
        {
            get => m_DefaultPort;
            set => m_DefaultPort = value;
        }
        private void Start()
        {
            m_JoinGame.onClick.AddListener(JoinGameSetUp);
            m_HostGame.onClick.AddListener(HostGameSetUp);
            m_enterPanel.gameObject.SetActive(false);
        }
        private void OnCancelClick()
        {
            Reset();
        }
        private void JoinGameSetUp()
        {
            gameObject.SetActive(false);
            m_enterPanel.GameSetUpDisplay(false, m_DefaultIP,
                ClientNetworkPortal.StartLocalClient, MDefaultPort);
        }
        private void HostGameSetUp()
        {
            gameObject.SetActive(false);
            m_enterPanel.GameSetUpDisplay(true, m_DefaultIP,
                (ip, port) => { NetworkPortal.m_NetworkPortal.StartHost(ip, port); },
                MDefaultPort);
        }

        private void Reset()
        {
            m_JoinGame.onClick.RemoveListener(JoinGameSetUp);
            m_HostGame.onClick.RemoveListener(HostGameSetUp);
        }

        
    }
}