using System;
using UnityEngine;
using Core;
using Core.Audio;
using Data;

namespace Presentation.Menu
{
    /// <summary>
    /// Presenter do menu (MVP). Classe POCO instanciada pela View.
    ///
    /// Responsabilidades:
    /// - Observar o Model (<see cref="GameManager"/>) e traduzir mudanças
    ///   de dificuldade em comandos para a <see cref="IMenuView"/>.
    /// - Centralizar a lógica de seleção/sincronização de dificuldade
    ///   e disparo do "Jogar".
    /// </summary>
    public class MenuPresenter : IDisposable
    {
        private readonly IMenuView _view;
        private readonly IAudioService _audio;
        private GameManager _gm;
        private bool _subscribed;

        private DifficultyLevelData _easy;
        private DifficultyLevelData _medium;
        private DifficultyLevelData _hard;

        /// <summary>
        /// A View e quem cria este Presenter, entao ela pode fornecer as
        /// dependencias no momento da construcao. O audio deixou de ser buscado
        /// num registro global: chega pela assinatura, e visivel a quem le a classe.
        /// </summary>
        public MenuPresenter(IMenuView view, IAudioService audio)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _audio = audio;
        }

        public void Initialize(
            DifficultyLevelData easy,
            DifficultyLevelData medium,
            DifficultyLevelData hard)
        {
            _easy = easy;
            _medium = medium;
            _hard = hard;

            _gm = GameManager.Instance;
            if (_gm == null)
            {
                Debug.LogError("[MenuPresenter] GameManager.Instance é nulo. Verifique a cena inicial.");
                return;
            }

            if (!_subscribed)
            {
                _gm.OnDifficultyChanged += HandleDifficultyChanged;
                _subscribed = true;
            }

            // Respeita a dificuldade já escolhida (ex: voltou do Lab); senão, easy.
            var current = _gm.CurrentDifficulty != null ? _gm.CurrentDifficulty : _easy;
            if (_gm.CurrentDifficulty == null && current != null)
                _gm.SetDifficulty(current);

            _view.ApplySelectionVisuals(current);
            _view.RenderDifficulty(current);
            _view.RefreshMusicToggleVisual();
            _view.RefreshSfxToggleVisual();
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
            _view.ApplySelectionVisuals(data);
            _view.RenderDifficulty(data);
        }

        // ─────────────────────────────────────────────
        // Comandos disparados pela View
        // ─────────────────────────────────────────────

        public void SelectEasy()   => SelectDifficulty(_easy);
        public void SelectMedium() => SelectDifficulty(_medium);
        public void SelectHard()   => SelectDifficulty(_hard);

        public void SelectDifficulty(DifficultyLevelData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[MenuPresenter] DifficultyLevelData nulo em SelectDifficulty().");
                return;
            }

            _view.ApplySelectionVisuals(data);

            if (_gm != null)
                _gm.SetDifficulty(data);

            _view.RenderDifficulty(data);
        }

        public void StartGame()
        {
            if (_gm == null) return;

            // A guarda de nulo e preservada: em C o lookup podia devolver null e
            // o fade era pulado. Aqui _audio vem da View e nao e nulo, entao a
            // guarda fica inerte — mas mante-la conserva a forma defensiva do
            // Estado C sem alterar nenhum caminho executavel.
            if (_audio != null && _audio.IsMusicEnabled)
                _audio.FadeMusicTo(0.6f, 0.25f);

            _gm.StartGame();
        }
    }
}
