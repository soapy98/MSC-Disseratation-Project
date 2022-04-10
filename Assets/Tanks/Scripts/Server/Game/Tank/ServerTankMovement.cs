using System;
using Tanks.Shared;
using UnityEngine;
using MLAPI;

namespace Tanks.Server
{
    public enum MoveState
    {
        Idle = 0,
        Forward = 1,
        Backward = 2,
        Right = 3,
        Left = 4,
    }

    public class ServerTankMovement : NetworkBehaviour
    {
        [SerializeField] private Rigidbody m_RigidBody;

        private NetworkTankState m_NetTankState;

        private MoveState m_MoveState;


        private float m_ForceSpeed;

        private float m_MoveTimeLeft;

        private void Awake()
        {
            m_NetTankState = GetComponent<NetworkTankState>() != null
                ? GetComponent<NetworkTankState>()
                : gameObject.AddComponent<NetworkTankState>();
        }

        public override void NetworkStart()
        {
            if (!IsServer)
            {
                enabled = false;
                return;
            }

            m_NetTankState.InitObjectNetworkPositionAndYRotation(transform.position, transform.rotation.eulerAngles.y);
            m_MoveState = MoveState.Idle;
        }

        public bool IsMoving()
        {
            return m_MoveState != MoveState.Idle;
        }

        public void CancelMovement()
        {
            m_MoveState = MoveState.Idle;
        }

        private void FixedUpdate()
        {
            PerformMovement();
            var transform1 = transform;
            m_NetTankState.ObjectNetworkPosition.Value = transform1.position;
            m_NetTankState.ObjectNetworkRotationY.Value = transform1.rotation.eulerAngles.y;
            m_NetTankState.ObjectMovementSpeed.Value = m_ForceSpeed;
            m_NetTankState.MoveStatus.Value = GetMovementState();
        }

        public void SetMoveValues(float duration, MoveState moveStatus)
        {
            m_MoveTimeLeft = duration;
            m_MoveState = moveStatus;
        }

        private void PerformMovement()
        {
            if (m_MoveState == MoveState.Idle)
            {
                m_RigidBody.velocity = Vector3.zero;
                return;
            }

            m_MoveTimeLeft -= Time.fixedDeltaTime;
            if (m_MoveTimeLeft <= 0)
            {
                m_MoveState = MoveState.Idle;
                return;
            }

            if (m_MoveState == MoveState.Forward)
            {
                transform.position += transform.forward*0.1f;
            }
            else if (m_MoveState == MoveState.Left)
            {
                transform.Rotate(Vector3.up, -0.1f, 0);
            }
            else if (m_MoveState == MoveState.Right)
            {
                transform.Rotate(Vector3.up, 0.1f, 0);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Wall"))
            {
                m_MoveTimeLeft = 0;
            }
        }

        private TankMoveStatus GetMovementState()
        {
            return m_MoveState switch
            {
                MoveState.Idle => TankMoveStatus.Idle,
                MoveState.Backward => TankMoveStatus.Back,
                MoveState.Left => TankMoveStatus.Left,
                MoveState.Right => TankMoveStatus.Right,
                MoveState.Forward => TankMoveStatus.Forward,
            };
        }
    }
}