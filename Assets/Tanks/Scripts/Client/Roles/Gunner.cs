using System;
using System.Collections.Generic;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks.Client
{
    //Is responsible for sending gunner actions to the server
    public class Gunner : Role
    {
       [SerializeField] private ActionSender m_Input;
        private Button ShootButton { get; set; }

        public bool shoot = false;
        // Start is called before the first frame update
        public override void SetParent()
        {
            if (GetComponent<Driver>()) Destroy(gameObject.GetComponent<Driver>());
            if (GetComponent<Engineer>()) Destroy(gameObject.GetComponent<Engineer>());
        }
        public void SetShootFunction()
        {
            ShootButton.onClick.AddListener(Shoot);
        }
        public void SetInputSender(ActionSender sender,Button shoot)
        {
            m_Input = sender;
            ShootButton = shoot;
            SetShootFunction();
        }

        public void Shoot()
        {
            if(m_Input.m_Parent.CanPerformActions)
                m_Input.ActionRequest(ActionOption.GunnerAction);
        }

        public override void Update()
        {
            if (shoot)
            {
                Shoot();
            }
            shoot = false;

        }
        

        public Gunner(Button shootButton)
        {
            ShootButton = shootButton;
        }
    }
}