using System;
using MLAPI;
using MLAPI.NetworkingManagerComponents.Core;
using MLAPI.Serialization.Pooled;
using MLAPI.Transports;
using MLAPI.Transports.UNET;
using UnityEngine;
using Tanks.Shared;
using UnityEngine.SceneManagement;
using NetworkSceneManager = MLAPI.SceneManagement.NetworkSceneManager;

namespace Tanks.Shared
{
    public enum ConnectionStatus
    {
        Unitialized,
        Connected,
        NoRoomInServer,
        LoggedInAgain,
        RequestedDisconnect,
        NormalDisconnect
    }

    [SerializeField]
    public class ConnectionPacket
    {
        public string clientGUID;
        public int clientCurrentScene = -1;
        public string clientName;
    }
    public enum NetworkStates
    {
        Unitialized,
        Intro,
        Lobby,
        Starting,
        Ready,
        Playing,
        GameOver
    }
    //Handles Actions related to networking

    public class NetworkPortal : MonoBehaviour
    {
        public GameObject NetworkManagerObject;
        public event Action NetworkInit;
        public event Action<ConnectionStatus> SuccessfulConnection;
        public event Action<ConnectionStatus> UserNetworkDisconnect;
        public event Action<ulong, int> ChangeOfSceneClient;
        public event Action PlayerRequestToDisconnect;
        public NetworkManager Net { get; private set; }
        public string LocalPlayerName;

        private NetworkStates _networkState = NetworkStates.Unitialized;
        public static NetworkPortal m_NetworkPortal;
        private bool initialized;
        private bool Host { get; set; }

        private NetworkStates NetworkState
        {
            get => _networkState;
            set => _networkState = value;
        }

        private void Init()
        {
            NetworkState = NetworkStates.Unitialized;
            DontDestroyOnLoad(gameObject);
            if (m_NetworkPortal == null) m_NetworkPortal = this;
            else if (m_NetworkPortal != this) Destroy(m_NetworkPortal);
        }

        private void Awake()
        {
            Init();
        }
        private void Start()
        {
            Net = NetworkManagerObject.GetComponent<NetworkManager>();
            GameManager._GameManager.NetPortal = gameObject;
            Net.OnServerStarted += OnNetworkReady;
            Net.OnClientConnectedCallback += ClientNetworkReadyWrapper;
            RegisterClientMessageHandlers();
            RegisterServerMessageHandlers();
        }

        private void OnDestroy()
        {
            if (Net != null)
            {
                Net.OnServerStarted -= OnNetworkReady;
                Net.OnClientConnectedCallback -= ClientNetworkReadyWrapper;
            }
            UnregisterClientMessageHandlers();
            UnregisterServerMessageHandlers();
        }

        private void HostGame()
        {
            NetworkManager.Singleton.StartHost();
            Host = NetworkManager.Singleton.IsHost;
            _networkState = NetworkStates.Lobby;
            NetworkSceneManager.SwitchScene("RoleTeamScene");
        }

        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            Host = NetworkManager.Singleton.IsHost;
            _networkState = NetworkStates.Lobby;
            NetworkSceneManager.SwitchScene("RoleTeamScene");
        }

        private void ClientNetworkReadyWrapper(ulong clientId)
        {
            if (clientId == Net.LocalClientId) OnNetworkReady();
        }

        private void RegisterClientMessageHandlers()
        {
            MLAPI.Messaging.CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientConnectResult",
                (senderClientId, stream) =>
                {
                    using var reader = PooledNetworkReader.Get(stream);
                    var status = (ConnectionStatus) reader.ReadInt32();
                    SuccessfulConnection?.Invoke(status);
                });

            MLAPI.Messaging.CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientSetDisconnectReason",
                (senderClientId, stream) =>
                {
                    using var reader = PooledNetworkReader.Get(stream);
                    var status = (ConnectionStatus) reader.ReadInt32();
                    UserNetworkDisconnect?.Invoke(status);
                });
        }

        private void RegisterServerMessageHandlers()
        {
            MLAPI.Messaging.CustomMessagingManager.RegisterNamedMessageHandler("ClientToServerSceneChanged",
                (senderClientId, stream) =>
                {
                    using var reader = PooledNetworkReader.Get(stream);
                    var sceneIndex = reader.ReadInt32();
                    ChangeOfSceneClient?.Invoke(senderClientId, sceneIndex);
                });
        }

        private void UnregisterClientMessageHandlers()
        {
            MLAPI.Messaging.CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientConnectResult");
            MLAPI.Messaging.CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientSetDisconnectReason");
        }

        private void UnregisterServerMessageHandlers()
        {
            MLAPI.Messaging.CustomMessagingManager.UnregisterNamedMessageHandler("ClientToServerSceneChanged");
        }

        private void OnNetworkReady()
        {
            if (Net.IsHost) SuccessfulConnection?.Invoke(ConnectionStatus.Connected);
            NetworkInit?.Invoke();
        }

        public void StartHost(string ipaddress, int port)
        {
            var chosenTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (chosenTransport == null) throw new Exception($"unhandled IpHost transport {chosenTransport.GetType()}");
            UNetTransport unetTransport = NetworkManagerObject.GetComponent<UNetTransport>()
                ? NetworkManagerObject.GetComponent<UNetTransport>()
                : NetworkManagerObject.AddComponent<UNetTransport>();
            unetTransport.ConnectAddress = ipaddress;
            unetTransport.ServerListenPort = port;
            HostGame();
        }

        public void ServerToClientConnectResult(ulong netId, ConnectionStatus status)
        {
            using var buffer = PooledNetworkBuffer.Get();
            using var writer = PooledNetworkWriter.Get(buffer);
            writer.WriteInt32((int) status);
            MLAPI.Messaging.CustomMessagingManager.SendNamedMessage("ServerToClientConnectResult", netId, buffer);
        }

        public void ServerToClientSetDisconnectReason(ulong netId, ConnectionStatus status)
        {
            using var buffer = PooledNetworkBuffer.Get();
            using var writer = PooledNetworkWriter.Get(buffer);
            writer.WriteInt32((int) status);
            MLAPI.Messaging.CustomMessagingManager.SendNamedMessage("ServerToClientSetDisconnectReason", netId,
                buffer);
        }

        public void ClientToServerChanged(int newScene)
        {
            if (Net.IsHost) ChangeOfSceneClient?.Invoke(Net.ServerClientId, newScene);
            else if (Net.IsConnectedClient)
            {
                using var buffer = PooledNetworkBuffer.Get();
                using var writer = PooledNetworkWriter.Get(buffer);
                writer.WriteInt32(newScene);
                MLAPI.Messaging.CustomMessagingManager.SendNamedMessage("ClientToServerSceneChanged",
                    Net.ServerClientId, buffer);
            }
        }

        public void RequestDisconnect()
        {
            if (PlayerRequestToDisconnect != null) PlayerRequestToDisconnect.Invoke();
        }
    }
}