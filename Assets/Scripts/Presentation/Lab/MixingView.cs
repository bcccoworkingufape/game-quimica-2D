namespace Presentation.Lab
{
    /// <summary>
    /// Contrato que o fluxo de mistura precisa da View do laboratório.
    ///
    /// É deliberadamente separado de <see cref="ILabView"/>: aquele contrato
    /// serve ao <see cref="LabPresenter"/> (renderização de dificuldade, vidas,
    /// progresso e reset de fluxo) e nenhuma de suas sete operações é usada
    /// aqui. Somar os dois produziria uma interface agregadora com dois papéis
    /// de cliente sem interseção.
    ///
    /// Implementado por <see cref="LabScripts.LabUIController"/>, que recebe a
    /// referência do orquestrador por composição explícita no seu Awake.
    /// </summary>
    public interface IMixingView
    {
        /// <summary>
        /// Painel de confirmação da mistura. A decisão sobre o reagente ser ou
        /// não um indicador vem pronta do orquestrador — a View não reclassifica
        /// química, apenas escolhe o texto.
        /// </summary>
        void ShowConfirmationPanel(string solventName, bool isIndicator);

        /// <summary>
        /// Texto do resultado do experimento. Substitui o acesso direto ao
        /// TextMeshProUGUI da View: quem conhece o componente é a View.
        /// </summary>
        void SetSolutionResultText(string message);

        void ShowSolutionAnimationPanel();
    }
}
