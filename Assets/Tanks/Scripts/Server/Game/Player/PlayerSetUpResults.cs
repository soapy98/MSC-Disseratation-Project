using System.Collections;
using System.Collections.Generic;
using Tanks.Shared;
using UnityEngine;

namespace Tanks.Server
{
    public class PlayerSetUpResults
    {
        public readonly Dictionary<ulong, PlayerSelectChoice> choices = new Dictionary<ulong, PlayerSelectChoice>();

        public struct PlayerSelectChoice
        {
            public int PlayerNumber;
            public DifferentRoles PlayerRole;
            public int PlayerTeam;
            public int RoleSprite;

            public PlayerSelectChoice(int playerNumber,int sprite, int playerTeam = 0, DifferentRoles role = DifferentRoles.None)
            {
                PlayerNumber = playerNumber;
                RoleSprite = sprite;
                PlayerRole = role;
                PlayerTeam = playerTeam;
            }
        }
    }
}