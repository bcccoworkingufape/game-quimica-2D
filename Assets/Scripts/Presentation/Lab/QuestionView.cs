using Domain;

namespace Presentation.Lab
{
    /// <summary>
    /// Contrato que o fluxo de questões precisa da View do laboratório.
    ///
    /// Separado de <see cref="ILabView"/> e de <see cref="IMixingView"/> pela
    /// mesma razão: são papéis de cliente distintos, sem interseção de operações.
    /// O <see cref="LabPresenter"/> não usa nenhuma destas cinco, e o fluxo de
    /// mistura não usa nenhuma delas tampouco.
    ///
    /// Implementado por <see cref="LabScripts.LabUIController"/>, que recebe a
    /// referência do orquestrador por composição explícita no seu Awake.
    /// </summary>
    public interface IQuestionView
    {
        /// <summary>
        /// Título do painel de questão. Substitui o acesso direto ao
        /// TextMeshProUGUI da View: quem conhece o componente é a View.
        /// </summary>
        void SetQuestionTitle(string title);

        void SetupQuestionPanel(Question question);
        void ShowQuestionPanel();
        void ShowQuestionVictoryPanel(string correctAnswer);
        void ShowQuestionErrorPanel();
    }
}
