using UnityEngine;

namespace UI
{
    /// <summary>
    /// Faz o objeto flutuar (subir e descer) usando uma onda senoidal.
    /// Funciona tanto em RectTransform (UI) quanto em Transform (world).
    /// </summary>
    public class FloatingEffect : MonoBehaviour
    {
        [Tooltip("Amplitude do movimento em pixels/unidades")]
        [SerializeField] private float amplitude = 15f;

        [Tooltip("Velocidade da oscilação")]
        [SerializeField] private float frequency = 2f;

        private Vector3 _startPos;

        void Start()
        {
            _startPos = transform.localPosition;
        }

        void Update()
        {
            float offset = Mathf.Sin(Time.time * frequency) * amplitude;
            transform.localPosition = _startPos + Vector3.up * offset;
        }
    }
}
