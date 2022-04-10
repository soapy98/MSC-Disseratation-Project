using MLAPI;
using Tanks.Shared;
using UnityEngine;

//ServerCharacter
namespace Tanks.Server
{
    public class ServerPlayer : NetworkBehaviour
    {
        public NetworkPlayerState NetPlayerState { get; private set; }

        private bool m_TankDead = false;

        private void Awake()
        {
            NetPlayerState = GetComponent<NetworkPlayerState>();
        }

        public override void NetworkStart()
        {
            if (IsServer) return;
            enabled = false;
            NetPlayerState.SetEnergy();
            NetPlayerState.NetworkEnergyState.HealthPoints.OnValueChanged += OnEnergyChange;
        }

        private void OnEnergyChange(int oldState, int newState)
        {
            if (NetPlayerState.Role == DifferentRoles.Engineer) return;
            NetPlayerState.RoleEnergyState = NetPlayerState.NetworkEnergyState.HealthPoints.Value < 10 ? EnergyState.NoEnergy : EnergyState.HasEnergy;
        }
        
     

       
    }
}