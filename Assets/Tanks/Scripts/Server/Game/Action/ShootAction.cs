using MLAPI;
using MLAPI.MonoBehaviours.Core;
using Tanks.Shared;
using UnityEngine;

namespace Tanks.Server
{
    public sealed class ShootAction : Action
    {
        private bool m_BulletShot = false;

        public ShootAction(ServerTank parent, ref ActionData data) : base(parent, ref data)
        {
        }

        public override bool Start()
        {
            m_parent.transform.forward = Data.Direction;
            foreach (var player in m_parent.NetworkTankState._PlayerInTeam)
            {
                if (player._NetworkRole.PRole != DifferentRoles.Driver) continue;
                m_parent.NetworkTankState.PlayerInfoByRole[DifferentRoles.Gunner].EnergyPoints -= 10;
                break;
            }
            return true;
        }

        public override bool Update()
        {
            if (RunTime >= ActionInfo.ExecutionTime && !m_BulletShot)
                ShootBullet();
            return true;
        }

        private ActionInfo.BulletInfo GetInfo()
        {
            var size = ActionInfo.Bullets.Length;
            var num = Random.Range(0, size);
            var bullet = ActionInfo.Bullets[num];
            if (bullet.BulletObj && bullet.BulletObj.GetComponent<NetworkBulletState>())
                return bullet;
            throw new System.Exception($"Action {ActionInfo.ActionOption} no bullet prefabs for use!");
        }

        private void ShootBullet()
        {
            if (m_BulletShot) return;
            m_BulletShot = true;
            var info = GetInfo();
            var bullet = Object.Instantiate(info.BulletObj);
            bullet.transform.forward = m_parent.transform.forward;
            bullet.transform.position = m_parent.transform.localToWorldMatrix.MultiplyPoint(bullet.transform.position);
            bullet.GetComponent<NetworkBulletState>().BulletNetworkTeam.Value = m_RequestData.Team;
            bullet.GetComponent<ServerBulletState>().Init(m_parent.NetworkObjectId, in info);
            bullet.GetComponent<NetworkObject>().Spawn();
        }

        public override void End()
        {
            ShootBullet();
        }
    }
}