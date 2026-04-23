using System.Collections;
using TMPro;
using UnityEngine;
using Presentation.Common;

namespace Presentation.Lab
{
    /// <summary>
    /// Controls the solution animation panel using a scale animation.
    ///
    /// The panel stays active in the hierarchy at all times, using a very small
    /// scale when "closed". This allows child animators to keep working correctly.
    ///
    /// The legend text appears after a delay with a fade-in using LeanTween.
    /// </summary>
    public class SolutionPanelAnimator : MonoBehaviour
    {
        #region Inspector

        [Header("Panel Animation")]
        [Tooltip("Duration of the open/close animation in seconds.")]
        [SerializeField] private float animationDuration = 0.3f;

        [Tooltip("Easing used by the panel scale animation.")]
        [SerializeField] private UIAnimator.EaseType panelEaseType = UIAnimator.EaseType.EaseOut;

        [Tooltip("Scale used when the panel is considered closed.")]
        [SerializeField] private float closedScale = 0.001f;

        [Tooltip("Scale used when the panel is open.")]
        [SerializeField] private float openScale = 1f;

        [Header("References")]
        [Tooltip("Panel transform to animate. If empty, this object's transform is used.")]
        [SerializeField] private Transform panelTransform;

        [Header("Legend Animation")]
        [Tooltip("Legend TextMeshProUGUI shown after the panel opens.")]
        [SerializeField] private TextMeshProUGUI legendText;

        [Tooltip("Delay before the legend starts appearing.")]
        [SerializeField] private float legendDelay = 4f;

        [Tooltip("Duration of the legend fade-in.")]
        [SerializeField] private float legendFadeDuration = 0.6f;

        [Tooltip("LeanTween easing used in the legend fade.")]
        [SerializeField] private LeanTweenType legendFadeEase = LeanTweenType.easeOutQuad;

        [Header("Initial State")]
        [Tooltip("Defines whether the panel starts closed.")]
        [SerializeField] private bool startClosed = true;

        #endregion

        #region State

        private bool _isOpen;
        private Coroutine _panelAnimationCoroutine;
        private Coroutine _legendDelayCoroutine;
        private int _legendTweenId = -1;

        public bool IsOpen => _isOpen;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (panelTransform == null)
            {
                panelTransform = transform;
            }

            ApplyInitialState();
        }

        private void OnDisable()
        {
            StopPanelAnimation();
            CancelLegendAnimation();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Opens the panel with scale animation and starts the delayed legend fade-in.
        /// </summary>
        public void Open()
        {
            if (_isOpen)
            {
                return;
            }

            _isOpen = true;

            StopPanelAnimation();

            _panelAnimationCoroutine = UIAnimator.ScaleTo(
                this,
                panelTransform,
                panelTransform.localScale,
                GetOpenScale(),
                animationDuration,
                panelEaseType,
                OnPanelAnimationFinished
            );

            StartLegendFadeIn();
        }

        /// <summary>
        /// Closes the panel with scale animation and hides the legend immediately.
        /// </summary>
        public void Close()
        {
            if (!_isOpen)
            {
                return;
            }

            _isOpen = false;

            StopPanelAnimation();
            CancelLegendAnimation();
            SetLegendAlpha(0f);

            _panelAnimationCoroutine = UIAnimator.ScaleTo(
                this,
                panelTransform,
                panelTransform.localScale,
                GetClosedScale(),
                animationDuration,
                panelEaseType,
                OnPanelAnimationFinished
            );
        }

        /// <summary>
        /// Toggles the panel state between open and closed.
        /// </summary>
        public void Toggle()
        {
            if (_isOpen)
            {
                Close();
                return;
            }

            Open();
        }

        /// <summary>
        /// Sets the panel state instantly, without animation.
        /// </summary>
        public void SetStateImmediate(bool open)
        {
            StopPanelAnimation();
            CancelLegendAnimation();

            _isOpen = open;
            panelTransform.localScale = open ? GetOpenScale() : GetClosedScale();
            SetLegendAlpha(open ? 1f : 0f);
        }

        #endregion

        #region Panel Animation

        private void ApplyInitialState()
        {
            _isOpen = !startClosed;
            panelTransform.localScale = _isOpen ? GetOpenScale() : GetClosedScale();
            SetLegendAlpha(_isOpen ? 1f : 0f);
        }

        private void StopPanelAnimation()
        {
            if (_panelAnimationCoroutine == null)
            {
                return;
            }

            StopCoroutine(_panelAnimationCoroutine);
            _panelAnimationCoroutine = null;
        }

        private void OnPanelAnimationFinished()
        {
            _panelAnimationCoroutine = null;
        }

        private Vector3 GetOpenScale()
        {
            return Vector3.one * openScale;
        }

        private Vector3 GetClosedScale()
        {
            return Vector3.one * closedScale;
        }

        #endregion

        #region Legend Animation

        private void StartLegendFadeIn()
        {
            if (legendText == null)
            {
                return;
            }

            CancelLegendAnimation();
            SetLegendAlpha(0f);

            _legendDelayCoroutine = StartCoroutine(DelayedLegendFadeIn());
        }

        private IEnumerator DelayedLegendFadeIn()
        {
            yield return new WaitForSeconds(legendDelay);

            _legendTweenId = LeanTween
                .value(legendText.gameObject, 0f, 1f, legendFadeDuration)
                .setEase(legendFadeEase)
                .setOnUpdate((float alpha) => SetLegendAlpha(alpha))
                .uniqueId;

            _legendDelayCoroutine = null;
        }

        private void CancelLegendAnimation()
        {
            if (_legendDelayCoroutine != null)
            {
                StopCoroutine(_legendDelayCoroutine);
                _legendDelayCoroutine = null;
            }

            if (_legendTweenId != -1)
            {
                LeanTween.cancel(_legendTweenId);
                _legendTweenId = -1;
            }
        }

        private void SetLegendAlpha(float alpha)
        {
            if (legendText == null)
            {
                return;
            }

            Color color = legendText.color;
            color.a = alpha;
            legendText.color = color;
        }

        #endregion
    }
}