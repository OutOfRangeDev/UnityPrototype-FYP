using UnityEngine;

namespace Core.Combat
{
    [CreateAssetMenu(fileName = "New Attack", menuName = "Combat/Attack Definition")]
    public class AttackDefinition : ScriptableObject
    {
        [Header("General")] 
        public string attackName; // Mainly for debugging purposes.
        
        [Header( "Timing (seconds)" )]
        public float startupTime = 0.1f; // Windup (flash white)
        public float activeTime = 0.2f; // Damage window
        public float recoveryTime = 0.2f; // Time before you can move again.
        
        [Header( "Hitbox" )]
        public Vector2 hitboxSize = new Vector2(1.5f, 1.5f);
        public Vector2 hitboxOffset = new Vector2(1f, 0f); // Offset from the center of the player-
        
        [Header( "Damage & Physics" )]
        public int damage = 10;
        public Vector2 targetKnockback = new Vector2(5, 2); // Push the enemy
        public Vector2 selfKnockback = Vector2.zero; // Push the player
    }
}