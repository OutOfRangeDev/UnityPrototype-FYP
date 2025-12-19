using System.Collections;
using UnityEngine;

namespace Core.Combat.Combat_Attacks
{
    public class Gun : MonoBehaviour
    {
        [Header("Combat stats")]
        [SerializeField] private int damage = 3;
        [SerializeField] private float range = 2;
        [SerializeField] private float fireRate = 1;
        [SerializeField] private LayerMask hitLayer;
        
        [Header("Control")]
        [SerializeField] private Vector2 knockbackForce = new Vector2(0, 4);
        
        [Header("Visuals")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float flashDuration = 0.1f;
        
        private float _nextFireTime;

        public bool IsFiring { get; set; }
        
        private void Awake()
        {
            if(lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
        }

        public void Shoot(Vector2 direction)
        {
            if (Time.time < _nextFireTime) return;
            _nextFireTime = Time.time + fireRate;

            StartCoroutine(FireRoutine(direction));
        }

        private IEnumerator FireRoutine(Vector2 direction)
        {
            IsFiring = true;
            
            // 1. Show the line
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, firePoint.position);
            // Draw the ray
            Vector2 endPos = (Vector2)firePoint.position + (direction * range);
            
            // 2. Check for hit
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, range, hitLayer);

            if (hit.collider != null)
            {
                endPos = hit.point;
                
                if(hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage, knockbackForce);
                }
            }
            
            lineRenderer.SetPosition(1, endPos);
            
            // 3. Cleanup
            yield return new WaitForSeconds(flashDuration);
            lineRenderer.enabled = false;
            
            IsFiring = false;
        }
    }
}
