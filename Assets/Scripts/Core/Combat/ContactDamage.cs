using System;
using UnityEngine;

namespace Core.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class ContactDamage : MonoBehaviour
    {
        // NOT IMPLEMENTED
        [SerializeField] private int damageAmount = 10;
        [SerializeField] private Vector2 knockbackForce = new Vector2(5, 2);

        private void OnCollisionStay2D(Collision2D other)
        {
            // Run this if physical collision is detected.
            CheckTouch(other.collider);
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            // This runs if trigger collision is detected.
            CheckTouch(other);
        }

        private void CheckTouch(Collider2D other)
        {
            // Try to find the damageable component.
            if(other.TryGetComponent(out IDamageable damageable))
            {
                // Apply damage and knockback.
                Vector2 direction = (other.transform.position - transform.position).normalized;
                Vector2 force = new Vector2(Mathf.Sign(direction.x) * knockbackForce.x, knockbackForce.y);
                
                damageable.TakeDamage(damageAmount, force);
            }
        }
    }
}