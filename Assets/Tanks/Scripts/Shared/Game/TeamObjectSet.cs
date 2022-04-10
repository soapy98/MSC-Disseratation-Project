using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks.Shared
{
    public class TeamObjectSet : MonoBehaviour
    {
        public NetworkEnergyHealthState TankHealth;
        public GameObject GunnerObjects;
        public Button ShootButton;

        public GameObject DriverObjects;
        public Button[] DriverButtons; 
        

        public GameObject EngineerObjects;
        public NetworkEnergyBall DriverBall;
        public NetworkEnergyBall GunnerBall;

        public bool SinglePlayer = false;

    }
}