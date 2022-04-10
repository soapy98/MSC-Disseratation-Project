using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

namespace Tanks.Shared
{
    public class NetworkBulletState:NetworkBehaviour,INetworkMovement
    {
        public NetworkVariableVector3 ObjectNetworkPosition { get; }=new NetworkVariableVector3();
        public NetworkVariableFloat ObjectNetworkRotationY { get; }=new NetworkVariableFloat();
        public NetworkVariableFloat ObjectMovementSpeed { get; }=new NetworkVariableFloat();
        public void InitObjectNetworkPositionAndYRotation(Vector3 startPos, float rotY)
        {
            ObjectNetworkPosition.Value = startPos;
            ObjectNetworkRotationY.Value = rotY;
        }
        public NetworkVariableInt BulletNetworkTeam { get; }=new NetworkVariableInt();
    }
}