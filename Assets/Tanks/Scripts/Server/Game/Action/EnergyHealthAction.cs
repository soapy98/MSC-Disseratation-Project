using MLAPI.Spawning;
using Tanks.Shared;

namespace Tanks.Server
{
    public class EnergyHealthAction : Action
    {
        private ServerPlayer m_RoleTarget;

        public EnergyHealthAction(ServerTank parent, ref ActionData data) : base(parent,
            ref data)
        {
        }

        public override bool Start()
        {
            if (m_RequestData.IdsOfTargets == null || m_RequestData.IdsOfTargets.Length == 0 ||
                !NetworkSpawnManager.SpawnedObjects.ContainsKey(m_RequestData.IdsOfTargets[0]))
                return false;
            var target = NetworkSpawnManager.SpawnedObjects[m_RequestData.IdsOfTargets[0]];
            m_RoleTarget = target.GetComponent<ServerPlayer>();
            ReplenishEnergy(m_RoleTarget.NetPlayerState.Role);
            return true;
        }

        public override bool Update()
        {
            throw new System.NotImplementedException();
        }

        private void ReplenishEnergy(DifferentRoles role)
        {
            m_parent.NetworkTankState.PlayerInfoByRole[role].EnergyPoints = 100;
        }
    }
}