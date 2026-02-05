using UnityEngine;
using Presentation.Common;

namespace Presentation.Lab
{
    /// <summary>
    /// Controla a exibição do painel de animação de solução com efeito de escala.
    /// O painel permanece sempre ativo na hierarquia, mas com escala zero quando "fechado".
    /// Isso garante que o Animator do frasco funcione corretamente.
    /// </summary>
    public class SolutionPanelAnimator : MonoBehaviour
    {
        [Header("Configurações de Animação")]
        [Tooltip("Duração da animação de abrir/fechar em segundos")]
        [SerializeField] private float animationDuration = 0.3f;

        [Tooltip("Tipo de easing para a animação")]
        [SerializeField] private UIAnimator.EaseType easeType = UIAnimator.EaseType.EaseOut;

        [Tooltip("Escala quando o painel está 'fechado' (quase zero para manter ativo)")]
        [SerializeField] private float closedScale = 0.001f;

        [Tooltip("Escala quando o painel está aberto")]
        [SerializeField] private float openScale = 1f;

        [Header("Referências")]
        [Tooltip("Transform do painel a ser animado (se vazio, usa este GameObject)")]
        [SerializeField] private Transform panelTransform;

        [Header("Estado")]
        [SerializeField] private bool startClosed = true;

        private bool _isOpen;
        private Coroutine _currentAnimation;

        /// <summary>
        /// Indica se o painel está atualmente aberto (visível).
        /// </summary>
        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (panelTransform == null)
                panelTransform = transform;

            if (startClosed)
            {
                // Começa fechado (escala mínima)
                panelTransform.localScale = Vector3.one * closedScale;
                _isOpen = false;
            }
            else
            {
                panelTransform.localScale = Vector3.one * openScale;
                _isOpen = true;
            }
        }

        /// <summary>
        /// Abre o painel com animação de escala.
        /// </summary>
        public void Open()
        {
            Debug.Log($"[SolutionPanelAnimator] Open() chamado. _isOpen={_isOpen}, panelTransform={panelTransform?.name ?? "NULL"}");

            if (_isOpen)
            {
                Debug.Log("[SolutionPanelAnimator] Painel já está aberto, ignorando.");
                return;
            }

            StopCurrentAnimation();

            Debug.Log($"[SolutionPanelAnimator] Iniciando animação de {closedScale} para {openScale}");

            _currentAnimation = UIAnimator.ScaleTo(
                this,
                panelTransform,
                Vector3.one * closedScale,
                Vector3.one * openScale,
                animationDuration,
                easeType,
                () =>
                {
                    _isOpen = true;
                    Debug.Log("[SolutionPanelAnimator] Animação de abertura concluída!");
                }
            );
        }

        /// <summary>
        /// Fecha o painel com animação de escala.
        /// </summary>
        public void Close()
        {
            if (!_isOpen) return;

            StopCurrentAnimation();

            _currentAnimation = UIAnimator.ScaleTo(
                this,
                panelTransform,
                Vector3.one * openScale,
                Vector3.one * closedScale,
                animationDuration,
                easeType,
                () => _isOpen = false
            );
        }

        /// <summary>
        /// Alterna entre aberto e fechado.
        /// </summary>
        public void Toggle()
        {
            if (_isOpen)
                Close();
            else
                Open();
        }

        /// <summary>
        /// Define o estado imediatamente sem animação.
        /// </summary>
        public void SetStateImmediate(bool open)
        {
            StopCurrentAnimation();

            _isOpen = open;
            panelTransform.localScale = Vector3.one * (open ? openScale : closedScale);
        }

        private void StopCurrentAnimation()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
                _currentAnimation = null;
            }
        }
    }
}
