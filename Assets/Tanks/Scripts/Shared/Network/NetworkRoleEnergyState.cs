using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

namespace Tanks.Shared
{
    public class NetworkRoleEnergyState: NetworkBehaviour
    {
        [SerializeField]
        NetworkVariable<EnergyState>m_EnergyState = new NetworkVariable<EnergyState>(Shared.EnergyState.HasEnergy);
        public NetworkVariable<EnergyState> EnergyState => m_EnergyState;
    }
}