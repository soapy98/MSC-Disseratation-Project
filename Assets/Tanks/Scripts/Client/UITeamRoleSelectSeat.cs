using System;
using System.Collections.Generic;
using MLAPI.NetworkingManagerComponents.Core;
using Tanks.Client;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tanks.Shared
{
    public class UITeamRoleSelectSeat : MonoBehaviour
    {
        public int m_Team;
        public bool SlotFilled = false;
        private int m_SeatIdx;
        [SerializeField] private RoleTeamSelectData.RoleSeatState m_state;
        [SerializeField] private TextMeshProUGUI m_PlayerName;
        [SerializeField] private DifferentRoles m_PlayerRole;
        [SerializeField] private Button m_RoleButton;
        private bool m_Disable;
        private int m_PlayerNum;
        private Button m_ReadyButton;
        public void Initialize(int seatIdx,Button ready)
        {
            m_SeatIdx = seatIdx;
            m_state = RoleTeamSelectData.RoleSeatState.Inactive;
            m_PlayerNum = -1;
            AdaptTeamGraphics();
            m_ReadyButton = ready;
        }
        public void SetTeamState(RoleTeamSelectData.RoleSeatState state, int idx, string name)
        {
            if (state == m_state && idx == m_PlayerNum) return;
            m_state = state;
            m_PlayerNum = idx;
            m_PlayerName.text = name;
            
            if (m_state == RoleTeamSelectData.RoleSeatState.Inactive) m_PlayerNum = -1;
            AdaptTeamGraphics();
        }
        public bool LockedIn()
        {
            return m_state == RoleTeamSelectData.RoleSeatState.LockedIn;
        }

        public void DisableInteraction(bool active)
        {
            m_RoleButton.interactable = !active;
            m_Disable = active;
        }

        private void AdaptTeamGraphics()
        {
            if (m_state == RoleTeamSelectData.RoleSeatState.Inactive)
            {
                m_RoleButton.interactable = !m_Disable;
            }
            else
            {
                m_PlayerName.gameObject.SetActive(true);
            }
        }

        public void OnClicked()
        {
            ClientRoleTeamState.Singleton.OnPlayerClickedSeatWithTeam(m_SeatIdx,m_Team);
        }
    }
}