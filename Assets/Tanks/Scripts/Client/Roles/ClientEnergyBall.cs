using UnityEngine;

namespace Tanks.Client
{
    //Handles sending a engineer action to the server based on a client hand collision
    public class ClientEnergyBall : MonoBehaviour
    {
        
        public ActionSender m_Input { get; set; }
        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.name.Contains("Hand"))
                m_Input.ActionRequest(ActionOption.EngineerAction);
        }
        
        
    }
}