using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Core.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
    public class InputReader : ScriptableObject, IInputProvider, AutoPlayerInput.IPlayerActions
    {
        private AutoPlayerInput _gameInput;
        
        public Vector2 MoveDirection { get; set; }

        public event UnityAction OnJumpInitiated = delegate { };
        public event UnityAction OnJumpCanceled = delegate { };
        public event UnityAction OnAttack = delegate { };
        public event UnityAction OnDash = delegate { };
        public event UnityAction OnHook = delegate { };
        public event UnityAction OnGun = delegate { };

        private void OnEnable()
        {
            if (_gameInput == null)
            {
                _gameInput = new AutoPlayerInput();
                _gameInput.Player.SetCallbacks(this);
            }
            _gameInput.Enable();
        }
        
        private void OnDisable()
        {
            _gameInput.Disable();
        }
        
        // --------------- Callbacks from the AutoPlayerInput ------------------

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveDirection = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    OnJumpInitiated?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    OnJumpCanceled?.Invoke();
                    break;
                case InputActionPhase.Disabled:
                    break;
                case InputActionPhase.Waiting:
                    break;
                case InputActionPhase.Started:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void AutoPlayerInput.IPlayerActions.OnAttack(InputAction.CallbackContext context)
        {
            if(context.phase == InputActionPhase.Performed)
                OnAttack.Invoke();
        }

        void AutoPlayerInput.IPlayerActions.OnDash(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                OnDash.Invoke();
            }
        }

        void AutoPlayerInput.IPlayerActions.OnHook(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                OnHook.Invoke();
            }
        }

        void AutoPlayerInput.IPlayerActions.OnGun(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                OnGun.Invoke();
            }
        }
    }
}