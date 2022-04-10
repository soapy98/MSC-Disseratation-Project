using System.Collections.Generic;
using MLAPI.NetworkVariable.Collections;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using MLAPI;
using UnityEngine.UI;

namespace Tanks.Client
{
    [RequireComponent(typeof(RoleTeamSelectData))]
    public class ClientRoleTeamState : SceneStateBehaviour
    {
        public static ClientRoleTeamState Singleton { get; private set; }
        protected override GameState ActiveState => GameState.RoleTeamSelect;
        private RoleTeamSelectData TeamRoleSelectData { get; set; }

        public Text m_NumberPlayersInGame;
        [SerializeField] private int m_LastRoleSelect = -1;
        [SerializeField] private int m_LastTeamSelect = -1;
        private bool m_LocalPlayerLockedIn = false;
        [SerializeField] private List<UITeamRoleSelectSeat> m_TeamRoleSeats;
        [SerializeField] private Button m_ReadyButton;

        [System.Serializable]
        public class RoleTeamColorSprite
        {
            public Sprite RoleSprite;
            public Color Color;
        }
        private Text m_ErrorMessage;
        public Text ErrorMessage
        {
            get => m_ErrorMessage;
            set => m_ErrorMessage = value;
        }

        public RoleTeamColorSprite[] m_TeamRoleIdentifier;
        private enum TeamSelectMode
        {
            NoTeamChoosen,
            TeamChoosen,
            LobbyEnd,
            Error
        }
        private void Awake()
        {
            Singleton = this;
            TeamRoleSelectData = GetComponent<RoleTeamSelectData>();
        }
        protected override void Start()
        {
            base.Start();
            m_ReadyButton.onClick.AddListener(OnPlayerClickedReady);

            for (int i = 0; i < m_TeamRoleSeats.Count; i++)
            {
                m_TeamRoleSeats[i].Initialize(i, m_ReadyButton);
            }

            ConfigureUIForLobbyMode(TeamSelectMode.TeamChoosen);
            UpdateTeamSelection(RoleTeamSelectData.RoleSeatState.Inactive);
        }
        public override void NetworkStart()
        {
            base.NetworkStart();
            if (!IsClient)
            {
                enabled = false;
            }
            else
            {
                TeamRoleSelectData.LobbyClosed.OnValueChanged += OnLobbyClosedChanged;
                TeamRoleSelectData.OnFatalLobbyError += OnFatalLobbyError;
                TeamRoleSelectData.OnRoleTeamAssigned += OnAssignedPlayerNumber;
                TeamRoleSelectData.RolePlayers.OnListChanged += OnLobbyPlayerStateChanged;
            }
        }
        protected override void OnDestroy() 
        {
            base.OnDestroy();
            if (TeamRoleSelectData)
            {
                TeamRoleSelectData.LobbyClosed.OnValueChanged -= OnLobbyClosedChanged;
                TeamRoleSelectData.OnFatalLobbyError -= OnFatalLobbyError;
                TeamRoleSelectData.OnRoleTeamAssigned -= OnAssignedPlayerNumber;
                TeamRoleSelectData.RolePlayers.OnListChanged -= OnLobbyPlayerStateChanged;
            }

            if (Singleton == this)
                Singleton = null;
        }
        /// When player number set
        private void OnAssignedPlayerNumber(int num)
        {
             GameManager._GameManager.Number = num;
        }
        private void UpdatePlayerCount()
        {
            int count = TeamRoleSelectData.RolePlayers.Count;
            var pstr = (count > 1) ? "players" : "player";
            m_NumberPlayersInGame.text = "<b>" + count + "</b> " + pstr + " connected";
        }
        private void OnLobbyPlayerStateChanged(NetworkListEvent<RoleTeamSelectData.RolePlayerState> lobbyArray)
        {
            UpdateTeamSeats();
            UpdatePlayerCount();
            int localPlayerIdx = -1;
            for (int i = 0; i < TeamRoleSelectData.RolePlayers.Count; ++i)
            {
                if (TeamRoleSelectData.RolePlayers[i].playerID != NetworkManager.Singleton.LocalClientId) continue;
                localPlayerIdx = i;
                break;
            }

            if (localPlayerIdx == -1)
            {
                // player not active in lobby
                UpdateTeamSelection(RoleTeamSelectData.RoleSeatState.Inactive);
            }
            else if (TeamRoleSelectData.RolePlayers[localPlayerIdx].SeatState ==
                     RoleTeamSelectData.RoleSeatState.Inactive)
            {
                // player either hasn't choosen role or someone took there role
                UpdateTeamSelection(RoleTeamSelectData.RoleSeatState.Inactive);
                // player number set in UI
                OnAssignedPlayerNumber(TeamRoleSelectData.RolePlayers[localPlayerIdx].PlayerNum);
            }
            else
            {
                // role locked in
                UpdateTeamSelection(TeamRoleSelectData.RolePlayers[localPlayerIdx].SeatState,
                    TeamRoleSelectData.RolePlayers[localPlayerIdx].SeatIdx);
            }
        }
        int TeamSelection(int idx)
        {
            return idx switch
            {
                0 => 1,
                1 => 1,
                2 => 1,
                3 => 2,
                4 => 2,
                5 => 2,
                _ => -1
            };
        }
        private void UpdateTeamSelection(RoleTeamSelectData.RoleSeatState state, int seatIdx = -1, int team = -1)
        {
            bool isNewSeat = m_LastRoleSelect != seatIdx;
            m_LastTeamSelect = TeamSelection(seatIdx);
            m_LastRoleSelect = seatIdx;
            switch (state)
            {
                case RoleTeamSelectData.RoleSeatState.Inactive:
                   
                    break;
                case RoleTeamSelectData.RoleSeatState.LockedIn when !m_LocalPlayerLockedIn:
                    // local player role state is locked in, causes changes in UI
                    ConfigureUIForLobbyMode(TeamRoleSelectData.LobbyClosed.Value? TeamSelectMode.LobbyEnd: TeamSelectMode.TeamChoosen);
                    m_LocalPlayerLockedIn = true;
                    break;
                default:
                {
                    if (m_LocalPlayerLockedIn && state == RoleTeamSelectData.RoleSeatState.Active)
                    {
                        // UI info reset if role seat changed
                        if (!m_LocalPlayerLockedIn) return;
                        ConfigureUIForLobbyMode(TeamSelectMode.TeamChoosen);
                        m_LocalPlayerLockedIn = false;
                    }
                    break;
                }
            }
        }
        private void UpdateTeamSeats()
        {
            var seats = new RoleTeamSelectData.RolePlayerState[m_TeamRoleSeats.Count];
            foreach (var playerState in TeamRoleSelectData.RolePlayers)
            {
                if (playerState.SeatIdx == -1 || playerState.SeatState == RoleTeamSelectData.RoleSeatState.Inactive)
                    continue; // player hasn't choosen role
                if (seats[playerState.SeatIdx].SeatState == RoleTeamSelectData.RoleSeatState.Inactive
                    || (seats[playerState.SeatIdx].SeatState == RoleTeamSelectData.RoleSeatState.Active &&
                        seats[playerState.SeatIdx].LastChangeTime < playerState.LastChangeTime))
                {
                    // player is last person to choose role
                    seats[playerState.SeatIdx] = playerState;
                }
            }

            for (var i = 0; i < m_TeamRoleSeats.Count; ++i)
            {
                m_TeamRoleSeats[i].SetTeamState(seats[i].SeatState, seats[i].PlayerNum, seats[i].PlayerName);
            }
        }
        private void OnLobbyClosedChanged(bool wasLobbyClosed, bool isLobbyClosed)
        {
            if (isLobbyClosed) ConfigureUIForLobbyMode(TeamSelectMode.LobbyEnd);
        }
        private void OnFatalLobbyError(RoleTeamSelectData.FatalLobby error)
        {
            ErrorMessage.text = error switch
            {
                RoleTeamSelectData.FatalLobby.LobbyFull => "Unable to join lobby is full",
                _ => throw new System.Exception($"Unknown fatal lobby error {error}")
            };

            ConfigureUIForLobbyMode(TeamSelectMode.Error);
        }
        private void ConfigureUIForLobbyMode(TeamSelectMode mode)
        {
            foreach (var seat in m_TeamRoleSeats)
            {
                seat.DisableInteraction(seat.LockedIn());
            }
        }
        public void OnPlayerClickedSeatWithTeam(int seatIdx, int team)
        {
            TeamRoleSelectData.ChangeRoleWithTeamServerRpc(NetworkManager.Singleton.LocalClientId, seatIdx, team,
                false);
        }
        private void OnPlayerClickedReady()
        {
            TeamRoleSelectData.ChangeRoleWithTeamServerRpc(NetworkManager.Singleton.LocalClientId, m_LastRoleSelect,
                m_LastTeamSelect,
                !m_LocalPlayerLockedIn);
        }
        public void OnPlayerExit()
        {
            var gameNetPortal = GameManager._GameManager.NetPortal.GetComponent<NetworkPortal>();
            gameNetPortal.RequestDisconnect();
            SceneManager.LoadScene("MainMenu");
        }
    }
}