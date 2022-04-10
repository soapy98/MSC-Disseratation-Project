using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tanks;
using Tanks.Shared;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.MonoBehaviours.Core;
using UnityEngine.UIElements;

namespace Tanks.Server
{
    [RequireComponent(typeof(RoleTeamSelectData))]
    public class ServerRoleTeamSelectState : SceneStateBehaviour
    {
        private ServerNetworkPortal _network;

        // Start is called before the first frame update
        protected override GameState ActiveState => GameState.RoleTeamSelect;

        private RoleTeamSelectData RoleTeamSelectData { get; set; }

        private void Awake()
        {
            _network = NetworkPortal.m_NetworkPortal.GetComponent<ServerNetworkPortal>();
            RoleTeamSelectData = GetComponent<RoleTeamSelectData>();
        }

        private void ClientRoleWithTeamChange(ulong id, int roleidx, int team, bool locked)
        {
            var idx = FindLobbyPlayerId(id);
            if (idx == -1)
            {
                locked = false;
                return;
            }

            if (RoleTeamSelectData.LobbyClosed.Value)
                return;
            if (RoleTeamSelectData.RolePlayers.Any(info =>
                info.playerID != id && info.SeatState == RoleTeamSelectData.RoleSeatState.LockedIn))
            {
                RoleTeamSelectData.RolePlayers[idx] = new RoleTeamSelectData.RolePlayerState(id,
                    RoleTeamSelectData.RolePlayers[idx].PlayerName,
                    RoleTeamSelectData.RolePlayers[idx].PlayerNum, RoleTeamSelectData.RoleSeatState.Inactive,
                    team,
                    RoleTeamSelectData.RolePlayers[idx].SeatIdx);
                return;
            }

            RoleTeamSelectData.RolePlayers[idx] = new RoleTeamSelectData.RolePlayerState(id,
                RoleTeamSelectData.RolePlayers[idx].PlayerName,
                RoleTeamSelectData.RolePlayers[idx].PlayerNum,
                locked ? RoleTeamSelectData.RoleSeatState.LockedIn : RoleTeamSelectData.RoleSeatState.Active, team,
                roleidx, Time.time);
            if (locked)
            {
                for (var i = 0; i < RoleTeamSelectData.RolePlayers.Count; ++i)
                {
                    if (RoleTeamSelectData.RolePlayers[i].SeatIdx == idx && i != idx)
                    {
                        RoleTeamSelectData.RolePlayers[i] = new RoleTeamSelectData.RolePlayerState(
                            RoleTeamSelectData.RolePlayers[idx].playerID,
                            RoleTeamSelectData.RolePlayers[idx].PlayerName,
                            RoleTeamSelectData.RolePlayers[idx].PlayerNum,
                            RoleTeamSelectData.RoleSeatState.Inactive, team, roleidx);
                    }
                }
            }

            LobbyClose();
        }

        private int FindLobbyPlayerId(ulong id)
        {
            for (var i = 0; i < RoleTeamSelectData.RolePlayers.Count; ++i)
            {
                if (RoleTeamSelectData.RolePlayers[i].playerID == id)
                {
                    return i;
                }
            }

            return -1;
        }

        private void LobbyClose()
        {
            if (RoleTeamSelectData.RolePlayers.Any(info => info.SeatState != RoleTeamSelectData.RoleSeatState.LockedIn)
            ) return;
            RoleTeamSelectData.LobbyClosed.Value = true;
            LobbySave();
            StartCoroutine(LobbyEndWait());
        }

        private void LobbySave()
        {
            var lobbyResults = new PlayerSetUpResults();
            foreach (var playerInfo in RoleTeamSelectData.RolePlayers)
            {
                lobbyResults.choices[playerInfo.playerID] = new PlayerSetUpResults.PlayerSelectChoice(
                    playerInfo.PlayerNum,
                    RoleTeamSelectData.RoleSeatConfigs[playerInfo.SeatIdx].roleSpriteidx, playerInfo.PlayerTeam,
                    RoleTeamSelectData.RoleSeatConfigs[playerInfo.SeatIdx].Role);
            }

            GameStateRelayObj.RelayObjectSet = lobbyResults;
        }

        private static IEnumerator LobbyEndWait()
        {
            yield return new WaitForSeconds(5);
            MLAPI.SceneManagement.NetworkSceneManager.SwitchScene("Game");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!NetworkManager.Singleton) return;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnConnectClient;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

        public override void NetworkStart()
        {
            base.NetworkStart();
            if (!IsServer) enabled = false;
            else
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnConnectClient;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
                RoleTeamSelectData.OnClientRoleWithTeamChange += ClientRoleWithTeamChange;
                if (!IsHost) return;
                var clients = NetworkManager.Singleton.ConnectedClientsList;
                foreach (var net_cl in clients)
                    OnConnectClient(net_cl.ClientId);
            }
        }

        private void OnConnectClient(ulong id)
        {
            StartCoroutine(WaitToSelectNewRole(id));
        }

        private IEnumerator WaitToSelectNewRole(ulong id)
        {
            yield return new WaitForSeconds(2.5f);
            SeatNewPlayerRole(id);
        }

        private int GetPlayerAmount()
        {
            for (var possiblePlayerNum = 0; possiblePlayerNum < 6; ++possiblePlayerNum)
            {
                var found = RoleTeamSelectData.RolePlayers.Any(playerState => playerState.PlayerNum == possiblePlayerNum);
                if (!found)
                {
                    return possiblePlayerNum;
                }
            }

            return -1;
        }

        private void SeatNewPlayerRole(ulong id)
        {
            var playerNum = GetPlayerAmount();
            if (playerNum == -1)
            {
                // we ran out of seats... there was no room!
                RoleTeamSelectData.FatalLobbyErrorClientRpc(RoleTeamSelectData.FatalLobby.LobbyFull,
                    new ClientRpcParams {Send = new ClientRpcSendParams {TargetClientIds = new ulong[] {id}}});
                return;
            }

            var playerName = _network.GetPlayerName(id, playerNum);
            RoleTeamSelectData.RolePlayers.Add(new RoleTeamSelectData.RolePlayerState(id, playerName, playerNum,
                RoleTeamSelectData.RoleSeatState.Inactive));
        }

        private void OnClientDisconnectCallback(ulong id)
        {
            for (var i = 0; i < RoleTeamSelectData.RolePlayers.Count; ++i)
            {
                if (RoleTeamSelectData.RolePlayers[i].playerID != id) continue;
                RoleTeamSelectData.RolePlayers.RemoveAt(i);
                break;
            }
        }
    }
}