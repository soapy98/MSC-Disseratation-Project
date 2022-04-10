using System;
using System.Collections;
using System.Collections.Generic;
using Tanks.Client;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tanks.Client
{
    // Handles Actions related to a Clients Networking
    public class ClientNetworkPortal : MonoBehaviour
    {
        private NetworkPortal m_NetworkPortal;
        public Disconnect DisconnectReason { get; private set; } = new Disconnect();
        private const int m_Timeout = 10;
        public event Action<ConnectionStatus> ConnectionFinsihed;

        public const int m_timeout = 10;

        private void Start()
        {
            m_NetworkPortal = NetworkPortal.m_NetworkPortal;
            m_NetworkPortal.NetworkInit += NetworkReady;
            m_NetworkPortal.SuccessfulConnection += ConnectFinsihed;
            m_NetworkPortal.UserNetworkDisconnect += UserNetworkDisconnectInfoReceieved;
            m_NetworkPortal.Net.OnClientDisconnectCallback += DisconnectTimeout;
        }

        private void OnDestroy()
        {
            if (m_NetworkPortal != null)
            {
                m_NetworkPortal.NetworkInit -= NetworkReady;
                m_NetworkPortal.SuccessfulConnection -= ConnectFinsihed;
                m_NetworkPortal.UserNetworkDisconnect -= UserNetworkDisconnectInfoReceieved;

                if (m_NetworkPortal.Net != null) m_NetworkPortal.Net.OnClientDisconnectCallback -= DisconnectTimeout;
            }
        }

        private void NetworkReady()
        {
            if (!m_NetworkPortal.Net.IsClient)
            {
                enabled = false;
            }
            else
            {
                if (!m_NetworkPortal.Net.IsHost) m_NetworkPortal.PlayerRequestToDisconnect += UserDisconnectRequest;
                SceneManager.sceneLoaded += SceneLoaded;
            }
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            m_NetworkPortal.ClientToServerChanged(SceneManager.GetActiveScene().buildIndex);
        }

        private void ConnectFinsihed(ConnectionStatus status)
        {
            Debug.Log("RecvConnectFinished Got status: " + status);
            if (status != ConnectionStatus.Connected)
            {
                DisconnectReason.SetReasonForDisconnect(status);
            }

            ConnectionFinsihed?.Invoke(status);
        }

        private void UserDisconnectRequest()
        {
            if (m_NetworkPortal.Net.IsClient)
            {
                DisconnectReason.SetReasonForDisconnect(ConnectionStatus.RequestedDisconnect);
                m_NetworkPortal.Net.StopClient();
            }
        }

        private void UserNetworkDisconnectInfoReceieved(ConnectionStatus status)
        {
            DisconnectReason.SetReasonForDisconnect(status);
        }

        private void DisconnectTimeout(ulong id)
        {
            if (!NetworkPortal.m_NetworkPortal.Net.IsConnectedClient && !MLAPI.NetworkManager.Singleton.IsHost)
            {
                SceneManager.sceneLoaded -= SceneLoaded;
                m_NetworkPortal.PlayerRequestToDisconnect -= UserDisconnectRequest;
                if (SceneManager.GetActiveScene().name != "Menu")
                {
                    MLAPI.NetworkManager.Singleton.Shutdown();
                    if (!DisconnectReason.HasReasonDisconnect)
                    {
                        DisconnectReason.SetReasonForDisconnect(ConnectionStatus.NormalDisconnect);
                    }
                    SceneManager.LoadScene("Menu");
                }
            }
        }

        public static void StartLocalClient(string ip, int port)
        {
            var chosenTransport=NetworkPortal.m_NetworkPortal.Net.NetworkConfig.NetworkTransport;
            switch (chosenTransport)
            {
         
                case MLAPI.Transports.UNET.UNetTransport unetTransport:
                    unetTransport.ConnectAddress = ip;
                    unetTransport.ServerListenPort = port;
                    break;
                default:
                    throw new Exception($"unhandled IpHost transport {chosenTransport.GetType()}");
            }
            ClientConnect();
        }

        private static void ClientConnect()
        {
            var localGUID = ClientPreInfo.GetGuid();
            var packet = JsonUtility.ToJson(new ConnectionPacket()
            {
                clientGUID = localGUID,
                clientCurrentScene = SceneManager.GetActiveScene().buildIndex,
                clientName = NetworkPortal.m_NetworkPortal.LocalPlayerName
            });
            var packetBytes = System.Text.Encoding.UTF8.GetBytes(packet);
            NetworkPortal.m_NetworkPortal.Net.NetworkConfig.ConnectionData = packetBytes;
            NetworkPortal.m_NetworkPortal.Net.NetworkConfig.ClientConnectionBufferTimeout = m_Timeout;
            NetworkPortal.m_NetworkPortal.StartClient();
        }
    }
}