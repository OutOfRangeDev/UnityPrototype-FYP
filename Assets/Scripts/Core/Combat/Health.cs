using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [Header( "Stats" )]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invincibilityDuration = 1f;
        
        [Header( "Dependencies" )]
        // If empty, knockback won't happen. Good for boxes.
        [SerializeField] private Rigidbody2D rb;
        
        public event UnityAction<int> OnHealthChanged = delegate { };
        public event UnityAction OnTakeDamage = delegate { };
        public event UnityAction OnDeath = delegate { };
        
        private int _currentHealth;
        private bool _isInvincible;
        
        public bool IsAlive => _currentHealth > 0;

        private void Awake()
        {
            _currentHealth = maxHealth;
            if(rb == null) rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            // For the UI.
            OnHealthChanged.Invoke(_currentHealth);
        }

        public bool TakeDamage(int amount, Vector2 knockBackForce)
        {
            // 1. If it's alive or invincible, don't take damage.
            if(!IsAlive || _isInvincible) return false;
            
            // 2. Apply damage
            _currentHealth -= amount;
            
            // 3. Notify the listeners
            OnHealthChanged.Invoke(_currentHealth);
            OnTakeDamage.Invoke();
            
            // 4. Knockback
            if (rb != null && knockBackForce != Vector2.zero)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(knockBackForce, ForceMode2D.Impulse);
            }
            
            // 5. Handle death or damage
            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvincibilityRoutine());
            }

            return true;
        }

        private void Die()
        {
            OnDeath.Invoke();
            // For now, just deactivate the object. Objective: Death scene
            Debug.Log($"{gameObject.name} has died.");
            gameObject.SetActive(false);
        }
        
        private IEnumerator InvincibilityRoutine()
        {
            _isInvincible = true;
            yield return new WaitForSeconds(invincibilityDuration);
            _isInvincible = false;
        }
        
        public void SetInvincibility(bool value) => _isInvincible = value;
        
        [ContextMenu( "Heal" )]
        public void Heal()
        {
            _currentHealth = maxHealth;
            OnHealthChanged.Invoke(_currentHealth);
        }
        
        [ContextMenu( "Half Health" )]
        public void HalfHealth() => TakeDamage(maxHealth / 2, Vector2.zero);
        
        [ContextMenu( "Kill" )]
        public void Kill() => TakeDamage(maxHealth, Vector2.zero);
    }
}