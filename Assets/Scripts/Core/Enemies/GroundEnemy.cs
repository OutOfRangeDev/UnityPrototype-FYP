using UnityEngine;

namespace Core.Enemies
{
    public class GroundEnemy : EnemyBase
    {
        [Header("Patrol Settings")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        
        private int _patrolDirection = 1;

        protected override void LogicPatrol()
        {
            // 1. Check for cliff or wall.
            bool isGroundAhead = 
                Physics2D.Raycast(groundCheck.position, Vector2.down, 1f, groundLayer);
            bool isWallAhead = 
                Physics2D.Raycast(transform.position, Vector2.right * _patrolDirection, 0.5f, groundLayer);

            // 2. Flip if wall ahead or cliff ahead.
            if (!isGroundAhead || isWallAhead) Flip();
            
            // 3. Move towards the target.
            Rb.linearVelocity = new Vector2(moveSpeed * _patrolDirection, Rb.linearVelocity.y);
        }

        private void Flip()
        {
            // Flip the direction.
            _patrolDirection *= -1;
            // Flip the scale.
            transform.localScale = 
                new Vector3(Mathf.Abs(transform.localScale.x) * _patrolDirection, transform.localScale.y, 1);
        }

        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.purple;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 1f);
        }
    }
}
