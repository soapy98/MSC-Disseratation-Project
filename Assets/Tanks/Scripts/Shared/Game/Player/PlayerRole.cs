using System;
using UnityEngine;

namespace Tanks.Shared
{
    public class PlayerRole : MonoBehaviour
    {
       [SerializeField] private NetworkRoleTypeState m_Role;

        public DifferentRoles PRole
        {
            get => m_Role.RoleType.Value;
            set => m_Role.RoleType.Value = value;
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            // m_Role = gameObject.GetComponent<NetworkRoleTypeState>() == null
            //     ? m_Role = gameObject.AddComponent<NetworkRoleTypeState>()
            //     : m_Role = gameObject.GetComponent<NetworkRoleTypeState>();

            PRole = DifferentRoles.None;
        }
    }
}