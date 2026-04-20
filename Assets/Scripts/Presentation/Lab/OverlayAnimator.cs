using System;
using System.Collections;
using UnityEngine;

namespace Presentation.Common
{
    /// <summary>
    /// Generic helper for animating UI overlay panels using LeanTween.
    /// </summary>
    public sealed class OverlayAnimator : MonoBehaviour
    {
        public static OverlayAnimator Instance { get; private set; }

        public static class Config
        {
            public static float Duration = 0.25f;
            public static float OpenScale = 1f;
            public static float ClosedScale = 0.85f;
            public static LeanTweenType ShowEase = LeanTweenType.easeOutBack;
            public static LeanTweenType HideEase = LeanTweenType.easeInBack;
            public static bool AnimateFade = true;
            public static int MaxSimultaneousTweens = 1200;
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

        /// <summary>
        /// Explicitly pre-warms LeanTween.
        /// Call from a persistent singleton in the initial scene.
        /// </summary>
        public static void Warmup()
        {
            EnsureLeanTweenInitialized();
        }

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

            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.blocksRaycasts = true;
                    cg.interactable = true;
                }
            }

            if (Instance != null)
            {
                Instance.StartCoroutine(ShowNextFrame(panel, onComplete, ignoreTimeScale));
            }
            else
            {
                // fallback seguro caso o singleton ainda não exista por algum motivo
                StartShowTween(panel, onComplete, ignoreTimeScale);
            }
        }

        private static IEnumerator ShowNextFrame(
            GameObject panel,
            Action onComplete,
            bool ignoreTimeScale
        )
        {
            yield return null;

            if (panel == null || !panel.activeInHierarchy)
            {
                yield break;
            }

            Canvas.ForceUpdateCanvases();
            StartShowTween(panel, onComplete, ignoreTimeScale);
        }

        private static void StartShowTween(
            GameObject panel,
            Action onComplete,
            bool ignoreTimeScale
        )
        {
            if (panel == null)
            {
                return;
            }

            LeanTween
                .scale(panel, Vector3.one * Config.OpenScale, Config.Duration)
                .setEase(Config.ShowEase)
                .setIgnoreTimeScale(ignoreTimeScale)
                .setOnComplete(() =>
                {
                    if (Config.AnimateFade)
                    {
                        var cg = GetOrAddCanvasGroup(panel);
                        if (cg != null)
                        {
                            cg.alpha = 1f;
                            cg.blocksRaycasts = true;
                            cg.interactable = true;
                        }
                    }

                    onComplete?.Invoke();
                });

            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null)
                {
                    LeanTween
                        .value(panel, 0f, 1f, Config.Duration)
                        .setEase(Config.ShowEase)
                        .setIgnoreTimeScale(ignoreTimeScale)
                        .setOnUpdate((float a) => cg.alpha = a);
                }
            }
        }

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

            if (!panel.activeSelf)
            {
                return;
            }

            CancelTweens(panel);

            if (Config.AnimateFade)
            {
                var cg = GetOrAddCanvasGroup(panel);
                if (cg != null)
                {
                    cg.blocksRaycasts = false;
                    cg.interactable = false;
                }
            }

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
                    cg.blocksRaycasts = true;
                    cg.interactable = true;
                }
            }
        }

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
                    cg.blocksRaycasts = false;
                    cg.interactable = false;
                }
            }

            panel.SetActive(false);
        }

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