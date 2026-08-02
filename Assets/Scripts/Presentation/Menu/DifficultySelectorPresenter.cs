using System;
using Core;
using Data;

namespace Presentation.Menu
{
    /// <summary>
    /// Presenter dedicado à seleção de dificuldade (MVP).
    ///
    /// Atualmente o <see cref="MenuPresenter"/> concentra esta lógica para evitar
    /// duplicar wiring de cena. Esta classe existe como contrato e ponto de extensão:
    /// caso a tela de seleção seja extraída em um componente próprio, ela já está
    /// pronta para ser usada com <see cref="IDifficultySelectorView"/>.
    /// </summary>
    public class DifficultySelectorPresenter : IDisposable
    {
        private readonly IDifficultySelectorView _view;
        private GameManager _gm;
        private bool _subscribed;

        public DifficultySelectorPresenter(IDifficultySelectorView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Initialize()
        {
            _gm = GameManager.Instance;
            if (_gm == null) return;

            if (!_subscribed)
            {
                _gm.OnDifficultyChanged += HandleDifficultyChanged;
                _subscribed = true;
            }

            if (_gm.CurrentDifficulty != null)
                HandleDifficultyChanged(_gm.CurrentDifficulty);
        }

        public void Dispose()
        {
            if (_gm != null && _subscribed)
            {
                _gm.OnDifficultyChanged -= HandleDifficultyChanged;
                _subscribed = false;
            }
        }

        private void HandleDifficultyChanged(DifficultyLevelData data)
        {
            _view.HighlightSelection(data);
            _view.RenderDifficultyInfo(data);
        }

        public void Select(DifficultyLevelData data)
        {
            if (data == null) return;
            _gm?.SetDifficulty(data);
        }
    }
}
