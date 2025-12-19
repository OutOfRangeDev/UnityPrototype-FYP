using TMPro;
using UnityEngine;

namespace Core.UI
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float lifeTime = 2f;
        [SerializeField] private float moveYSpeed = 2f;
        [SerializeField] private Vector2 randomOffset = new Vector2(0.5f, 0.5f);
        
        private float _textColorAlpha;

        private void Awake()
        {
            if(text == null) text = GetComponent<TMP_Text>();
        }

        public void SetUp(int damageAmount)
        {
            text.text = damageAmount.ToString();
            _textColorAlpha = text.color.a;
            
            transform.position += new Vector3(
                Random.Range(-randomOffset.x, randomOffset.x), 
                Random.Range(-randomOffset.y, randomOffset.y), 
                0f);
        }

        private void Update()
        {
            //1. Move up.
            transform.position += new Vector3(0f, moveYSpeed * Time.deltaTime, 0f);
            
            //2. Fade out.
            _textColorAlpha -= Time.deltaTime * lifeTime;
            text.color = new Color(text.color.r, text.color.g, text.color.b, _textColorAlpha);

            if (_textColorAlpha <= 0f) Destroy(gameObject);
        }
    }
}