using UnityEngine;
using UnityEngine.Events;

namespace Core.Input
{
    public interface IInputProvider 
    {
        Vector2 MoveDirection { get; }
        event UnityAction OnJumpInitiated;
        event UnityAction OnJumpCanceled;
        event UnityAction OnAttack;
        event UnityAction OnDash;
    }
}

