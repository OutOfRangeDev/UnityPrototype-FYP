using System.Collections;
using UnityEngine;

namespace Core.Boss
{
    public class BossController : MonoBehaviour
    {
        public enum BossPhase { Phase1, Phase2, Phase3 }
        
        [Header( "Dependencies" )]
        [SerializeField] private BossHealth bossHealth;
        
        [Header( "State" )]
        [SerializeField] public BossPhase currentPhase = BossPhase.Phase1;
        
        private void Start()
        {
            bossHealth.OnPhaseChanged += HandlePhaseChange;
        }

        private void OnDestroy()
        {
            bossHealth.OnPhaseChanged -= HandlePhaseChange;
        }
        
        void HandlePhaseChange(int newPhaseIndex)
        {
            Debug.Log($"BOSS ENTERING NEW PHASE {newPhaseIndex}");

            switch (newPhaseIndex)
            {
                case 1: currentPhase = BossPhase.Phase2; break;
                case 2: currentPhase = BossPhase.Phase3; break;
            }

            StartCoroutine(PhaseTransitionRoutine());
        }
        
        private IEnumerator PhaseTransitionRoutine()
        {
            if(bossHealth.isVulnerableDuringTrasition) bossHealth.SetInvincibility(true);

            yield return new WaitForSeconds(2);
            
            bossHealth.SetInvincibility(false);
        }

        private void Update()
        {
            // Each phase logic.
        }
    }
}