using Data;
using Domain;

namespace Presentation.Menu
{
    /// <summary>
    /// Contrato da View do menu (MVP).
    /// Implementado por <see cref="MenuScripts.MenuUIController"/>.
    /// O <see cref="MenuPresenter"/> opera apenas sobre esta abstração.
    /// </summary>
    public interface IMenuView
    {
        void RenderDifficulty(DifficultyLevelData data);
        void ApplySelectionVisuals(DifficultyLevelData data);
        void ShowLoadingPanel();
        void HideLoadingPanel();
        void RefreshMusicToggleVisual();
        void RefreshSfxToggleVisual();
    }
}
