using System;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using TMPro;
using UnityEngine;

namespace Tanks.Shared
{
    public enum TankLifeState
    {
        Alive,
        Dead
    }

    [Serializable]
    public enum TankMoveStatus
    {
        Idle,
        Forward,
        Left,
        Right,
        Back
    }

    public class NetworkTankState : NetworkBehaviour, INetworkMovement
    {
        public void InitObjectNetworkPositionAndYRotation(Vector3 startPos, float rotY)
        {
            ObjectNetworkPosition.Value = startPos;
            ObjectNetworkRotationY.Value = rotY;
        }

        public TextMeshProUGUI m_TankNameUI;

        public NetworkVariableVector3 ObjectNetworkPosition { get; } = new NetworkVariableVector3(
            new NetworkVariableSettings() {SendNetworkChannel = MLAPI.Transports.NetworkChannel.PositionUpdate});

        public NetworkVariableFloat ObjectNetworkRotationY { get; } = new NetworkVariableFloat(
            new NetworkVariableSettings() {SendNetworkChannel = MLAPI.Transports.NetworkChannel.PositionUpdate});

        public NetworkVariableFloat ObjectMovementSpeed { get; } = new NetworkVariableFloat();


        public NetworkVariable<TankMoveStatus> MoveStatus { get; } = new NetworkVariable<TankMoveStatus>();
        [SerializeField] private NetworkEnergyHealthState NetworkHealth;

        public NetworkEnergyHealthState Health => NetworkHealth;

        public int HealthBarValue
        {
            get => NetworkHealth.HealthPoints.Value;
            set => NetworkHealth.HealthPoints.Value = value;
        }

        // private NetworkVariable<EnergyHealthBar> NetworkHealth { get; } = new NetworkVariable<EnergyHealthBar>();
        private NetworkVariableInt TankTeam { get; } = new NetworkVariableInt();
        [SerializeField] private NetworkTankLifeState m_NetworkLifeState;
        public NetworkTankLifeState NetworkTankLifeState => m_NetworkLifeState;

        public TankLifeState LifeState
        {
            get => m_NetworkLifeState.TankLifeState.Value;
            set => m_NetworkLifeState.TankLifeState.Value = value;
        }

        public int Team
        {
            get => TankTeam.Value;
            set => TankTeam.Value = value;
        }

        public bool IsAlive => LifeState != TankLifeState.Dead;
        public bool CanPerformActions => LifeState == TankLifeState.Alive;

        private List<NetworkPlayerState> _players = new List<NetworkPlayerState>();

        public List<NetworkPlayerState> _PlayerInTeam => _players;

        private Dictionary<DifferentRoles, NetworkPlayerState> m_TeamPlayerRoleMap =
            new Dictionary<DifferentRoles, NetworkPlayerState>();

        public Dictionary<DifferentRoles, NetworkPlayerState> PlayerInfoByRole => m_TeamPlayerRoleMap;
        
        private void AddPlayerToDictionary(NetworkPlayerState state)
        {
            foreach (var player in _players)
            {
                if (m_TeamPlayerRoleMap.ContainsKey(state.Role))
                    throw new System.Exception($"Duplicate role definition detected: {state.Role}");
                if (player != state) continue;
                m_TeamPlayerRoleMap[player.Role] = player;
                return;
            }
        }
        
        public void SetPlayerTeamMatesInfo()
        {
            foreach (var player in _players)
            {
                foreach (var TeamMate in _players.TakeWhile(TeamMate => player != TeamMate))
                {
                    player.HUD.GetComponentInChildren<PartyHUD>().SetPlayerData(TeamMate, player.Role);
                    player.HUD.GetComponentInChildren<PartyHUD>().SetPlayerTankHealth(NetworkObject.NetworkInstanceId);
                }
            }
        }
        public NetworkVariableInt TeamAmount;
        public void AddPlayer(NetworkPlayerState p)
        {
            TeamAmount.Value++;
            p.PlayerTeamTank = GetComponent<NetworkObject>();
            p.PlayerTeamTankid = GetComponent<NetworkObject>().NetworkObjectId;
            _players.Add(p);
            AddPlayerToDictionary(p);
        }
        public event Action<ActionData> TankActionEventServer;
        [ServerRpc]
        public void TankActionServerRpc(ActionData data)
        {
            TankActionEventServer?.Invoke(data);
        }
    }
}