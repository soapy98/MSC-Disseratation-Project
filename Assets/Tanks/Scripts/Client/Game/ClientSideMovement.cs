using UnityEngine;
using MLAPI;
using Tanks.Shared;

namespace Tanks.Client
{
    //Handles updating the client side for objects on the server
    public class ClientSideMovement : NetworkBehaviour
    {
        private INetworkMovement m_MoveSource;
        private Rigidbody m_RigidBody;
        private bool m_Intialized;
        private void Start()
        {
            m_MoveSource = GetComponent<INetworkMovement>();
        }
        public override void NetworkStart()
        {
            if (IsServer) enabled = false;
            m_Intialized = true;
        }
        private void Update()
        {
            if (!m_Intialized) return;

            transform.position = m_MoveSource.ObjectNetworkPosition.Value;
            transform.rotation = Quaternion.Euler(0, m_MoveSource.ObjectNetworkRotationY.Value, 0);
            if (!m_RigidBody) return;
            var transform1 = transform;
            m_RigidBody.position = transform1.position;
            m_RigidBody.rotation = transform1.rotation;
        }
    }
}