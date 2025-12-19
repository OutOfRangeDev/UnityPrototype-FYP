using System.Collections;
using Core.Combat.Combat_Attacks;
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
        [SerializeField] private PlayerController playerController;
        
        [Header("Configuration")] 
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform attackOrigin;
        
        [Header( "Move List" )]
        [SerializeField] private AttackDefinition[] groundCombo;
        [SerializeField] private AttackDefinition[] airCombo;
        [SerializeField] private AttackDefinition dashAttack;
        [SerializeField] private AttackDefinition launcherAttack;
        
        [Header( "Tools" )]
        [SerializeField] private Hook hook;
        [SerializeField] private Gun gun;
        
        
        [Header( "Camera" )]
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private float stopTimeDuration = 0.1f;
        
        [Header("Combo Logic")]
        [SerializeField] private float comboResetTime = 1f;
        [SerializeField] private float stepMultiplier = 0.2f;
        [SerializeField] private float chainMultiplier = 0.1f;
        
        // State
        private bool _isAttacking;
        private float _lastAttackTime;
        private int _comboIndex;
        private int _globalChainCount;
        private float _extraBonus;

        private bool IsBusy()
        {
            return _isAttacking|| (hook != null && hook.IsHooking) || (gun != null && gun.IsFiring);
        }

        private void Start()
        {
            if(impulseSource == null) impulseSource = GetComponent<CinemachineImpulseSource>();
            if(playerController == null) playerController = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            inputReader.OnAttack += HandleAttackInput;
            inputReader.OnHook += HandleHook;
            inputReader.OnGun += HandleGun;
        }

        private void OnDisable()
        {
            inputReader.OnAttack -= HandleAttackInput;
            inputReader.OnHook -= HandleHook;
            inputReader.OnGun -= HandleGun;
        }

        private void HandleAttackInput()
        {
            if (IsBusy()) return;
            
            if(Time.time - _lastAttackTime > comboResetTime)
            {
                // Reset combo
                _comboIndex = 0;
                _globalChainCount = 0;
            }

            _extraBonus = 0;
            float yInput = inputReader.MoveDirection.y;
            bool isGrounded = IsGrounded();
            bool isDashing = playerController.IsDashing;
            AttackDefinition attackToPerform;
            
            // Attack Selection
            
            // If only dashing
            if (isDashing)
            {
                attackToPerform = dashAttack;
                _comboIndex = 0;
                _extraBonus = 0.2f; // Extra bonus for dash attacks (style)
            }
            
            if (isGrounded && yInput > 0.5f && !isDashing)
            {
                attackToPerform = launcherAttack;
                _comboIndex = 0;
            }
            
            else if(!isGrounded && !isDashing)
            {
                if (_comboIndex >= airCombo.Length)
                {
                    _comboIndex = 0;
                    _extraBonus = 0.2f; // Extra bonus for completing combo.
                }
                attackToPerform = airCombo[_comboIndex];
            }
            else
            {
                if (_comboIndex >= groundCombo.Length)
                {
                    _comboIndex = 0;
                    _extraBonus = 0.2f; // Extra bonus for completing combo.
                }
                attackToPerform = groundCombo[_comboIndex];
            }

            if (attackToPerform != null)
            {
                float stepBonus = _comboIndex * stepMultiplier;
                
                float chainBonus = _globalChainCount * chainMultiplier;

                float totalMultiplier = 1f + chainBonus + stepBonus + _extraBonus;

                if (attackToPerform == groundCombo[_comboIndex] || attackToPerform == airCombo[_comboIndex])
                {
                    _comboIndex++;
                }
                _globalChainCount++;
                
                StartCoroutine(PerformAttack(attackToPerform, totalMultiplier));
            }
        }

        private IEnumerator PerformAttack(AttackDefinition attack, float damageMultiplier)
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
            DetectAndDamage(attack, damageMultiplier);
            yield return new WaitForSeconds(attack.activeTime);
            
            // 4. Ending phase. Cooldown.
            yield return new WaitForSeconds(attack.recoveryTime);
            
            _isAttacking = false;
        }

        private void DetectAndDamage(AttackDefinition attack, float damageMultiplier)
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

                    int baseDamage = attack.damage;
                    int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier); // Apply combo bonus
                    
                    health.TakeDamage(finalDamage, knockbackForce);
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
        
        private void HandleHook()
        {
            if(IsBusy()) return;
            
            if(hook != null)
            {
                float facingDir = transform.localScale.x > 0 ? 1 : -1;
                Vector2 dir = new Vector2(facingDir, 0);
                hook.FireHook(dir);
            }
        }

        private void HandleGun()
        {
            if (IsBusy()) return;
            
            if(gun != null)
            {
                float facingDir = transform.localScale.x > 0 ? 1 : -1;
                Vector2 dir = new Vector2(facingDir, 0);
                gun.Shoot(dir);
            }
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