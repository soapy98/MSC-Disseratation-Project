using System.Collections;
using System.Collections.Generic;
using MLAPI.MonoBehaviours.Core;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tanks
{
    public class NetworkName : NetworkedBehaviour
    {
        public NetworkVariableString Name = new NetworkVariableString();
    }
}