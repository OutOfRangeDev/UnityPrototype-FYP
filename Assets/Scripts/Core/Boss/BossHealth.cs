using Core.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Boss
{
    public class BossHealth : Health
    {
        [Header("Boss Stats")]
        [SerializeField] private float[] phaseTresholds = {0.6f, 0.3f};
        [SerializeField] public bool isVulnerableDuringTrasition = false;
        
        public event UnityAction<int> OnPhaseChanged = delegate { };
        
        private int _currentPhase;
        private bool _isChangingPhase;

        public override bool TakeDamage(int damageAmount, Vector2 knockBackForce)
        {
            if (_isChangingPhase) return false;
            
            bool damageApplied = base.TakeDamage(damageAmount, knockBackForce);
            
            if (damageApplied) CheckPhaseThreshold();
            
            return damageApplied;
        }

        private void CheckPhaseThreshold()
        {
            if (_currentPhase >= phaseTresholds.Length) return;
            
            float lifePercent = (float)_currentHealth / (float)maxHealth;

            if (lifePercent <= phaseTresholds[_currentPhase])
            {
                _currentPhase++;
                OnPhaseChanged.Invoke(_currentPhase);
                
                // Transition to next phase
            }
        }
    }
}