using System;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks.Shared
{
    public class UIHealthLocalInfo : MonoBehaviour
    {
        public Slider HealthSlider;
        public Gradient gradient;
        public Image fill;
        public NetworkVariableInt NetworkHealthPoints;

        private void Start()
        {
        }

        private void Update()
        {
            fill.color = gradient.Evaluate(HealthSlider.normalizedValue);
        }

        public float Health
        {
            get => HealthSlider.value;
            set => HealthSlider.value = value;
        }

        public void InitalizeHealth(NetworkVariableInt health, int maxHealth)
        {
            NetworkHealthPoints = health;
            SetMaxHealth(maxHealth);
            NetworkHealthPoints.OnValueChanged += HealthChange;
        }

        public void SetMaxHealth(int health)
        {
            HealthSlider.minValue = 0;
            HealthSlider.maxValue = health;
            Health = health;
            fill.color = gradient.Evaluate(1f);
            HealthChange(health, health);
        }

        void HealthChange(int previousHealth, int newHealth)
        {
            Health = newHealth;
            fill.color = gradient.Evaluate(HealthSlider.normalizedValue);
        }

        private void OnDestroy()
        {
            NetworkHealthPoints.OnValueChanged -= HealthChange;
        }

        public void SetHealth(int health)
        {
            Health = health;
            fill.color = gradient.Evaluate(HealthSlider.normalizedValue);
        }

        public void AddHealth(int health)
        {
            Health += health;
        }

        public void TakeDamage(int health)
        {
            Health -= health;
        }
    }
}