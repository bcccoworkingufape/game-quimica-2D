using System;
using UnityEngine;

namespace Presentation.Common
{
    /// <summary>
    /// Generic helper for animating UI overlay panels using LeanTween.
    ///
    /// Centralises:
    ///   - Show / Hide with scale + optional CanvasGroup fade
    ///   - Per-panel tween cancellation (no overlap between calls)
    ///   - Automatic SetActive(true) before opening and SetActive(false) after closing
    ///   - Configurable easing, duration, and scale values via <see cref="OverlayAnimator.Config"/>
    ///
    /// Usage (static, no MonoBehaviour required):
    /// <code>
    ///   OverlayAnimator.Show(myPanel);
    ///   OverlayAnimator.Hide(myPanel, onComplete: () => Debug.Log("closed"));
    ///   OverlayAnimator.ShowImmediate(myPanel);
    ///   OverlayAnimator.HideImmediate(myPanel);
    /// </code>
    /// </summary>
    public sealed class OverlayAnimator : MonoBehaviour
    {
        public static OverlayAnimator Instance { get; private set; }

        // ─────────────────────────────────────────────────────────────────────
        // Configuration
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Shared animation settings.  Tweak once, applies to every overlay.
        /// </summary>
        public static class Config
        {
            /// <summary>Duration of every show/hide animation (seconds).</summary>
            public static float Duration = 0.25f;

            /// <summary>Scale the panel punches UP to when opening.</summary>
            public static float OpenScale = 1f;

            /// <summary>Starting scale used when the panel is hidden.</summary>
            public static float ClosedScale = 0.85f;

            /// <summary>LeanTween ease used on Show.</summary>
            public static LeanTweenType ShowEase = LeanTweenType.easeOutBack;

            /// <summary>LeanTween ease used on Hide.</summary>
            public static LeanTweenType HideEase = LeanTweenType.easeInBack;

            /// <summary>
            /// When true, a CanvasGroup is required (or auto-added) and its
            /// alpha is also animated alongside the scale.
            /// </summary>
            public static bool AnimateFade = true;

            /// <summary>
            /// Optional capacity hint for LeanTween. Applied on first use only.
            /// </summary>
            public static int MaxSimultaneousTweens = 1200;

            /// <summary>
            /// Optional sequence capacity hint for LeanTween. Applied on first use only.
            /// </summary>
            public static int MaxSimultaneousSequences = 200;
        }

        private static bool s_LeanTweenInitialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Warmup();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Explicitly pre-warms LeanTween.
        ///
        /// Call this from a persistent scene singleton (for example, a manual
        /// node placed in Menu) to avoid first-use hitch in Lab overlays.
        /// </summary>
        public static void Warmup()
        {
            EnsureLeanTweenInitialized();
        }

        /// <summary>
        /// Animates <paramref name="panel"/> open (scale + fade in).
        /// Activates the GameObject automatically before the animation starts.
        /// </summary>
        /// <param name="panel">Root GameObject of the overlay.</param>
        /// <param name="onComplete">Optional callback invoked when the animation finishes.</param>
        /// <param name="ignoreTimeScale">
        /// Pass <c>true</c> when the game is paused (Time.timeScale == 0) so the
        /// animation still runs — e.g. the pause menu panel.
        /// </param>
        public static void Show(
            GameObject panel,
            Action onComplete = null,
            bool ignoreTimeScale = false
        )
        {
            if (panel == null)
            {
                return;
            }

            Warmup();

            CancelTweens(panel);

            panel.SetActive(true);

            var rt = panel.GetComponent<RectTransform>() ?? panel.transform as RectTransform;
            if (rt != null)
            {
                rt.localScale = Vector3.one * Config.ClosedScale;
            }

            // --- Scale tween ---
            LeanTween
                .scale(panel, Vector3.one * Config.OpenScale, Config.Duration)
                .setEase(Config.ShowEase)
                .setIgnoreTimeScale(ignoreTimeScale)
                .setOnComplete(() => onComplete?.Invoke());

            // --- Fade tween (optional) ---
            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null)
                {
                    cg.alpha = 0f;

                    LeanTween
                        .value(panel, 0f, 1f, Config.Duration)
                        .setEase(Config.ShowEase)
                        .setIgnoreTimeScale(ignoreTimeScale)
                        .setOnUpdate((float a) => cg.alpha = a);
                }
            }
        }

        /// <summary>
        /// Animates <paramref name="panel"/> closed (scale + fade out).
        /// Deactivates the GameObject automatically when the animation finishes.
        /// </summary>
        /// <param name="panel">Root GameObject of the overlay.</param>
        /// <param name="onComplete">Optional callback invoked after deactivation.</param>
        /// <param name="ignoreTimeScale">
        /// Pass <c>true</c> when the game is paused (Time.timeScale == 0) so the
        /// animation still runs — e.g. the pause menu panel.
        /// </param>
        public static void Hide(
            GameObject panel,
            Action onComplete = null,
            bool ignoreTimeScale = false
        )
        {
            if (panel == null)
            {
                return;
            }

            Warmup();

            // Already inactive — nothing to do.
            if (!panel.activeSelf)
            {
                return;
            }

            CancelTweens(panel);

            // --- Scale tween ---
            LeanTween
                .scale(panel, Vector3.one * Config.ClosedScale, Config.Duration)
                .setEase(Config.HideEase)
                .setIgnoreTimeScale(ignoreTimeScale)
                .setOnComplete(() =>
                {
                    if (panel != null)
                    {
                        panel.SetActive(false);
                    }

                    onComplete?.Invoke();
                });

            // --- Fade tween (optional) ---
            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null)
                {
                    LeanTween
                        .value(panel, cg.alpha, 0f, Config.Duration)
                        .setEase(Config.HideEase)
                        .setIgnoreTimeScale(ignoreTimeScale)
                        .setOnUpdate((float a) => cg.alpha = a);
                }
            }
        }

        /// <summary>
        /// Opens <paramref name="panel"/> instantly, with no animation.
        /// Cancels any running tween and resets scale / alpha.
        /// </summary>
        public static void ShowImmediate(GameObject panel)
        {
            if (panel == null)
            {
                return;
            }

            Warmup();

            CancelTweens(panel);
            panel.SetActive(true);
            panel.transform.localScale = Vector3.one * Config.OpenScale;

            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null)
                {
                    cg.alpha = 1f;
                }
            }
        }

        /// <summary>
        /// Closes <paramref name="panel"/> instantly, with no animation.
        /// Cancels any running tween and deactivates the GameObject.
        /// </summary>
        public static void HideImmediate(GameObject panel)
        {
            if (panel == null)
            {
                return;
            }

            Warmup();

            CancelTweens(panel);
            panel.transform.localScale = Vector3.one * Config.ClosedScale;

            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null)
                {
                    cg.alpha = 0f;
                }
            }

            panel.SetActive(false);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Internals
        // ─────────────────────────────────────────────────────────────────────

        private static void CancelTweens(GameObject panel)
        {
            LeanTween.cancel(panel);
        }

        private static void EnsureLeanTweenInitialized()
        {
            if (s_LeanTweenInitialized)
            {
                return;
            }

            LeanTween.init(Config.MaxSimultaneousTweens, Config.MaxSimultaneousSequences);
            s_LeanTweenInitialized = true;
        }

        /// <summary>
        /// Returns an existing CanvasGroup on <paramref name="go"/>, or adds one
        /// if <see cref="Config.AnimateFade"/> is true and none exists.
        /// Returns null when fade animation is disabled.
        /// </summary>
        private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = go.AddComponent<CanvasGroup>();
            }

            return cg;
        }
    }
}
