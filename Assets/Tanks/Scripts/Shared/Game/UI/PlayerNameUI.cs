using System;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using TMPro;

namespace Tanks
{
    public class PlayerNameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_UIName;
        private NetworkVariableString m_NetworkName;

        public void Init(NetworkVariableString name)
        {
            m_NetworkName = name;
            m_UIName.text = name.Value;
            m_NetworkName.OnValueChanged += NameChange;
        }

        void NameChange(string previous, string newName)
        {
            m_UIName.text = newName;
        }

        private void OnDestroy()
        {
            m_NetworkName.OnValueChanged -= NameChange;
        }
    }
}