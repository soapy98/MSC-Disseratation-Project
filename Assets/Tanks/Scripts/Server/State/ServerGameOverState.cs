using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tanks.Server
{
    public class ServerGameOverState : SceneStateBehaviour
    {
        [SerializeField]
        protected override GameState ActiveState => GameState.GameOver;
        public override void NetworkStart()
        {
            base.NetworkStart();
            if (!IsServer) enabled = false;
            else
            {
            }
        }
    }
}