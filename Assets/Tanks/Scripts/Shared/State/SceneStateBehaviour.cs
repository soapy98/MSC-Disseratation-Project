using MLAPI;
using UnityEngine;

namespace Tanks
{
    public enum  GameState
    {
        MainMenu,
        NameInput,
        NetworkSetUp,
        RoleTeamSelect,
        MainGame,
        GameOver
    }
    public abstract class SceneStateBehaviour : NetworkBehaviour
    {
        private bool Survive => false;
        protected abstract GameState ActiveState { get; }
        
        private static GameObject m_ActiveObjectState;
        protected virtual void Start()
        {
            if (m_ActiveObjectState != null)
            {
                if (m_ActiveObjectState == gameObject)  return;
                var previousState = m_ActiveObjectState.GetComponent<SceneStateBehaviour>();
                if (previousState.Survive && previousState.ActiveState == ActiveState)
                {
                    Destroy(gameObject);
                    return;
                }
                Destroy(m_ActiveObjectState);
            }
            m_ActiveObjectState = gameObject;
            if (Survive)  DontDestroyOnLoad(gameObject);
           }
        protected virtual void OnDestroy()
        {
            if (!Survive) m_ActiveObjectState = null;
        }
    }
}