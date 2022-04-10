using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.MonoBehaviours.Core;
using MLAPI.NetworkVariable;
using MLAPI.Spawning;
using Tanks;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tanks.Server
{
    public class ServerMainSceneState : SceneStateBehaviour
    {
        [SerializeField] private NetworkObject m_LocalPlayerPrefab;
        private PlayerSetUpResults m_results;
        private ServerNetworkPortal m_ServerPortal;
        private const float m_GameOverDelay = 7.0f;
        private bool PlayersSpawned { get; set; }
        private List<ulong> m_PlayerIds = new List<ulong>();
        [SerializeField] private TankSpawnPoint Spawning;
        public TeamObjectSet Team1Objects;
        public TeamObjectSet Team2Objects;

        private PlayerSetUpResults.PlayerSelectChoice GetLobbySetUpForClients(ulong id)
        {
            if (m_results.choices.TryGetValue(id, out var result))
                return result;
            m_results.choices[id] = result = new PlayerSetUpResults.PlayerSelectChoice(-1, -1);
            return result;
        }

        protected override GameState ActiveState => GameState.MainGame;

        public override void NetworkStart()
        {
            base.NetworkStart();
            if (!IsServer) enabled = false;
            else
            {
                m_ServerPortal = NetworkPortal.m_NetworkPortal.GetComponent<ServerNetworkPortal>();
                NetworkPortal.m_NetworkPortal.ChangeOfSceneClient += OnChangeOfSceneClientChangeScene;
                var p = GameStateRelayObj.GetRelayObj();
                if (p != null && p.GetType() != typeof(PlayerSetUpResults))
                    throw new System.Exception("No PlayerSetup found!");
                m_results = (PlayerSetUpResults) p;
                Spawning.SpawnTanks();

                IntialSpawn();

                foreach (var tank in TankOnServer.m_ActiveTanks)
                {
                    tank.GetComponent<NetworkTankState>().NetworkTankLifeState.TankLifeState.OnValueChanged +=
                        OnTankLifeStateChange;
                    tank.GetComponent<NetworkTankState>().SetPlayerTeamMatesInfo();
                }
            }
        }

        private bool IntialSpawn()
        {
            if (!m_ServerPortal.AllClientsInServerScene() || PlayersSpawned ||
                NetworkPortal.m_NetworkPortal.Net.ConnectedClientsList.Count != m_results.choices.Count) return false;
            PlayersSpawned = true;
            foreach (var result in m_results.choices)
            {
                SpawnPlayer(result.Key);
            }

            return true;
        }

        void SpawnPlayer(ulong id)
        {
            var PlayerSetUpResults = GetLobbySetUpForClients(id);

            var TeamTankPost = Spawning.m_Tanks.GetTankOffTeam(PlayerSetUpResults.PlayerTeam).transform;
            var tank = TeamTankPost.gameObject;
            var state = tank.GetComponent<NetworkTankState>();
            var t = PlayerSetUpResults.PlayerTeam;


            var obj = t switch
            {
                1 => Team1Objects,
                2 => Team2Objects,
                _ => null
            };

            obj.SinglePlayer = NetworkPortal.m_NetworkPortal.Net.ConnectedClientsList.Count == 1;

            var newPlayer = Instantiate(m_LocalPlayerPrefab);
            // newPlayer.transform.position = obj.transform.position;
            var networkstate = newPlayer.GetComponent<NetworkPlayerState>();

            networkstate.SetPlayerUp(PlayerSetUpResults.PlayerRole, PlayerSetUpResults.RoleSprite, obj,
                PlayerSetUpResults.PlayerTeam);

            var playerName = m_ServerPortal.GetPlayerName(id, PlayerSetUpResults.PlayerNumber);
            
            networkstate.TankHealthState = tank.GetComponent<NetworkTankState>().Health;
            networkstate.TeamTankState = tank.GetComponent<NetworkTankState>();
            networkstate.PlayerTeamTank = tank.GetComponent<NetworkObject>();
            // networkstate.Team  = PlayerSetUpResults.PlayerTeam;
            networkstate.Name = playerName;
            networkstate.Role = PlayerSetUpResults.PlayerRole;
            networkstate.TeamObjs = obj;
            state.AddPlayer(networkstate);
            newPlayer.GetComponent<NetworkPlayerState>().EnergyPoints = 20;
            newPlayer.GetComponent<UIHealthLocalInfo>().InitalizeHealth(newPlayer.GetComponent<NetworkPlayerState>().NetworkEnergyState.HealthPoints,20);
            // switch (PlayerSetUpResults.PlayerRole)
            // {
            //     case DifferentRoles.Driver:
            //         networkstate.NetworkRoleEnergyState.EnergyState.OnValueChanged += obj.DriverBall.OnEnergyStateChange;
            //         break;
            //     case DifferentRoles.Gunner:
            //         networkstate.NetworkRoleEnergyState.EnergyState.OnValueChanged += obj.GunnerBall.OnEnergyStateChange;
            //         break;
            // }
            networkstate.RoleSet.Value = true;
            newPlayer.SpawnAsPlayerObject(id, null, true);
        }

        private void OnChangeOfSceneClientChangeScene(ulong id, int scene)
        {
            var CurrentSceneOnServer = SceneManager.GetActiveScene().buildIndex;
            if (scene != CurrentSceneOnServer) return;
            var spawned = IntialSpawn();
            if (!spawned && PlayersSpawned && NetworkSpawnManager.GetPlayerNetworkObject(id) == null)
            {
                SpawnPlayer(id);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var id in m_PlayerIds)
            {
                var TankState = GetTankLifeState(id);
                if (TankState != null) TankState -= OnTankLifeStateChange;
            }

            NetworkPortal.m_NetworkPortal.ChangeOfSceneClient -= OnChangeOfSceneClientChangeScene;
        }

        private NetworkVariable<TankLifeState>.OnValueChangedDelegate GetTankLifeState(ulong id)
        {
            if (!NetworkSpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject obj) || obj == null) return null;
            var NetworkState = obj.GetComponent<NetworkTankState>();
            return NetworkState != null ? NetworkState.NetworkTankLifeState.TankLifeState.OnValueChanged : null;
        }

        private void OnTankLifeStateChange(TankLifeState previousState, TankLifeState currentState)
        {
            switch (currentState)
            {
                case TankLifeState.Dead:
                  StartCoroutine(GameOver(m_GameOverDelay, false));
                    break;
                case TankLifeState.Alive:
                    StartCoroutine(GameOver(m_GameOverDelay, true));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }
        }

        private IEnumerator GameOver(float wait, bool tankoutcome)
        {
            yield return new WaitForSeconds(wait);
            GameStateRelayObj.RelayObjectSet = tankoutcome;
            MLAPI.SceneManagement.NetworkSceneManager.SwitchScene("GameOver");
        }
    }
}