using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tanks.Client
{
    public class ClientNetworkSetUpState : SceneStateBehaviour
    {
        public static ClientNetworkSetUpState Singleton { get; private set; }
        // Start is called before the first frame update
        protected override GameState ActiveState => GameState.NetworkSetUp;
    }
}