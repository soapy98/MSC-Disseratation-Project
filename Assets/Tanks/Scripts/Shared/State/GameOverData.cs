using MLAPI;
using MLAPI.NetworkVariable;

namespace Tanks
{
    public class GameOverData:NetworkBehaviour
    {
        public enum WinState
        {
            None,
            Win,
            Lose
        }
    }
}