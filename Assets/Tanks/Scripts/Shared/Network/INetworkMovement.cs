using UnityEngine;
using MLAPI.NetworkVariable;

namespace Tanks.Shared
{
    public interface INetworkMovement
    {
        NetworkVariableVector3 ObjectNetworkPosition { get; }
        NetworkVariableFloat ObjectNetworkRotationY { get; }
        NetworkVariableFloat ObjectMovementSpeed { get; }
        void InitObjectNetworkPositionAndYRotation(Vector3 startPos, float rotY);
    }
}