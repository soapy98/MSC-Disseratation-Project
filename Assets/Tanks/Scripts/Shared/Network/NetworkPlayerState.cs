using System;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

//NetworkCharacterState
namespace Tanks.Shared
{
    public enum EnergyState
    {
        HasEnergy,
        NoEnergy
    }

    public struct PlayerInfo
    {
        public PlayerName _NetworkName;
        public NetworkVariableInt PlayerNumber;
        public NetworkVariableInt _NetworkTeam;
        public PlayerRole _NetworkRole;
    };

    [RequireComponent(typeof(NetworkRoleTypeState), typeof(NetworkName))]
    public class NetworkPlayerState : NetworkBehaviour
    {
        [SerializeField] private NetworkObject m_PlayerTeamTank;
        [SerializeField] private GameObject player;
        public GameObject HUD;
        public ulong PlayerTeamTankid { get; set; }

        public NetworkObject PlayerTeamTank
        {
            get => m_PlayerTeamTank;
            set => m_PlayerTeamTank = value;
        }

        public PlayerName _NetworkName;
        public NetworkVariableInt PlayerNumber;
        public NetworkVariableInt _NetworkTeam;

        public PlayerRole _NetworkRole;

        //NetworkTankState
        private NetworkTankState m_PlayerTeamTankState;

        public NetworkTankState TeamTankState
        {
            get => m_PlayerTeamTankState;
             set => m_PlayerTeamTankState = value;
        }
        //Tank life state
        NetworkTankLifeState m_TeamTankLifeState;
        public NetworkTankLifeState NetworkTankLifeState => TeamTankState.NetworkTankLifeState;
        //Tank health state
        [SerializeField] private NetworkEnergyHealthState m_TankNetworkHealthState;
        public NetworkEnergyHealthState TankHealthState
        {
            get => m_TankNetworkHealthState;
            set => m_TankNetworkHealthState = value;
        }

        //PlayerInfo
        public string Name
        {
            get =>_NetworkName.PName;
            set => _NetworkName.PName = value;
        }
        public DifferentRoles Role
        {
            get => _NetworkRole.PRole;
            set => _NetworkRole.PRole = value;
        }
        public int Team
        {
            get => _NetworkTeam.Value;
            set =>_NetworkTeam.Value = value;
        }
        
        
        //Player energy info and state info
        public bool CanPerformActions => RoleEnergyState == Shared.EnergyState.HasEnergy;
        [SerializeField] private NetworkRoleEnergyState m_RoleEnergyState;
        public NetworkRoleEnergyState NetworkRoleEnergyState => m_RoleEnergyState;
        public EnergyState RoleEnergyState
        {
            get => NetworkRoleEnergyState.EnergyState.Value;
            set => NetworkRoleEnergyState.EnergyState.Value = value;
        }
        [SerializeField] private NetworkEnergyHealthState EnergyState;
        public NetworkEnergyHealthState NetworkEnergyState => EnergyState;

        public int EnergyPoints
        {
            set => NetworkEnergyState.HealthPoints.Value = value;
            get => NetworkEnergyState.HealthPoints.Value;
        }
        public GameObject RoleObjects;
        public TeamObjectSet TeamObjs;
        // public Role CurrentRole => GetComponent<Role>();

        private GameManager _GameManager;
        public NetworkVariableInt RoleAppearance;
        public NetworkVariableBool RoleSet { get; } = new NetworkVariableBool();
        public NetworkVariableBool WinningTeam { get; } = new NetworkVariableBool();

        public void SetPlayerUp(DifferentRoles role, int roleSprite, TeamObjectSet obj,int team)
        {
            HUD.SetActive(true);
            // Role = role;
            Team = team;
            RoleAppearance.Value = roleSprite;
           
            switch (role)
            {
                case DifferentRoles.Driver:
                    RoleObjects = obj.DriverObjects;
                    break;
                case DifferentRoles.Engineer:
                    RoleObjects = obj.EngineerObjects;
                    break;
                case DifferentRoles.Gunner:
                    RoleObjects = obj.GunnerObjects;
                    break;
                case DifferentRoles.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }

        public void SetEnergy()
        {
            EnergyPoints = 100;
        }
        
        
        public NetworkVariableULong TargetId { get; } = new NetworkVariableULong();

        private void Start()
        {
            player = gameObject;

            // m_PlayerData = new PlayerInfo();

            GameManager._GameManager.m_PlayerData._NetworkName = GetComponent<PlayerName>() != null
                ? GetComponent<PlayerName>()
                : gameObject.AddComponent<PlayerName>();
            GameManager._GameManager.m_PlayerData._NetworkRole = GetComponent<PlayerRole>() != null
                ? GetComponent<PlayerRole>()
                : gameObject.AddComponent<PlayerRole>();
            GameManager._GameManager.m_PlayerData._NetworkTeam = new NetworkVariableInt();
            DontDestroyOnLoad(gameObject);
        }
    }
}