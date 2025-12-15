using System.Collections;
using Core.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.Combat
{
    public class PlayerCombatController : MonoBehaviour
    {
        [Header( "Dependencies" )]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Rigidbody2D rb;

        [Header("Configuration")] 
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform attackOrigin;
        
        [Header( "Move List" )]
        [SerializeField] private AttackDefinition[] groundCombo;
        [SerializeField] private AttackDefinition airAttack;
        [SerializeField] private AttackDefinition launcherAttack;
        
        [Header( "Camera" )]
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private float stopTimeDuration = 0.1f;
        
        // State
        private bool _isAttacking;
        private int _comboIndex;
        private float _lastAttackTime;
        private readonly float _comboResetTime = 1f;

        private void Start()
        {
            if(impulseSource == null) impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        private void OnEnable()
        {
            inputReader.OnAttack += HandleAttackInput;
        }

        private void OnDisable()
        {
            inputReader.OnAttack -= HandleAttackInput;
        }

        private void HandleAttackInput()
        {
            if (_isAttacking) return;
            
            // 1. Check the input. And if we are on the ground.
            float yInput = inputReader.MoveDirection.y;
            bool isGrounded = IsGrounded();
            
            // 2. Reset the last attack performed.
            AttackDefinition attackToPerform;
            
            // 3. Check for each attack condition.
            // If we are on the ground and holding up, then launch to the air.
            if (isGrounded && yInput > 0.5f)
            {
                attackToPerform = launcherAttack;
                _comboIndex = 0;
            }
            // If we are not on the ground, then it means in the air, air attack.
            else if (!isGrounded)
            {
                attackToPerform = airAttack;
            }
            // If not, then ground combo.
            else
            {
                // Reset the combo if we waited too long.
                if (Time.time - _lastAttackTime > _comboResetTime) _comboIndex = 0;
                // Or we already performed the combo.
                if(_comboIndex >= groundCombo.Length) _comboIndex = 0;
                
                attackToPerform = groundCombo[_comboIndex];
                _comboIndex++;
            }

            if (attackToPerform != null)
            {
                StartCoroutine(PerformAttack(attackToPerform));
            }
        }

        private IEnumerator PerformAttack(AttackDefinition attack)
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;
            
            // 1. Prepare for the attack.
            // Play the animation, preparing for the attack.
            yield return new WaitForSeconds(attack.startupTime);
            
            // 2. Attack phase.
            // Apply self knockback.
            if(attack.selfKnockback != Vector2.zero)
            {
                // Reset Y velocity, for launching to the air.
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                // We asume is facing right. If left, flip X.
                float direction = transform.localScale.x > 0 ? 1 : -1;
                Vector2 force = new Vector2(attack.selfKnockback.x * direction, attack.selfKnockback.y);
                rb.AddForce(force, ForceMode2D.Impulse);
            }
            
            // 3. Detect the enemies
            DetectAndDamage(attack);
            yield return new WaitForSeconds(attack.activeTime);
            
            // 4. Ending phase. Cooldown.
            yield return new WaitForSeconds(attack.recoveryTime);
            
            _isAttacking = false;
        }

        private void DetectAndDamage(AttackDefinition attack)
        {
            // Calculate the hitbox position, based on the direction.
            float direction = transform.localScale.x > 0 ? 1 : -1;
            Vector2 offset = new Vector2(attack.hitboxOffset.x * direction, attack.hitboxOffset.y);
            Vector2 center = ( Vector2 ) attackOrigin.position + offset;
            
            // Get everything inside the collider.
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(center, attack.hitboxSize, 0f, enemyLayer);
            bool hitSomething = false;

            foreach (var enemy in hitEnemies)
            {
                if(enemy.TryGetComponent<Health>(out var health))
                {
                    // Apply damage and knockback.
                    Vector2 knockbackForce = new Vector2(attack.targetKnockback.x * direction, attack.targetKnockback.y);
                    health.TakeDamage(attack.damage, knockbackForce);
                    hitSomething = true;
                }
            }

            if (hitSomething)
            {
                if(impulseSource != null)
                {
                    float shakeMultiplier = attack.damage * 0.1f;
                    impulseSource.GenerateImpulse(shakeMultiplier);
                }
                StartCoroutine(HitStopCoroutine(stopTimeDuration));
            }
        }
        
        private IEnumerator HitStopCoroutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }

        private bool IsGrounded()
        {
            return Physics2D.Raycast(transform.position, Vector2.down, 1.1f, LayerMask.GetMask("Ground"));
        }
        
        // --------------- Gizmos debug ------------------
        private void OnDrawGizmos()
        {
            if(attackOrigin == null) return;

            AttackDefinition debugAttack = launcherAttack; // Hitbox of the attack. For now change this manually.

            if (debugAttack != null)
            {
                Gizmos.color = Color.yellow;
                float direction = transform.localScale.x > 0 ? 1 : -1;
                Vector2 offset = new Vector2(debugAttack.hitboxOffset.x * direction, debugAttack.hitboxOffset.y);
                Vector2 center = ( Vector2 ) attackOrigin.position + offset;
                Gizmos.DrawWireCube(center, debugAttack.hitboxSize);
            }
        }
    }
}