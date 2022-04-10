using System.Collections;
using System.Collections.Generic;
using Tanks;
using UnityEngine;

namespace Tanks.Client
{
    public class ClientMainMenuState : SceneStateBehaviour
    {
        protected override GameState ActiveState => GameState.MainMenu;
    }
}