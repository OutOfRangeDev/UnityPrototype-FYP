using System.Collections;
using Core.Combat;
using UnityEngine;

namespace Core.Input
{
    public class PlayerController : MonoBehaviour
    {
        [Header( "Dependencies" )]
        [SerializeField] private InputReader inputReader;
        
        [Header( "Movement Settings" )]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float jumpForce = 16f;
        [SerializeField] private float jumpCutMultiplier = 0.5f; // How much velocity is kept when jumping early.
        [SerializeField] private float gravityScale = 3f; // High gravity, less floaty.
        [SerializeField] private float fallGravityMultiplier = 1.5f; // Fall faster than rise.
        
        [Header( "Game feel" )]
        [SerializeField] private float coyoteTime = 0.15f; // Time allowed to jump after leaving the ground.
        [SerializeField] private float jumpBufferTime = 0.1f; // Time allowed to queue a jump before hitting the ground.
        
        [Header( "Dash Settings" )]
        [SerializeField] private float dashSpeed = 24f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;
        
        [Header( "Ground Detection" )]
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private Vector2 groundCheckSize = new Vector2( 0.8f, 0.2f );
        
        //States
        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private bool _isGrounded;
        private bool _isFacingRight = true;
        
        private bool _isDashing;
        private float _lastDashTime = -10f;
        private float _originalGravity;
        private float _dashDirection;
        private Health _health;
        
        //Timers
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;

        #region Lifetime Cycle

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = gravityScale;
        }
        
        private void Start()
        {
            _health = GetComponent<Health>();
            _originalGravity = _rb.gravityScale;
        }
        
        private void OnEnable()
        {
            inputReader.OnJumpInitiated += HandleJumpInitiated;
            inputReader.OnAttack += HandleJumpCancelled;
            inputReader.OnDash += HandleDash;
        }

        private void OnDisable()
        {
            inputReader.OnJumpInitiated -= HandleJumpInitiated;
            inputReader.OnAttack -= HandleJumpCancelled;
            inputReader.OnDash -= HandleDash;
        }

        private void Update()
        {
            // 1. Read the input.
            _moveInput = inputReader.MoveDirection;
            
            // 2. Flip logic.
            if (_moveInput.x != 0)
                TurnCheck(_moveInput.x > 0);
            
            // Update the timers.
            
            // If it's on the ground, reset the coyote time. If it's not, decrement the timer.
            if(_isGrounded) _coyoteTimeCounter = coyoteTime;
            else _coyoteTimeCounter -= Time.deltaTime;
            
            // If the button is pressed, reset the timer. Always count down.
            if(_jumpBufferCounter > 0) _jumpBufferCounter -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            // If dashing, override all other movement.
            if (_isDashing)
            {
                _rb.linearVelocity = new Vector2(_dashDirection * dashSpeed, 0f);
                return;
            }
            
            // 4. Ground Check.
            _isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayerMask);
            
            // 5. Apply horizontal velocity.
            // We let the vertical be controlled by the physics. 
            _rb.linearVelocity = new Vector2(_moveInput.x * moveSpeed, _rb.linearVelocity.y);
            
            // 6. Gravity modification.
            // If we are falling, add even more velocity. If not, we are on the ground, so reset it to normal.
            if(_rb.linearVelocity.y < 0) _rb.gravityScale = gravityScale * fallGravityMultiplier;
            else _rb.gravityScale = gravityScale;
            
            // 7. Check jump conditions.
            if (_jumpBufferCounter > 0 && _coyoteTimeCounter > 0)
            {
                PerformJump();
            }
        }

        #endregion
        
        #region Jump
        
        private void HandleJumpInitiated()
        {
            // Instead of jumping right away, we queue the jump.
            // Then in the FixedUpdate, the buffer and coyote decide if we jump or not.
            _jumpBufferCounter = jumpBufferTime;
        }

        private void PerformJump()
        {
            _jumpBufferCounter = 0;
            _coyoteTimeCounter = 0;
            
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        }

        private void HandleJumpCancelled()
        {
            if (_rb.linearVelocity.y < 0)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * jumpCutMultiplier);
            }
        }
        
        #endregion

        #region Dash

        public bool IsDashing => _isDashing;
        
        private void HandleDash()
        {
            if (_isDashing || Time.time - _lastDashTime < dashCooldown) return;
            
            StartCoroutine(PerformDash());
        }

        private IEnumerator PerformDash()
        {
            // 1. Prepare for the dash.
            _isDashing = true;
            _lastDashTime = Time.time;
            _health?.SetInvincibility(true);

            // 2. Calculate the dash direction.
            if(Mathf.Abs(_moveInput.x) > 0.1f)
                _dashDirection = Mathf.Sign(_moveInput.x);
            else
                _dashDirection = _isFacingRight ? 1f : -1f;
            
            // 3. Dash!
            _originalGravity = _rb.gravityScale;
            _rb.gravityScale = 0;
            
            _rb.linearVelocity = new Vector2(_dashDirection * dashSpeed, _rb.linearVelocity.y);
            
            yield return new WaitForSeconds(dashDuration);
            
            // 4. Reset.
            _rb.gravityScale = _originalGravity;
            _rb.linearVelocity = Vector2.zero;
            _isDashing = false;
            _health?.SetInvincibility(false);
        }
        
        #endregion

        private void TurnCheck(bool moveRight)
        {
            if (_isFacingRight == moveRight) return;
            
            // Flip the character. (Multiplying the X scale by -1)
            _isFacingRight = moveRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
    }
}