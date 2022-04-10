using System.Collections;
using System.Collections.Generic;
using Tanks.Shared;
using UnityEngine;

public class Disconnect
{
    //Used for disconnecting from the server
    public ConnectionStatus Reason { get; private set; } = ConnectionStatus.Unitialized;
    public void SetReasonForDisconnect(ConnectionStatus status)
    {
        Reason = status;
    }

    public void Clear()
    {
        Reason = ConnectionStatus.Unitialized;
    }

    public bool HasReasonDisconnect => Reason != ConnectionStatus.Unitialized;
}