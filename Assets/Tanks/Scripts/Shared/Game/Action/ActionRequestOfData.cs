using System;
using MLAPI.Serialization;
using UnityEngine;

namespace Tanks
{
    public enum ActionOption
    {
        None,
        GunnerAction,
        EngineerAction,
        DriverRightAction, 
        DriverLeftAction,
        DriverStopAction,
        DriverForwardAction,
    }

    public enum ActionLogic
    {
        Shoot,
        SpeedUp,
        TurnLeft,
        TurnRight,
        Stop,
        Disable,
        Enable
    }

    public struct ActionData : INetworkSerializable
    {
        public ActionOption WhichAction;
        public Vector3 Position;
        public Vector3 Direction;
        public ulong[] IdsOfTargets;
        public float Total;
        public bool AddToQueue;
        public bool Close;
        public bool Cancel;
        public int Team;

        [Flags]
        private enum PacketFlags
        {
            None = 0,
            HasPos = 1,
            HasDir = 1 << 1,
            HasIds = 1 << 2,
            HasTotal = 1 << 3,
            ToQ = 1 << 4,
            ToClose = 1 << 5,
            CancelMove = 1 << 6,
            Team = 1 << 7
        }


        private PacketFlags GetPacketFlags()
        {
            var packetFlags = PacketFlags.None;
            if (Position != Vector3.zero)
            {
                packetFlags |= PacketFlags.HasPos;
            }

            if (Direction != Vector3.zero)
            {
                packetFlags |= PacketFlags.HasDir;
            }

            if (IdsOfTargets != null)
            {
                packetFlags |= PacketFlags.HasIds;
            }

            if (Math.Abs(Total) > 0.1)
            {
                packetFlags |= PacketFlags.HasTotal;
            }

            if (AddToQueue)
            {
                packetFlags |= PacketFlags.ToQ;
            }

            if (Close)
            {
                packetFlags |= PacketFlags.ToClose;
            }

            if (Cancel)
            {
                packetFlags |= PacketFlags.CancelMove;
            }

            if (Team != 0)
            {
                packetFlags |= PacketFlags.Team;
            }

            return packetFlags;
        }

        public void NetworkSerialize(NetworkSerializer serializer)
        {
            var packetFlags = PacketFlags.None;
            if (!serializer.IsReading) packetFlags = GetPacketFlags();
            serializer.Serialize(ref WhichAction);
            serializer.Serialize(ref packetFlags);
            if (serializer.IsReading)
            {
                AddToQueue = (packetFlags & PacketFlags.ToQ) != 0;
                Cancel = (packetFlags & PacketFlags.CancelMove) != 0;
                Close = (packetFlags & PacketFlags.ToClose) != 0;
            }

            if ((packetFlags & PacketFlags.HasPos) != 0)
            {
                serializer.Serialize(ref Position);
            }

            if ((packetFlags & PacketFlags.HasDir) != 0)
            {
                serializer.Serialize(ref Direction);
            }

            if ((packetFlags & PacketFlags.HasIds) != 0)
            {
                serializer.Serialize(ref IdsOfTargets);
            }

            if ((packetFlags & PacketFlags.HasTotal) != 0)
            {
                serializer.Serialize(ref Total);
            }

            if ((packetFlags & PacketFlags.Team) != 0)
            {
                serializer.Serialize(ref Team);
            }
        }
    }
}