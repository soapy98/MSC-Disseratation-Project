using System;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks.Client
{
    //Is responsible for sending Driver actions from a client
    public class Driver : Role
    {
        public Button[] TurnButtons;
        [SerializeField] private ActionSender m_Input;
        public bool left = false, right, forward, stop;
        public override void SetParent()
        {
            if (GetComponent<Engineer>()) Destroy(gameObject.GetComponent<Engineer>());
            if (GetComponent<Gunner>()) Destroy(gameObject.GetComponent<Gunner>());
        }

        public void SetInputSender(ActionSender sender)
        {
            m_Input = sender;
        }

        public void SetButtonEvent(Button driverAction)
        {
            switch (driverAction.name)
            {
                case "Right":
                    driverAction.onClick.AddListener(Right);
                    break;
                case "Left":
                    driverAction.onClick.AddListener(Left);
                    break;
                case "Stop":
                    driverAction.onClick.AddListener(Stop);
                    break;
                case "Go":
                    driverAction.onClick.AddListener(Forward);
                    break;
            }
        }
        
        private void Left()
        {
            if (m_Input.m_Parent.CanPerformActions)
                m_Input.ActionRequest(ActionOption.DriverLeftAction);
        }

        private void Right()
        {
            if (m_Input.m_Parent.CanPerformActions)
                m_Input.ActionRequest(ActionOption.DriverRightAction);
        }

        private void Forward()
        {
            if (m_Input.m_Parent.CanPerformActions)
                m_Input.ActionRequest(ActionOption.DriverForwardAction);
        }

        private void Stop()
        {
            if (m_Input.m_Parent.CanPerformActions)
                m_Input.ActionRequest(ActionOption.DriverStopAction);
        }

        void Update()
        {
            if (left)
            {
                Left();
            }
            if (right)
            {
                Right();
            }
            if (forward)
            {
                Forward();
            }

            left = false;
            right = false;
            forward = false;
        }
        public Driver()
        {
        }
    }
}