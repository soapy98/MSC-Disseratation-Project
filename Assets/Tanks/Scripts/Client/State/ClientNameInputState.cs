using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tanks.Client
{
    public class ClientNameInputState : SceneStateBehaviour
    {
        // Start is called before the first frame update
        protected override GameState ActiveState => GameState.NameInput;
    }
}