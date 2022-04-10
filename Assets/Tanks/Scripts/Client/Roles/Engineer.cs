using System;
using MLAPI.NetworkVariable;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tanks.Client
{
    //Is reponsible for sending engineer actions to the server
    public class Engineer : Role
    {
        private GameObject EngineerObject;
        [SerializeField] private ActionSender m_Input;


        public GameObject EngineerObjs
        {
            get => EngineerObject;
            set => EngineerObject = value;
        }

        private GameObject DriverHealthObject;

        public GameObject DriverHealthObj
        {
            get => DriverHealthObject;
            set => DriverHealthObject = value;
        }

        private GameObject GunnerHealthObject;

        public GameObject GunnerHealthObj
        {
            get => GunnerHealthObject;
            set => GunnerHealthObject = value;
        }

        private NetworkVariable<NetworkEnergyHealthState> m_DriverBar { get; } = new NetworkVariable<NetworkEnergyHealthState>();

        public NetworkEnergyHealthState DriverState
        {
            get => m_DriverBar.Value;
            set => m_DriverBar.Value = value;
        }

        private NetworkVariable<NetworkEnergyHealthState> m_Gunner { get; } = new NetworkVariable<NetworkEnergyHealthState>();

        public NetworkEnergyHealthState GunnerState
        {
            get => m_Gunner.Value;
            set => m_Gunner.Value = value;
        }

        public ClientEnergyBall DriverBall;

        public ClientEnergyBall GunnerBall;
        // Start is called before the first frame update
        public override void SetParent()
        {
            if (GetComponent<Driver>()) Destroy(gameObject.GetComponent<Driver>());
            if (GetComponent<Gunner>()) Destroy(gameObject.GetComponent<Gunner>());
        } 
        public void SetInputSender(ActionSender sender)
        {
            m_Input = sender;
        }

        public void SetEnergyObjects(ClientEnergyBall driver, ClientEnergyBall gunner)
        {
            DriverBall = driver;
            DriverBall.m_Input = m_Input; 
            GunnerBall = gunner;
        }
        
        public void LinkEnergyBarsWithObjects()
        {
            
            DriverState = GetComponent<NetworkPlayerState>().TeamTankState.PlayerInfoByRole[DifferentRoles.Driver]
                .NetworkEnergyState;  // DriverHealthObj.GetComponentInChildren<NetworkEnergyHealthState>();
            GunnerState =GetComponent<NetworkPlayerState>().TeamTankState.PlayerInfoByRole[DifferentRoles.Gunner]
                .NetworkEnergyState;// GunnerHealthObject.GetComponentInChildren<NetworkEnergyHealthState>();
        }
        public override void Start()
        {
            SetParent();
        }
        public Engineer()
        {
        }
    }
}