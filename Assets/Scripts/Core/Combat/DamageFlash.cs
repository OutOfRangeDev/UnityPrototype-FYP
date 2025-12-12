using System.Collections;
using UnityEngine;

namespace Core.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageFlash : MonoBehaviour
    {
        [SerializeField] private Health healthComponent;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private int numberOfFlashes = 2;
        
        private SpriteRenderer _spriteRenderer;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (healthComponent != null) healthComponent.OnTakeDamage += TriggerFlash;
        }

        private void OnDisable()
        {
            if (healthComponent != null) healthComponent.OnTakeDamage -= TriggerFlash;
        }

        private void TriggerFlash()
        {
            if(_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            // It can be done through material or swapping color tint.
            // Here we do color tint swapping for simplicity.
            // Can also play with visibility.
            
            for (int i = 0; i < numberOfFlashes; i++)
            {
                _spriteRenderer.color = flashColor;
                yield return new WaitForSeconds(flashDuration);
                _spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(flashDuration);
            }
        }
    }
}