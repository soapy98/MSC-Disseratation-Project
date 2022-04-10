using System.Collections;
using System.Collections.Generic;
using MLAPI.MonoBehaviours.Core;
using MLAPI.NetworkVariable;
using Tanks.Shared;
using UnityEngine;

public class NetworkTankLifeState : NetworkedBehaviour
{
   [SerializeField]
   NetworkVariable<TankLifeState>m_LifeState = new NetworkVariable<TankLifeState>();
   public NetworkVariable<TankLifeState> TankLifeState => m_LifeState;
}
