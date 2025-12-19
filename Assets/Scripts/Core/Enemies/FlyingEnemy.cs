using UnityEngine;

namespace Core.Enemies
{
    public class FlyingEnemy : EnemyBase
    {
        [Header("Flying Settings")]
        [SerializeField] private float hoverHeight = 2f;
        [SerializeField] private float bobAmplitude = 0.5f;
        [SerializeField] private float bobFrequency = 2f;

        protected override void LogicPatrol()
        {
            float newY = transform.position.y + (Mathf.Sin(Time.time * bobFrequency) * bobAmplitude * Time.deltaTime);
            Rb.linearVelocity = new Vector2(0, Mathf.Sin(newY));
        }

        protected override void LogicChase()
        {
            if (Target == null)
            {
                currentState = EnemyState.Patrol; 
                return;
            }
            
            Vector2 targetPos = Target.position;
            targetPos.y += hoverHeight;
            
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            Rb.linearVelocity = direction * moveSpeed;

            if (direction.x != 0)
            {
                float facing = direction.x > 0 ? 1 : -1;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facing, transform.localScale.y, 1);
            }

            if (Vector2.Distance(transform.position, targetPos) < attackRange)
            {
                currentState = EnemyState.Attack;
            }
        }
    }
}