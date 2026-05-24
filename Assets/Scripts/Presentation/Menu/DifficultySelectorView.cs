using Data;

namespace Presentation.Menu
{
    /// <summary>
    /// Contrato de uma View dedicada à seleção de dificuldade.
    /// Reservado para evolução futura caso a seleção seja extraída do
    /// <see cref="MenuScripts.MenuUIController"/> para um componente próprio.
    /// O <see cref="DifficultySelectorPresenter"/> já implementa o lado do
    /// Presenter sobre este contrato.
    /// </summary>
    public interface IDifficultySelectorView
    {
        void HighlightSelection(DifficultyLevelData data);
        void RenderDifficultyInfo(DifficultyLevelData data);
    }
}
