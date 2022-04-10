using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using Tanks.Shared;
using UnityEngine;
using MLAPI.SceneManagement;
using Tanks.Shared;
using UnityEngine.SceneManagement;

namespace Tanks.Server
{
    //Handles Actions related to the Server 
    public struct PlayerData
    {
        public readonly string m_PlayerName;
        public ulong m_PlayerID;

        public PlayerData(string name, ulong id, int team = 0)
        {
            m_PlayerName = name;
            m_PlayerID = id;
        }
    }

    public class ServerNetworkPortal : MonoBehaviour
    {
        private NetworkPortal m_net;
        [SerializeField] public Dictionary<string, PlayerData> m_PlayerData;
        [SerializeField] public Dictionary<ulong, string> m_ClientIDtoGuid;
        private Dictionary<int, string> m_TeamPlayerMap = new Dictionary<int, string>();
        private const int m_MaxPayload = 1024;
        [SerializeField] public Dictionary<ulong, int> m_ClientSceneMap = new Dictionary<ulong, int>();
        private static int ServerScene => SceneManager.GetActiveScene().buildIndex;

        private void Start()
        {
            m_net = GetComponent<NetworkPortal>();
            m_net.NetworkInit += OnNetworkReady;
            m_net.Net.ConnectionApprovalCallback += ApprovalCheck;
            m_net.Net.OnServerStarted += ServerStartedHandler;
            m_PlayerData = new Dictionary<string, PlayerData>();
            m_ClientIDtoGuid = new Dictionary<ulong, string>();
        }

        private void OnDestroy()
        {
            if (m_net == null) return;
            m_net.NetworkInit -= OnNetworkReady;
            if (m_net.Net == null) return;
            m_net.Net.ConnectionApprovalCallback -= ApprovalCheck;
            m_net.Net.OnServerStarted -= ServerStartedHandler;
        }

        private void OnNetworkReady()
        {
            if (!m_net.Net.IsServer)
                enabled = false;
            else
            {
                m_net.PlayerRequestToDisconnect += UserDisconnectRequest;
                m_net.Net.OnClientConnectedCallback += OnClientDisconnect;
                m_net.ChangeOfSceneClient += OnChangeOfSceneClient;
                if (m_net.Net.IsHost) m_ClientSceneMap[m_net.Net.LocalClientId] = ServerScene;
            }
        }

        private void OnClientDisconnect(ulong id)
        {
            m_ClientSceneMap.Remove(id);
            if (m_ClientIDtoGuid.TryGetValue(id, out var guid))
            {
                m_ClientIDtoGuid.Remove(id);
                if (m_PlayerData[guid].m_PlayerID == id) m_PlayerData.Remove(guid);
            }

            if (id != m_net.Net.LocalClientId) return;
            m_net.PlayerRequestToDisconnect -= UserDisconnectRequest;
            m_net.Net.OnClientDisconnectCallback -= OnClientDisconnect;
            m_net.ChangeOfSceneClient -= OnChangeOfSceneClient;
        }

        private void OnChangeOfSceneClient(ulong id, int sceneIndex)
        {
            m_ClientSceneMap[id] = sceneIndex;
        }

        private void UserDisconnectRequest()
        {
            if (m_net.Net.IsServer) m_net.Net.StopServer();
            Clear();
        }

        private void Clear()
        {
            m_PlayerData.Clear();
            m_ClientSceneMap.Clear();
            m_ClientIDtoGuid.Clear();
        }

        public bool AllClientsInServerScene()
        {
            return m_ClientSceneMap.All(whichScene => whichScene.Value == ServerScene);
        }

        public bool ClientInServerScene(ulong id)
        {
            return m_ClientSceneMap.TryGetValue(id, out int clientScene) && clientScene == ServerScene;
        }

        private PlayerData? GetPlayerData(ulong id)
        {
            if (m_ClientIDtoGuid.TryGetValue(id, out var playerguid))
            {
                if (m_PlayerData.TryGetValue(playerguid, out var data)) return data;
                Debug.Log("No PlayerData of matching guid found");
            }
            else Debug.Log("No client guid found mapped to the given client ID");

            return null;
        }

        public string GetPlayerName(ulong id, int playerNum)
        {
            var data = GetPlayerData(id);
            return data != null ? data.Value.m_PlayerName : ("Player" + playerNum);
        }

        private void ApprovalCheck(byte[] connectionData, ulong id, NetworkManager.ConnectionApprovedDelegate callback)
        {
            if (connectionData.Length > m_MaxPayload)
            {
                callback(false, 0, false, null, null);
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPacket>(payload);
            var scene = connectionPayload.clientCurrentScene;
            var gameStatus = ConnectionStatus.Connected;

            if (m_PlayerData.ContainsKey(connectionPayload.clientGUID))
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"Player GUID {connectionPayload.clientGUID} exists in game as this is a debug build");
                    while (m_PlayerData.ContainsKey(connectionPayload.clientGUID))
                        connectionPayload.clientGUID += "_Secondary";
                }
                else
                {
                    var oldClientId = m_PlayerData[connectionPayload.clientGUID].m_PlayerID;
                    StartCoroutine(WaitToDisconnectClient(oldClientId, ConnectionStatus.LoggedInAgain));
                }
            }

            if (m_PlayerData.Count >= RoleTeamSelectData.m_MaxLobbyPlayers)
                gameStatus = ConnectionStatus.NoRoomInServer;
            if (gameStatus == ConnectionStatus.Connected)
            {
                m_ClientSceneMap[id] = scene;
                m_ClientIDtoGuid[id] = connectionPayload.clientGUID;
                m_PlayerData[connectionPayload.clientGUID] = new PlayerData(connectionPayload.clientName, id);
            }

            callback(false, 0, true, null, null);
            m_net.ServerToClientConnectResult(id, gameStatus);
            if (gameStatus != ConnectionStatus.Connected) StartCoroutine(WaitToDisconnectClient(id, gameStatus));
        }

        private IEnumerator WaitToDisconnectClient(ulong id, ConnectionStatus status)
        {
            m_net.ServerToClientSetDisconnectReason(id, status);
            yield return new WaitForSeconds(0);
            BootClient(id);
        }

        private void BootClient(ulong id)
        {
            var netObj = MLAPI.Spawning.NetworkSpawnManager.GetPlayerNetworkObject(id);
            if (netObj) netObj.Despawn(true);
            m_net.Net.DisconnectClient(id);
        }

        private void ServerStartedHandler()
        {
            m_PlayerData.Add("host_guid", new PlayerData(m_net.LocalPlayerName, m_net.Net.LocalClientId));
            m_ClientIDtoGuid.Add(m_net.Net.LocalClientId, "host_guid");
        }
    }
}