using System.Collections;
using System.Collections.Generic;
using MLAPI.MonoBehaviours.Core;
using MLAPI.NetworkVariable;
namespace Tanks.Shared
{
    public class NetworkRoleTypeState : NetworkedBehaviour
    {
        public NetworkVariable<DifferentRoles> RoleType = new NetworkVariable<DifferentRoles>();
    }
}