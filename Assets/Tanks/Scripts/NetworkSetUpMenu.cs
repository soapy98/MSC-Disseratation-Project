using System;
using System.Collections;
using System.Collections.Generic;
using Tanks.Client;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;


namespace Tanks.Shared
{
    public class NetworkSetUpMenu : MonoBehaviour
    {
        [SerializeField] private NetworkSetUpPanel m_SetUpPanel;
        private const string m_defaultIP = "127.0.0.1";
        private NetworkPortal m_gamePortal;
        private ClientNetworkPortal m_clientPortal;
        private const int m_ConnectPort = 9998;
        private void Start()
        {
            GameObject PortalGo = GameObject.FindGameObjectWithTag("NetworkPortal");
            m_SetUpPanel.MDefaultIp = m_defaultIP;
            m_SetUpPanel.MDefaultPort = m_ConnectPort;
            m_gamePortal = PortalGo.GetComponent<NetworkPortal>();
            m_clientPortal = PortalGo.GetComponent<ClientNetworkPortal>();
            ConnectionStatusMessgae(m_clientPortal.DisconnectReason.Reason,false);
            m_clientPortal.DisconnectReason.Clear();
            NetworkPortal.m_NetworkPortal.LocalPlayerName = GameManager._GameManager.Name;
        }
        private void ConnectionStatusMessgae(ConnectionStatus status, bool connection)
        {
            switch (status)
            {
                case ConnectionStatus.Unitialized:
                case ConnectionStatus.RequestedDisconnect:
                    break;
                case ConnectionStatus.NoRoomInServer:
                    break;
                case ConnectionStatus.Connected:
                    break;
                case ConnectionStatus.LoggedInAgain:
                    break;
                case ConnectionStatus.NormalDisconnect:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}