using System;
using System.Collections.Generic;
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
    public static class OverlayAnimator
    {
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
        }

        // ─────────────────────────────────────────────────────────────────────
        // Internal state  (panel → active tween ids)
        // ─────────────────────────────────────────────────────────────────────

        private static readonly Dictionary<int, List<int>> s_ActiveTweens =
            new Dictionary<int, List<int>>();

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

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
        public static void Show(GameObject panel, Action onComplete = null, bool ignoreTimeScale = false)
        {
            if (panel == null) return;

            CancelTweens(panel);

            panel.SetActive(true);

            var rt = panel.GetComponent<RectTransform>() ?? panel.transform as RectTransform;
            if (rt != null)
            {
                rt.localScale = Vector3.one * Config.ClosedScale;
            }

            var ids = GetOrCreateList(panel);

            // --- Scale tween ---
            int scaleId = LeanTween
                .scale(panel, Vector3.one * Config.OpenScale, Config.Duration)
                .setEase(Config.ShowEase)
                .setIgnoreTimeScale(ignoreTimeScale)
                .setOnComplete(() => onComplete?.Invoke())
                .uniqueId;

            ids.Add(scaleId);

            // --- Fade tween (optional) ---
            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null)
                {
                    cg.alpha = 0f;

                    int fadeId = LeanTween
                        .value(panel, 0f, 1f, Config.Duration)
                        .setEase(Config.ShowEase)
                        .setIgnoreTimeScale(ignoreTimeScale)
                        .setOnUpdate((float a) => cg.alpha = a)
                        .uniqueId;

                    ids.Add(fadeId);
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
        public static void Hide(GameObject panel, Action onComplete = null, bool ignoreTimeScale = false)
        {
            if (panel == null) return;

            // Already inactive — nothing to do.
            if (!panel.activeSelf) return;

            CancelTweens(panel);

            var ids = GetOrCreateList(panel);

            // --- Scale tween ---
            int scaleId = LeanTween
                .scale(panel, Vector3.one * Config.ClosedScale, Config.Duration)
                .setEase(Config.HideEase)
                .setIgnoreTimeScale(ignoreTimeScale)
                .setOnComplete(() =>
                {
                    panel.SetActive(false);
                    onComplete?.Invoke();
                })
                .uniqueId;

            ids.Add(scaleId);

            // --- Fade tween (optional) ---
            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null)
                {
                    int fadeId = LeanTween
                        .value(panel, cg.alpha, 0f, Config.Duration)
                        .setEase(Config.HideEase)
                        .setIgnoreTimeScale(ignoreTimeScale)
                        .setOnUpdate((float a) => cg.alpha = a)
                        .uniqueId;

                    ids.Add(fadeId);
                }
            }
        }

        /// <summary>
        /// Opens <paramref name="panel"/> instantly, with no animation.
        /// Cancels any running tween and resets scale / alpha.
        /// </summary>
        public static void ShowImmediate(GameObject panel)
        {
            if (panel == null) return;

            CancelTweens(panel);
            panel.SetActive(true);
            panel.transform.localScale = Vector3.one * Config.OpenScale;

            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null) cg.alpha = 1f;
            }
        }

        /// <summary>
        /// Closes <paramref name="panel"/> instantly, with no animation.
        /// Cancels any running tween and deactivates the GameObject.
        /// </summary>
        public static void HideImmediate(GameObject panel)
        {
            if (panel == null) return;

            CancelTweens(panel);
            panel.transform.localScale = Vector3.one * Config.ClosedScale;

            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null) cg.alpha = 0f;
            }

            panel.SetActive(false);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Internals
        // ─────────────────────────────────────────────────────────────────────

        private static void CancelTweens(GameObject panel)
        {
            int key = panel.GetInstanceID();

            if (!s_ActiveTweens.TryGetValue(key, out var ids)) return;

            foreach (int id in ids)
            {
                if (LeanTween.isTweening(id))
                    LeanTween.cancel(id);
            }

            ids.Clear();
        }

        private static List<int> GetOrCreateList(GameObject panel)
        {
            int key = panel.GetInstanceID();

            if (!s_ActiveTweens.TryGetValue(key, out var list))
            {
                list = new List<int>(2);
                s_ActiveTweens[key] = list;
            }

            return list;
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
                cg = go.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}