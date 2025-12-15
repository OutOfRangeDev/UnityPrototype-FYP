using System.Collections;
using Core.Combat;
using UnityEngine;

namespace Core.Enemies
{
    public abstract class EnemyBase : MonoBehaviour
    {
        protected enum EnemyState{Idle, Patrol, Chase, Attack, Stunned}

        [Header("Stats")] 
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float detectionRadius = 10f;
        [SerializeField] protected float attackRange = 1f;
        [SerializeField] protected float stunDuration = 1f;
        
        [Header("Combat Settings")]
        [SerializeField] protected float attackWindupTime = 0.5f; // Warning
        [SerializeField] protected float attackActiveTime = 0.2f; // Damage
        [SerializeField] protected float attackCooldown = 1f;
        [SerializeField] protected int attackDamage = 10;
        [SerializeField] protected Vector2 attackKnockback = new Vector2(5, 2);
        [SerializeField] protected Vector2 attackSize = new Vector2(1.5f, 1f);
        [SerializeField] protected LayerMask playerLayer;
        
        [Header("Debug")]
        [SerializeField] protected EnemyState currentState;

        protected Rigidbody2D Rb;
        protected Health Health;
        protected Transform Target; // The player.
        
        protected float LastAttackTime = -10f;
        protected bool IsAttackingRoutineRunning;
        protected SpriteRenderer SpriteRenderer;
        protected Color OriginalColor;
        
        private float _stunTimer;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Health = GetComponent<Health>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            OriginalColor = SpriteRenderer.color;
        }

        protected virtual void OnEnable()
        {
            Health.OnTakeDamage += HandleHit;
        }

        protected virtual void OnDisable()
        {
            Health.OnTakeDamage -= HandleHit;
        }

        protected void Update()
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    LogicIdle();
                    break;
                case EnemyState.Patrol:
                    LogicPatrol();
                    ScanForPlayer();
                    break;
                case EnemyState.Chase:
                    LogicChase();
                    break;
                case EnemyState.Attack:
                    LogicAttack();
                    break;
                case EnemyState.Stunned:
                    LogicStunned();
                    break;
            }
        }
        
        // ------------------ Logic/States ------------------
        // To be overridden/used by child classes.
        
        protected virtual void LogicIdle(){}
        protected abstract void LogicPatrol(); // Children are forced to use it.

        protected virtual void LogicChase()
        {
            if (Target == null)
            {
                currentState = EnemyState.Patrol;
                return;
            }
            
            float distance = Vector2.Distance(transform.position, Target.position);
            
            if(distance <= attackRange)
            {
                currentState = EnemyState.Attack;
                Rb.linearVelocity = Vector2.zero;
                return;
            }
            
            if (distance > detectionRadius * 1.5f)
            {
                Target = null;
                currentState = EnemyState.Patrol;
                return;
            }
            
            MoveTowards(Target.position);
        }

        protected virtual void LogicAttack()
        {
            Rb.linearVelocity = Vector2.zero;

            if (!IsAttackingRoutineRunning) StartCoroutine(AttackingRoutine());
        }

        protected virtual void LogicStunned()
        {
            // AI disabled.
            _stunTimer -= Time.deltaTime;
            if (_stunTimer <= 0)
            {
                currentState = EnemyState.Chase;
            }
        }
        
        // ------------------ Attacking ------------------

        protected virtual IEnumerator AttackingRoutine()
        {
            IsAttackingRoutineRunning = true;

            // 1. Advise the player, enemy is about to attack.
            SpriteRenderer.color = Color.yellow;
            yield return new WaitForSeconds(attackWindupTime);
            
            // 2. Attack.
            SpriteRenderer.color = Color.purple;
            CheckForHit(); // Can either be active while attacking, or for a brief moment.
            yield return new WaitForSeconds(attackActiveTime);
            
            // 3. Cooldown.
            SpriteRenderer.color = Color.blue;
            yield return new WaitForSeconds(attackCooldown);
            
            // 4. Reset.
            SpriteRenderer.color = OriginalColor;
            LastAttackTime = Time.time;
            IsAttackingRoutineRunning = false;

            currentState = EnemyState.Chase;
        }
        
        // ------------------ Helpers ------------------

        protected virtual void CheckForHit()
        {
            float dir = transform.localScale.x > 0 ? 1 : -1;
            Vector2 attackCenter = (Vector2)transform.position + new Vector2(dir * 0.5f, 0);
            
            Collider2D[] hits = Physics2D.OverlapBoxAll(attackCenter, attackSize, 0f, playerLayer);

            foreach (var hit in hits)
            {
                if(hit.TryGetComponent(out IDamageable health))
                {
                    Vector2 knockbackForce = new Vector2(attackKnockback.x * dir, attackKnockback.y);
                    health.TakeDamage(attackDamage, knockbackForce);
                }
            }
        }

        private void HandleHit()
        {
            // Stunned state. Interrupt everything.
            currentState = EnemyState.Stunned;
            _stunTimer = stunDuration;
        }

        private void ScanForPlayer()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
            Target = hit?.transform;
            currentState = EnemyState.Chase;
        }

        protected void MoveTowards(Vector2 target)
        {
            float direction = (target.x - transform.position.x) > 0 ? 1 : -1;
            
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y , 1);
            
            Rb.linearVelocity = new Vector2(moveSpeed * direction, Rb.linearVelocity.y);
        }
        
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            Gizmos.color = Color.magenta;
            float dir = transform.localScale.x > 0 ? 1 : -1;
            Vector2 center = (Vector2)transform.position + new Vector2(dir * 0.5f, 0);
            Gizmos.DrawWireCube(center, attackSize);
        }
    }
}

