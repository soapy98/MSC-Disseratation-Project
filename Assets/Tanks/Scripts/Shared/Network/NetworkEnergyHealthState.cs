using System;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks.Shared
{
    public class NetworkEnergyHealthState : NetworkBehaviour
    {
        public NetworkVariableInt HealthPoints = new NetworkVariableInt();
        public event Action HealthEnergyDamage;
        public event Action HealthEnergyReplenish;
        private void OnEnable()
        {
            HealthPoints.OnValueChanged += HealthChange;
        }

        private void OnDisable()
        {
            HealthPoints.OnValueChanged -= HealthChange;
        }

        void HealthChange(int previousHealth, int newHealth)
        {
            if (previousHealth > 0 && newHealth <= 0)
            {
                HealthEnergyDamage?.Invoke();
            }
            else if(previousHealth<=0&&newHealth>0)
            {
                HealthEnergyReplenish?.Invoke();
            }
        }

    }
}