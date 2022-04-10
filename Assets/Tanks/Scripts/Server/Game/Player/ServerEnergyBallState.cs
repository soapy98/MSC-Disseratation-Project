using MLAPI;
using UnityEngine;

namespace Tanks.Shared
{
    public class ServerEnergyBallState : NetworkBehaviour
    {
        private NetworkEnergyBall m_BallState;
        public Color OnColor;
        public Color OffColor;
        public override void NetworkStart()
        {
            if (!IsServer)
            {
                enabled = false;
                return;
            }

            UpdateNetworkState();
        }
        private void UpdateNetworkState()
        {
            m_BallState.ObjectNetworkColor.Value = gameObject.GetComponent<Renderer>().material.color;
        }
       
        protected virtual void ColorEnterChange()
        {
            gameObject.GetComponent<Renderer>().material.color = OnColor;
        }

        protected virtual void ColorExitChange()
        {
            gameObject.GetComponent<Renderer>().material.color = OffColor;
        }


        public void OnEnergyStateChange(EnergyState old, EnergyState newState)
        {
            if (newState == EnergyState.HasEnergy)
            {
                ColorEnterChange();
            }
            else if (newState == EnergyState.NoEnergy)
            {
                ColorExitChange();
            }
        }
    }
}