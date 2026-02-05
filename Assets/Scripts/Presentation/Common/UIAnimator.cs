using UnityEngine;
using System;
using System.Collections;

namespace Presentation.Common
{
    /// <summary>
    /// Utilitário para animações simples de UI sem dependências externas.
    /// Usa Coroutines e curvas de easing nativas do Unity.
    /// </summary>
    public class UIAnimator : MonoBehaviour
    {
        /// <summary>
        /// Tipos de easing disponíveis.
        /// </summary>
        public enum EaseType
        {
            Linear,
            EaseInOut,
            EaseIn,
            EaseOut,
            Bounce,
            Elastic
        }

        /// <summary>
        /// Anima a escala de um Transform.
        /// </summary>
        public static Coroutine ScaleTo(
            MonoBehaviour owner,
            Transform target,
            Vector3 from,
            Vector3 to,
            float duration,
            EaseType ease = EaseType.EaseInOut,
            Action onComplete = null)
        {
            return owner.StartCoroutine(ScaleCoroutine(target, from, to, duration, ease, onComplete));
        }

        /// <summary>
        /// Anima a escala de um Transform usando a escala atual como ponto de partida.
        /// </summary>
        public static Coroutine ScaleTo(
            MonoBehaviour owner,
            Transform target,
            Vector3 to,
            float duration,
            EaseType ease = EaseType.EaseInOut,
            Action onComplete = null)
        {
            return owner.StartCoroutine(ScaleCoroutine(target, target.localScale, to, duration, ease, onComplete));
        }

        /// <summary>
        /// Faz um "pop" na escala (escala para cima e volta).
        /// </summary>
        public static Coroutine ScalePop(
            MonoBehaviour owner,
            Transform target,
            float popScale = 1.1f,
            float duration = 0.2f,
            Action onComplete = null)
        {
            return owner.StartCoroutine(ScalePopCoroutine(target, popScale, duration, onComplete));
        }

        private static IEnumerator ScaleCoroutine(
            Transform target,
            Vector3 from,
            Vector3 to,
            float duration,
            EaseType ease,
            Action onComplete)
        {
            if (target == null) yield break;

            target.localScale = from;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; // Usa unscaled para funcionar com Time.timeScale = 0
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = ApplyEasing(t, ease);

                target.localScale = Vector3.LerpUnclamped(from, to, easedT);
                yield return null;
            }

            target.localScale = to;
            onComplete?.Invoke();
        }

        private static IEnumerator ScalePopCoroutine(
            Transform target,
            float popScale,
            float duration,
            Action onComplete)
        {
            if (target == null) yield break;

            Vector3 originalScale = target.localScale;
            Vector3 popTarget = originalScale * popScale;
            float halfDuration = duration / 2f;

            // Escala para cima
            yield return ScaleCoroutine(target, originalScale, popTarget, halfDuration, EaseType.EaseOut, null);

            // Escala de volta
            yield return ScaleCoroutine(target, popTarget, originalScale, halfDuration, EaseType.EaseIn, null);

            onComplete?.Invoke();
        }

        private static float ApplyEasing(float t, EaseType ease)
        {
            return ease switch
            {
                EaseType.Linear => t,
                EaseType.EaseIn => t * t,
                EaseType.EaseOut => 1f - (1f - t) * (1f - t),
                EaseType.EaseInOut => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f,
                EaseType.Bounce => BounceEaseOut(t),
                EaseType.Elastic => ElasticEaseOut(t),
                _ => t
            };
        }

        private static float BounceEaseOut(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
                return n1 * t * t;
            else if (t < 2f / d1)
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            else if (t < 2.5f / d1)
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            else
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }

        private static float ElasticEaseOut(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;

            return t switch
            {
                0f => 0f,
                1f => 1f,
                _ => Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f
            };
        }
    }
}
