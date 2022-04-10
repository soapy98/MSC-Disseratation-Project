using System;
using Tanks.Server;
using UnityEngine;
//Interface to enable object to be damaged
public interface IDamage
{
    void AffectHealth(ServerTank inflict, int HP);
    // void AffectHealth(ServerPlayer player, int HP);
    ulong NetworkObjectId { get; }
    Transform transform { get; }
    bool TakesDamage();
}
