using System.Collections;
using System.Collections.Generic;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks.Client
{
    public class ClientSceneOverState : SceneStateBehaviour
    {
      [SerializeField]  private Button PlayAgain;
      [SerializeField] private Button m_Quit;
        public Text GameOverText;
        // Start is called before the first frame update
        protected override GameState ActiveState => GameState.GameOver;

        protected override void Start()
        {
            base.Start();
             NetworkPortal.m_NetworkPortal.GetComponent<ClientNetworkPortal>().DisconnectReason.SetReasonForDisconnect(ConnectionStatus.RequestedDisconnect);
             m_Quit.onClick.AddListener(GameManager._GameManager._menu.QuitGame);
             PlayAgain.onClick.AddListener(delegate { GameManager._GameManager._menu.LoadScene("Menu");});
        }

        public override void NetworkStart()
        {
            base.NetworkStart();
            if (!IsClient) enabled = false;
            else
            {
                GameOverText.text = GameManager._GameManager.WinningTeam1 ? "Winner" : "Loser";
            }
        }
        
    }
}