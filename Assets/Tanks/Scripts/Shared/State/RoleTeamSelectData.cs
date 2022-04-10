using System;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Serialization;
using UnityEngine;

namespace Tanks.Shared
{
    public class RoleTeamSelectData : MonoBehaviour
    {
        public enum RoleSeatState
        {
            Inactive,
            Active,
            LockedIn
        }

        public enum FatalLobby
        {
            LobbyFull
        }

        [Serializable]
        public struct RoleSeatConfig
        {
            public DifferentRoles Role;
            public int roleSpriteidx;

            public RoleSeatConfig(DifferentRoles roleClass, int roleSpriteidx)
            {
                Role = roleClass;
                this.roleSpriteidx = roleSpriteidx;
            }
        }

        [SerializeField] public RoleSeatConfig[] RoleSeatConfigs = new RoleSeatConfig[]
        {
            new RoleSeatConfig(DifferentRoles.Driver, 1),
            new RoleSeatConfig(DifferentRoles.Gunner, 2),
            new RoleSeatConfig(DifferentRoles.Engineer, 3),
        };

        public const int m_MaxLobbyPlayers = 6;

        public struct RolePlayerState : INetworkSerializable
        {
            public ulong playerID;
            public string PlayerName;
            public int PlayerNum;
            public int SeatIdx;
            public int PlayerTeam;
            public RoleSeatState SeatState;
            public float LastChangeTime;

            public RolePlayerState(ulong id, string name, int playerNum,
                RoleSeatState seatState, int team = -1, int idx = -1, float time = 0)
            {
                playerID = id;
                PlayerName = name;
                PlayerNum = playerNum;
                SeatIdx = idx;
                PlayerTeam = team;
                SeatState = seatState;
                LastChangeTime = time;
            }

            public void NetworkSerialize(NetworkSerializer serializer)
            {
                serializer.Serialize(ref playerID);
                serializer.Serialize(ref PlayerName);
                serializer.Serialize(ref PlayerNum);
                serializer.Serialize(ref SeatIdx);
                serializer.Serialize(ref PlayerTeam);
                serializer.Serialize(ref SeatState);
                serializer.Serialize(ref LastChangeTime);
            }
        }
        private NetworkList<RolePlayerState> m_RolePlayers;
        private void Awake()
        {
            m_RolePlayers = new NetworkList<RolePlayerState>();
        }

        public NetworkList<RolePlayerState> RolePlayers => m_RolePlayers;

        public NetworkVariableBool LobbyClosed { get; } = new NetworkVariableBool();

        public event Action<int> OnRoleTeamAssigned;

        [ClientRpc]
        public void AssignPlayerNumClientRPC(int roleidx)
        {
            OnRoleTeamAssigned?.Invoke(roleidx);
        }

        public event Action<FatalLobby> OnFatalLobbyError;
       
        [ClientRpc]
        public void FatalLobbyErrorClientRpc(FatalLobby error, ClientRpcParams clientParams = default)
        {
            OnFatalLobbyError?.Invoke(error);
        }
        public event Action<ulong, int, int, bool> OnClientRoleWithTeamChange;

        [ServerRpc(RequireOwnership = false)]
        public void ChangeRoleWithTeamServerRpc(ulong id, int roleidx, int team, bool lockedIn)
        {
            OnClientRoleWithTeamChange?.Invoke(id, roleidx, team, lockedIn);
        }
    }
}