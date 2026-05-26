using UnityEngine;
using Presentation.Common;

namespace Presentation.Lab
{
    /// <summary>
    /// Sub-view POCO para painis sobrepostos do laboratorio.
    /// Centraliza operações de <see cref="OverlayAnimator"/> para evitar repetição
    /// no LabUIController (HideAllPanels, ResetFlowState etc).
    /// </summary>
    public class LabPanelsView
    {
        private readonly GameObject _solutionAnim;
        private readonly GameObject[] _overlayPanels;

        public LabPanelsView(GameObject solutionAnimPanel, params GameObject[] overlayPanels)
        {
            _solutionAnim = solutionAnimPanel;
            _overlayPanels = overlayPanels ?? new GameObject[0];
        }

        /// <summary>
        /// Esconde todos os overlays imediatamente (sem animar).
        /// O solutionAnimationPanel e mantido ativo (animação propria).
        /// </summary>
        public void HideAll()
        {
            if (_solutionAnim != null) _solutionAnim.SetActive(true);
            for (int i = 0; i < _overlayPanels.Length; i++)
                OverlayAnimator.HideImmediate(_overlayPanels[i]);
        }
    }
}
