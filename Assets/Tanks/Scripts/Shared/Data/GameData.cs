using System;
using System.Collections;
using System.Collections.Generic;
using Tanks.Shared;
using UnityEngine;

namespace Tanks
{//Class which holds all the action which can occur on the server
    public class GameData : MonoBehaviour
    {
        public static GameData Instance { get; private set; }
        [SerializeField] private ActionInfo[] m_ActionData;
        private Dictionary<ActionOption, ActionInfo> m_ActionDataMap;
        private void Awake()
        {
            if (Instance != null) throw new SystemException("Multiple GameSources!");
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        public Dictionary<ActionOption, ActionInfo> ActionsByDataType
        {
            get
            {
                if (m_ActionDataMap != null) return m_ActionDataMap;
                m_ActionDataMap = new Dictionary<ActionOption, ActionInfo>();
                foreach (var data in m_ActionData)
                {
                    // if (m_ActionDataMap.ContainsKey(data.ActionOption))
                    //     throw new SystemException($"Duplicate action:{data.ActionOption}");
                    m_ActionDataMap[data.ActionOption] = data;
                }

                return m_ActionDataMap;
            }
        }
    }
}