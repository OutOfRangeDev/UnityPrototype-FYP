using UnityEngine;

namespace Core.Combat
{
    public class CombatEffectsManager : MonoBehaviour
    {
        [Header("Assets")]
        [SerializeField] private GameObject damagePopUpPrefab;

        private void Start()
        {
            //For fast prototyping we get all global healths. In real it would be a global event.
            var allHealths = FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (var health in allHealths)
            {
                RegisterHealth(health);
            }
        }


        private void RegisterHealth(Health health)
        {
            health.OnDamageTaken += HandleDamage;
        }

        private void HandleDamage(Vector3 position, int damageAmount)
        {
            if(damagePopUpPrefab == null) return;
            
            GameObject popup = Instantiate(damagePopUpPrefab, position, Quaternion.identity);
            popup.GetComponent<UI.FloatingText>().SetUp(damageAmount);
        }
    }
}