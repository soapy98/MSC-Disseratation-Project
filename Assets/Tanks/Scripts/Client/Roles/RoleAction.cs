using System.Collections.Generic;
using MLAPI;
using UnityEngine;
using Tanks.Shared;
using UnityEngine.UI;

namespace Tanks.Client
{
    public class RoleAction : NetworkBehaviour
    {
        public ActionSender m_PlayerInput;

        public override void NetworkStart()
        {
            if (!IsClient)
                enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.name.Contains("Hand")) return;
            if (m_PlayerInput.m_Parent.TeamObjs.SinglePlayer)
            {
                if (gameObject.name.Contains("Right"))
                {
                    m_PlayerInput.ActionRequest(ActionOption.DriverRightAction);
                }
                else if (gameObject.name.Contains("Left"))
                {
                    m_PlayerInput.ActionRequest(ActionOption.DriverLeftAction);
                }
                else if (gameObject.name.Contains("Go"))
                {
                    m_PlayerInput.ActionRequest(ActionOption.DriverForwardAction);
                }
                else if (gameObject.name.Contains("Stop"))
                {
                    m_PlayerInput.ActionRequest(ActionOption.DriverStopAction);
                }
                else if (gameObject.name.Contains("Gunner"))
                {
                    m_PlayerInput.ActionRequest(ActionOption.GunnerAction);
                }
            }

            if (m_PlayerInput.m_Parent.Role == DifferentRoles.Driver)
            {
                if (gameObject.name.Contains("Right"))
                {
                    m_PlayerInput.ActionRequest(ActionOption.DriverRightAction);
                }
                else if (gameObject.name.Contains("Left"))
                {
                    m_PlayerInput.ActionRequest(ActionOption.DriverLeftAction);
                }
                else if (gameObject.name.Contains("Go"))
                {
                    m_PlayerInput.ActionRequest(ActionOption.DriverLeftAction);
                }
                else if (gameObject.name.Contains("Stop"))
                {
                    m_PlayerInput.ActionRequest(ActionOption.DriverStopAction);
                }
            }
            else if (m_PlayerInput.m_Parent.Role == DifferentRoles.Gunner)
            {
                if (gameObject.name.Contains("Gunner"))
                {
                    m_PlayerInput.ActionRequest(ActionOption.GunnerAction);
                }
            }
            else if (m_PlayerInput.m_Parent.Role == DifferentRoles.Engineer)
            {
            }
        }
    }
}