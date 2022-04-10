using System;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using Tanks.Shared;
using UnityEngine;

namespace Tanks.Shared
{
    public class TankOnServer:NetworkBehaviour
    {
        public static readonly List<NetworkObject>m_ActiveTanks = new List<NetworkObject>();
         public List<ulong> m_ActiveTanksNetworkID = new List<ulong>();
        public override void NetworkStart()
        {
            if (!IsServer) enabled = false;
        }

        public void AddTankToServerListList(NetworkObject Tank)
        {
            m_ActiveTanks.Add(Tank);
            m_ActiveTanksNetworkID.Add(Tank.NetworkInstanceId);
        }

        public NetworkObject GetTank(ulong id)
        {
            foreach (var tank in m_ActiveTanks)
            {
                if(tank.NetworkInstanceId == id) 
                    return tank;
            }

            return null;
        }
        public NetworkObject GetTankOffTeam(int team)
        {
            return (from tank in m_ActiveTanks let tankState = tank.GetComponent<NetworkTankState>() where tankState.Team == team select tank).FirstOrDefault();
        }
        public NetworkTankState State(ulong id)
        {
           return GetTank(id).GetComponent<NetworkTankState>();
        }
        public static List<NetworkObject> GetTanksOnServer()
        {
            return m_ActiveTanks;
        }
    }
}