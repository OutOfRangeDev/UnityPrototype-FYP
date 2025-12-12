using UnityEngine;

namespace Core.Combat
{
    public interface IDamageable
    {
        // Return true if damage was taken successfully
        bool TakeDamage(int amount, Vector2 knockBackForce);
        
        // To check enemy status
        bool IsAlive { get; }
    }
}