using System;
using System.Collections;
using UnityEngine;

namespace Core.Combat.Combat_Attacks
{
    public class Hook : MonoBehaviour
    {
        [Header( "Settings" )]
        [SerializeField] private float range = 8;
        [SerializeField] private float pullSpeed = 5;
        [SerializeField] private float stopDistance = 1;
        [SerializeField] private LayerMask hitLayer;
        
        [Header( "Visuals" )]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform hookPoint;
        
        private Rigidbody2D _playerRb;
        private Rigidbody2D _targetRb;
        private Vector2 _hookDirection;
        private bool _isPullingPlayer;
        private bool _isPullingEnemy;

        public bool IsHooking { get; set; }
        
        private void Awake()
        {
            _playerRb = GetComponent<Rigidbody2D>();
            if(lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
        }

        public void FireHook(Vector2 direction)
        {
            IsHooking = true;
            
            RaycastHit2D hit = Physics2D.Raycast(hookPoint.position, direction, range, hitLayer);

            if (hit.collider != null) StartCoroutine(GrappleCoroutine(hit));
            else StartCoroutine(MissCoroutine(direction));
        }

        private IEnumerator GrappleCoroutine(RaycastHit2D hit)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, hookPoint.position);
            lineRenderer.SetPosition(1, hit.point);

            bool isHeavy = false;
            _targetRb = hit.collider.TryGetComponent<Rigidbody2D>(out var rb) ? rb : null;

            if (_targetRb == null || _targetRb.bodyType == RigidbodyType2D.Static || _targetRb.mass > 50)
            {
                isHeavy = true;
            }

            if (isHeavy)
            {
                _isPullingPlayer = true;
                _hookDirection = hit.point;
                _playerRb.gravityScale = 0;
            }
            else
            {
                _isPullingEnemy = true;
                if (_targetRb) _targetRb.gravityScale = 0;
            }

            while (_isPullingPlayer || _isPullingEnemy)
            {
                UpdatePhysicsLogic();
                lineRenderer.SetPosition(0, hookPoint.position);
                if(_targetRb) lineRenderer.SetPosition(1, _targetRb.position);
                else lineRenderer.SetPosition(1, _hookDirection);
                
                yield return null;
            }
            
            lineRenderer.enabled = false;
            _playerRb.gravityScale = 1;
            if (_targetRb)
            {
                _targetRb.gravityScale = 1;
                _targetRb = null;
            }
            _isPullingPlayer = false;
            _isPullingEnemy = false;
            IsHooking = false;
        }

        private void UpdatePhysicsLogic()
        {
            if (_isPullingPlayer)
            {
                Vector2 dir = (_hookDirection - (Vector2)transform.position).normalized;
                _playerRb.linearVelocity = dir * pullSpeed;

                if (Vector2.Distance(transform.position, _hookDirection) < stopDistance)
                {
                    _playerRb.linearVelocity = Vector2.zero;
                    _isPullingPlayer = false;
                }
            }
            else if (_isPullingEnemy)
            {
                if (_targetRb == null)
                {
                   _isPullingEnemy = false;
                   return;
                }
                
                Vector2 dir = ((Vector2)transform.position - (Vector2)_targetRb.transform.position).normalized;
                _targetRb.linearVelocity = dir * pullSpeed;
                
                if (Vector2.Distance(transform.position, _targetRb.transform.position) < stopDistance)
                {
                    _targetRb.linearVelocity = Vector2.zero;
                    _isPullingEnemy = false;
                }
            }
        }
        
        private IEnumerator MissCoroutine(Vector2 direction)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, hookPoint.position);
            lineRenderer.SetPosition(1, (Vector2)hookPoint.position + (direction * range));
            yield return new WaitForSeconds(0.5f);
            lineRenderer.enabled = false;
            IsHooking = false;
        }
    }
}