using System.Collections.Generic;
using MLAPI;
using UnityEngine;
using Tanks.Shared;
using UnityEngine.UI;

//Handles the setting up a local client and related functionality
namespace Tanks.Client
{
    public class ClientPlayer : NetworkBehaviour
    {
       [SerializeField] private NetworkPlayerState m_NetworkPlayer;
        private NetworkTankState m_NetTankState;
        private Role m_Role;
        [SerializeField] private ActionSender m_PlayerInput;
        public override void NetworkStart()
        {
            if (!IsClient&& !m_NetworkPlayer.IsLocalPlayer)
                enabled = false;
            if (m_NetworkPlayer.RoleSet.Value)
            {
                SetTankState();
            }
        }

        private void SetTankState()
        {
            m_NetTankState = m_NetworkPlayer.TeamTankState;
            m_NetTankState .NetworkTankLifeState.TankLifeState.OnValueChanged += ChangeGameOverState;
            if (m_NetworkPlayer.TeamObjs.SinglePlayer)
            {
                SetDriverObjs();
                SetGunnerObjs();
               // SetEngineerObjs();
            }
            else
                SetRoleWithObjects();
        }

        public void ChangeGameOverState(TankLifeState previousState, TankLifeState currentState)
        {
            if (currentState == TankLifeState.Dead)
                GameManager._GameManager.WinningTeam1 = false;
        }
        
        void SetEngineerObjs(Transform objs)
        {
            m_Role = gameObject.AddComponent<Engineer>();
            var engineObjs = objs.Find("Engineer").gameObject;
            GetComponent<Engineer>().EngineerObjs = objs.Find("Engineer").gameObject;
            GetComponent<Engineer>().SetInputSender(m_PlayerInput);
            // GetComponent<Engineer>().SetEnergyObjects(m_NetworkPlayer.TeamObjs.DriverBall,m_NetworkPlayer.TeamObjs.GunnerBall);

            var Gun = engineObjs.transform.Find("DriverRope");
            var Drive = engineObjs.transform.Find("GunnerRope");
            GetComponent<Engineer>().GunnerHealthObj = Gun.gameObject;
            GetComponent<Engineer>().DriverHealthObj = Drive.gameObject;
            GetComponent<Engineer>().LinkEnergyBarsWithObjects();
        }

        void SetDriverObjs()
        {
            m_Role = gameObject.AddComponent<Driver>();
            GetComponent<Driver>().SetInputSender(m_PlayerInput);
            GetComponent<Driver>().TurnButtons = m_NetworkPlayer.TeamObjs.DriverButtons;
            foreach (var button in GetComponent<Driver>().TurnButtons)
            {
                GetComponent<Driver>().SetButtonEvent(button);
                button.GetComponentInParent<RoleAction>().m_PlayerInput = m_PlayerInput;
            }
        }

        void SetGunnerObjs()
        {
            m_Role = gameObject.AddComponent<Gunner>();
            GetComponent<Gunner>().SetInputSender(m_PlayerInput,m_NetworkPlayer.TeamObjs.ShootButton);
            m_NetworkPlayer.TeamObjs.ShootButton.GetComponentInParent<RoleAction>().m_PlayerInput = m_PlayerInput;

        }

        private void SetRoleWithObjects()
        {
            var objs = m_NetworkPlayer.RoleObjects.transform;
            
            if (m_NetworkPlayer.Role == DifferentRoles.Gunner)
            {
                SetGunnerObjs();
            }
            else if (m_NetworkPlayer.Role == DifferentRoles.Driver)
            {
                SetDriverObjs();
            }
            else if (m_NetworkPlayer.Role == DifferentRoles.Engineer)
            {
                SetEngineerObjs(objs);
            }
            m_Role.SetParent();
        }
    }
}