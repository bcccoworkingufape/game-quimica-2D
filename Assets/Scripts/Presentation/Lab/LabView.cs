using Data;
using Domain;

namespace Presentation.Lab
{
    /// <summary>
    /// Contrato da View do laboratório (MVP).
    /// Implementado por <see cref="LabScripts.LabUIController"/>.
    /// O <see cref="LabPresenter"/> depende apenas desta abstração — nunca
    /// referencia tipos do Unity diretamente para tomar decisões.
    /// </summary>
    public interface ILabView
    {
        // Renderização (View "burra" — apenas exibe o que o Presenter mandar)
        void RenderDifficulty(DifficultyLevelData data, string modeLabel);
        void RenderLives(int lives, GameMode mode);
        void RenderProgress(int percentage);
        void SetTreeAvailable(bool available);

        // Comandos de painéis disparados pelo Presenter em resposta a eventos do Model
        void ShowDefeatPanel();
        void HideAllPanels();

        // Reset de estado visual (overlays/textos) sem mexer no Model
        void ResetFlowState(bool resetScore, bool resetLives);
    }
}
