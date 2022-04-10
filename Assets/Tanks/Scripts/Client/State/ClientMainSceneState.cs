using System.Collections;
using MLAPI;
using UnityEngine;
using Tanks;

namespace Tanks.Client
{
    public class ClientMainSceneState : SceneStateBehaviour
    {
        // Start is called before the first frame update
        protected override GameState ActiveState => GameState.MainGame;

        public override void NetworkStart()
        {
            base.NetworkStart();
            if (!IsClient) enabled = false;
        }
    }
}