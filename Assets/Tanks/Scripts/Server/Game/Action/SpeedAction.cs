using Tanks.Shared;
using UnityEngine;
namespace Tanks.Server
{
    public class SpeedAction:Action
    {
        public enum MoveStageState
        {
            Start,
            Moving,
            Finished
        }

        private ServerTankMovement m_Move;
        private Rigidbody m_Rigidbody;
        public SpeedAction(ServerTank parent, ref ActionData data) : base(parent, ref data)
        {
        }

        public override bool Start()
        {
            m_Move = m_parent.GetComponent<ServerTankMovement>();
            var state = m_RequestData.WhichAction == ActionOption.DriverForwardAction
                ? MoveState.Forward
                : MoveState.Idle;
            m_Move = m_parent.GetComponent<ServerTankMovement>();
            m_Move.SetMoveValues(ActionInfo.Duration,state);
            foreach (var player in m_parent.NetworkTankState._PlayerInTeam)
                if (player._NetworkRole.PRole == DifferentRoles.Driver)
                {
                    m_parent.NetworkTankState.PlayerInfoByRole[DifferentRoles.Driver].EnergyPoints -= 10;
                    break;
                }
            return true;
        }

        private MoveStageState GetState()
        {
            float timeDone = Time.time - StartTime;

            if (timeDone < ActionInfo.Duration)
                return MoveStageState.Moving;
            return MoveStageState.Finished;
        }
        private void Speedup()
        {
            //m_parent.transform.Translate(Vector3.forward * (speed * Time.deltaTime));
            m_parent.NetworkTankState.MoveStatus.Value = TankMoveStatus.Forward;
        }
        
        public void Stop()
        {
            //m_parent.transform.Translate(Vector3.forward * (0 * Time.deltaTime));
            m_parent.NetworkTankState.MoveStatus.Value = TankMoveStatus.Idle;
        }
        
        
        public override bool Update()
        {
            // var newState = GetState();
            // if (state != newState && newState == MoveStageState.Moving || Vector3.Distance(m_parent.transform.position,  NewPos)<0.1f)
            // {
            //     m_Rigidbody.AddRelativeForce(Vector3.forward * 0.5f, ForceMode.Acceleration);
            // }
            return true;
        }
    }
}