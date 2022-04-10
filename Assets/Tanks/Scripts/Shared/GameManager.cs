using System;
using System.Collections.Generic;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tanks.Shared
{
    [Serializable]
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject _player;

        [SerializeField] private NetworkPlayerState networkPlayerStateComponent;
        [SerializeField] internal PlayerInfo m_PlayerData;

        //Static reference
        public static GameManager _GameManager;
        public MenuController _menu;
        private GameObject _NetPortal;

        public GameObject NetPortal
        {
            get => _NetPortal;
            set => _NetPortal = value;
        }

        public string Name
        {
            get => m_PlayerData._NetworkName.PName;
            set => m_PlayerData._NetworkName.PName = value;
        }

        public DifferentRoles Role
        {
            get => m_PlayerData._NetworkRole.PRole;
            set => m_PlayerData._NetworkRole.PRole = value;
        }

        public int Number
        {
            get => m_PlayerData.PlayerNumber.Value;
            set => m_PlayerData.PlayerNumber.Value = value;
        }
        //Data to persist
        [SerializeField] private string n;
        public NetworkPlayerState NetworkPlayerStateComponent
        {
            get => networkPlayerStateComponent;
            private set => networkPlayerStateComponent = value;
        }

        private bool WinningTeam = true;

        public bool WinningTeam1
        {
            get => WinningTeam;
            set => WinningTeam = value;
        }

        void Awake()
        {
            _player = GameObject.FindWithTag("Player");
            NetworkPlayerStateComponent = _player.GetComponent<NetworkPlayerState>();
            DontDestroyOnLoad(gameObject);
            if (_GameManager == null)
                _GameManager = this;
            else if (_GameManager != this)
                Destroy(gameObject);
        }
        private void Start()
        {
            n = Name;
        }
      
    }
}