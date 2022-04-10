using System;
using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Spawning;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tanks.Client
{
//Is reponsible for sending the action packets to the server from a client
    public class ActionSender : NetworkBehaviour
    {
        private float m_LastAction;
        public NetworkPlayerState m_Parent;

        private struct RequestAction
        {
            public ActionOption WhichActionWanted;
        }

        private readonly RequestAction[] m_ActionsRequested = new RequestAction[5];
        private int m_ActionRequestAmount;

        private void Awake()
        {
            m_Parent = GetComponent<NetworkPlayerState>();
        }

        public override void NetworkStart()
        {
            if (!IsClient || !IsOwner || SceneManager.GetActiveScene().name != "Game")
            {
                enabled = false;
                return;
            }

        }
  

        private void SentAction(ActionData action)
        {
            m_Parent.TeamTankState.TankActionServerRpc(action);
        }

        private void FixedUpdate()
        {
            for (var i = 0; i < m_ActionRequestAmount; i++)
            {
                var data = GameData.Instance.ActionsByDataType[m_ActionsRequested[i].WhichActionWanted];
                var action = new ActionData();
                FillRequest(data.ActionOption, ref action);
                SentAction(action);
            }

            m_ActionRequestAmount = 0;
        }

        private void FillRequest(ActionOption action, ref ActionData data)
        {
            data.WhichAction = action;
            var info = GameData.Instance.ActionsByDataType[action];
            data.Close = true;
            data.Team = m_Parent.TeamTankState.Team;
            switch (info.ActionLogic)
            {
                case ActionLogic.SpeedUp:
                    data.Total = 10;
                    return;
                case ActionLogic.Enable:
                    data.Cancel = false;
                    return;
                case ActionLogic.Shoot:
                    var pos = (m_Parent.TeamTankState.ObjectNetworkPosition.Value - 2*m_Parent.TeamTankState.transform.forward);
                    pos.y+=0.5f;
                    data.Position = pos;
                    data.Direction = m_Parent.TeamTankState.gameObject.transform.forward;
                    return;
                case ActionLogic.TurnLeft:
                    data.Total = -data.Total;
                    return;
                case ActionLogic.Stop:
                    data.Total = 0;
                    return;
                case ActionLogic.Disable:
                    data.Cancel = true;
                    return;
                case ActionLogic.TurnRight:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ActionRequest(ActionOption action)
        {
            if (action == ActionOption.None || m_ActionRequestAmount >= m_ActionsRequested.Length) return;
            m_ActionsRequested[m_ActionRequestAmount].WhichActionWanted = action;
            m_ActionRequestAmount++;
        }
    }
}