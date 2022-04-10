using System;
using System.Collections.Generic;
using System.Linq;
using MLAPI.Spawning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks.Shared
{
    public class PartyHUD : MonoBehaviour
    {
        [SerializeField] private Image m_PlayerPortrait;
        [SerializeField] private GameObject[] m_TeamMemebersPanels;
        [SerializeField] private TextMeshProUGUI[] m_TeamPartynames;
        [SerializeField] private Image[] m_TeamPartySymbols;
        [SerializeField] private Slider[] m_TeamPartyEnergyBars;
        [SerializeField] private Sprite[] m_TeamRoleSymbols;
        [SerializeField] private ulong[] m_TeamsIds;
        [SerializeField] private Slider m_TeamTankHealth;
        [SerializeField] public DifferentRoles m_localRole;

        private ulong m_CurrentTarget;

        private Dictionary<ulong, NetworkPlayerState> m_TrackedHeroes = new Dictionary<ulong, NetworkPlayerState>();

        private int SetRoleSlot(DifferentRoles role)
        {
            return m_localRole switch
            {
                DifferentRoles.Driver => (role switch
                {
                    DifferentRoles.Gunner => 1,
                    DifferentRoles.Engineer => 2,
                    _ => 3
                }),
                DifferentRoles.Gunner => (role switch
                {
                    DifferentRoles.Driver => 1,
                    DifferentRoles.Engineer => 2,
                    _ => 3
                }),
                DifferentRoles.Engineer => (role switch
                {
                    DifferentRoles.Gunner => 1,
                    DifferentRoles.Driver => 2,
                    _ => 3
                }),
                DifferentRoles.None => -1,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        
        public void SetPlayerData(NetworkPlayerState playerState,DifferentRoles local)
        {
            m_localRole = local;
            InitiateTeamMemberArray();
            m_TeamsIds[0] = playerState.NetworkObject.NetworkObjectId;
            SetAllyDataSlot(SetRoleSlot(playerState.Role), playerState);
            var appearance = playerState.RoleAppearance.Value;
            if (appearance < m_TeamRoleSymbols.Length)
                m_PlayerPortrait.sprite = m_TeamRoleSymbols[appearance];
            playerState.TargetId.OnValueChanged += OnRoleSelectChanged;
            m_TrackedHeroes.Add(playerState.NetworkObjectId, playerState);
        }
        public void SetRoleEnergy(int amount)
        {
            m_TeamPartyEnergyBars[0].value = amount;
        }
        private static int GetMaxRoleEnergy(DifferentRoles role)
        {
            return role switch
            {
                DifferentRoles.Driver => 100,
                DifferentRoles.Gunner => 200,
                DifferentRoles.Engineer => 0,
                _ => 100
            };
        }
        private static string GetPlayerName(ulong id)
        {
            NetworkSpawnManager.SpawnedObjects[id].TryGetComponent(out NetworkPlayerState netState);
            return netState.Name;
        }

        public void SetTeamMemberData(NetworkPlayerState state)
        {
            var id = state.NetworkObjectId;
            var slot = AddAllyCheck(id);
            // do nothing if not in a slot
            if (slot == -1) return;
            SetAllyDataSlot(slot, state);
        }

        private int GetRoleID(DifferentRoles role)
        {
            return role switch
            {
                DifferentRoles.Driver => 0,
                DifferentRoles.Gunner => 1,
                DifferentRoles.Engineer => 2,
                _ => -1
            };
        }

        private void SetAllyDataSlot(int slot, NetworkPlayerState state)
        {
            m_TeamPartyEnergyBars[slot].maxValue = GetMaxRoleEnergy(state.Role);
            m_TeamPartyEnergyBars[slot].value = state.EnergyPoints;
            m_TeamPartynames[slot].text = GetPlayerName(m_TeamsIds[slot]);
            var symbol = GetRoleID(state.Role);
            if (symbol > m_TeamRoleSymbols.Length) return;
            m_TeamPartySymbols[slot].sprite = m_TeamRoleSymbols[symbol];
        }

        public void SetMemberEnergy(ulong id, int amount)
        {
            var slot = AddAllyCheck(id);
            if (slot == -1) return;
            m_TeamPartyEnergyBars[slot].value = amount;
        }

        public bool IsTeamMate(ulong id)
        {
            return m_TrackedHeroes.ContainsKey(id);
        }
        private void OnRoleSelectChanged(ulong previous, ulong newtarget)
        {
            SetPlayerFX(m_CurrentTarget, false);
            SetPlayerFX(newtarget, true);
        }

        private void SetPlayerFX(ulong target, bool select)
        {
            var slot = AddAllyCheck(target, true);
            if (slot < 0) return;
            m_TeamPartynames[slot].color = select ? Color.green : Color.white;
            m_CurrentTarget = select ? target : 0;
        }
       
        public void SetPlayerTankHealth(ulong tank)
        {
            var teamTank = NetworkSpawnManager.SpawnedObjects[tank];
            m_TeamTankHealth.value = teamTank.GetComponent<NetworkTankState>().Health.HealthPoints.Value;
        }

        private void InitiateTeamMemberArray()
        {
            if (m_TeamsIds != null) return;
            m_TeamsIds = new ulong[m_TeamPartyEnergyBars.Length];

            for (var i = 0; i < m_TeamPartyEnergyBars.Length; i++)
            {
                m_TeamsIds[i] = 0;
                m_TeamPartyEnergyBars[i].maxValue = 100;
            }
        }

        private int AddAllyCheck(ulong id, bool canAdd = false)
        {
            InitiateTeamMemberArray();
            var open = -1;
            for (var i = 0; i < m_TeamsIds.Length; i++)
            {
                if (m_TeamsIds[i] == id)
                    return i;
                if (open == -1 && i > 0 && m_TeamsIds[i] == 0)
                    open = i;
            }

            if (!canAdd) return -1;
            if (open <= 0) return -1;
            m_TeamMemebersPanels[open - 1].SetActive(true);
            m_TeamsIds[open] = id;
            return open;
        }
        public void RemoveTeamMember(ulong id)
        {
            for (var i = 0; i < m_TeamsIds.Length; i++)
            {
                if (m_TeamsIds[i] != id) continue;
                m_TeamMemebersPanels[i - 1].SetActive(false);
                m_TeamsIds[i] = 0;
                return;
            }
            if (!m_TrackedHeroes.TryGetValue(id, out var state)) return;
            if (state) state.TargetId.OnValueChanged -= OnRoleSelectChanged;
        }

        private void OnDestroy()
        {
            foreach (var id in m_TrackedHeroes.Where(id => id.Value))
            {
                id.Value.TargetId.OnValueChanged -= OnRoleSelectChanged;
            }
        }
    }
}