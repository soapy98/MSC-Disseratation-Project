using System;
using MLAPI;
using Tanks.Shared;
using UnityEngine;

namespace Tanks.Client
{
    //Is responsible for updating each clients UI information for themselves and team mates
    public class ClientPlayerGameLogic : NetworkBehaviour
    {
        public NetworkPlayerState m_PlayerState;
        PartyHUD m_PartyHUD;
        public Transform ParentTransform { get; private set; }
        private Action Destroyed;
        public override void NetworkStart()
        {
            if (!IsClient || transform.parent == null)
            {
                enabled = false;
                return;
            }

            ParentTransform = transform.parent;
            m_PlayerState = ParentTransform.GetComponent<NetworkPlayerState>();
            transform.SetParent(gameObject.transform);
            m_PlayerState.NetworkEnergyState.HealthPoints.OnValueChanged += OnEnergyChanged;
            if (IsLocalPlayer)
            {
                m_PartyHUD.SetPlayerData(m_PlayerState,m_PlayerState.Role);
            }
            else
            {
                m_PartyHUD.SetTeamMemberData(m_PlayerState);
            }
        }

        private void OnEnergyChanged(int previousValue, int newValue)
        {
            if (m_PartyHUD == null) return;
            if (IsLocalPlayer) m_PartyHUD.SetRoleEnergy(newValue);
            else if (!IsLocalPlayer)
                if(m_PartyHUD.IsTeamMate(GetComponent<NetworkObject>().NetworkObjectId))
                    m_PartyHUD.SetMemberEnergy(m_PlayerState.NetworkObjectId, newValue);
        }
    }
}