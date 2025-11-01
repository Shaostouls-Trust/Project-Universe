using UnityEngine;
using System;

namespace ProjectUniverse.PowerSystem
{
    [Serializable]
    public class CableSegmentData 
    {
        [SerializeField] private float _health;
        [SerializeField] private float _heat;
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _maxHeat;
        [SerializeField] private bool _isBroken;

        public float Health => _health;
        public float Heat => _heat;
        public float MaxHealth => _maxHealth;
        public float MaxHeat => _maxHeat;
        public bool IsBroken => _isBroken;

        public CableSegmentData(float maxHealth, float maxHeat)
        {
            this._maxHealth = maxHealth;
            this._maxHeat = maxHeat;
            this._health = maxHealth;
            this._heat = 0f;
            this._isBroken = false;
        }

        public void ApplyDamage(float damage)
        {
            _health = Mathf.Max(0f, _health - damage);
            if (_health <= 0f)
            {
                _isBroken = true;
            }
        }

        public void ApplyHeat(float heatAmount)
        {
            _heat = Mathf.Min(_maxHeat, _heat + heatAmount);
            // Heat causes damage when at max
            if (_heat >= _maxHeat)
            {
                ApplyDamage(1f); // 1 damage per frame when overheated
            }
        }

        public void CoolDown(float coolAmount)
        {
            _heat = Mathf.Max(0f, _heat - coolAmount);
        }

        public float GetHealthPercentage() => _health / _maxHealth;
        public float GetHeatPercentage() => _heat / _maxHeat;
        public bool IsOperational() => !_isBroken && _health > 0f;
    }
}