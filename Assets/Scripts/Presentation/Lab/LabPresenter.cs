using System;
using UnityEngine;
using Core;
using Data;
using Domain;

namespace Presentation.Lab
{
    /// <summary>
    /// Presenter da cena do laboratório (MVP).
    ///
    /// Responsabilidades:
    /// - Observar o Model (<see cref="GameManager"/>) e traduzir mudanças de estado
    ///   (dificuldade, vidas, progresso, game over) em comandos para a <see cref="ILabView"/>.
    /// - Coordenar comandos de fluxo de jogo disparados pela View
    ///   (próxima fase, reiniciar, voltar ao menu) consultando o Model e/ou
    ///   o <see cref="QuestionFlowPresenter"/>.
    ///
    /// É uma classe POCO (sem dependência de MonoBehaviour) — instanciada pela View
    /// em Awake e descartada em OnDisable. Isso mantém o Presenter testável e
    /// desacoplado do ciclo de vida do Unity.
    /// </summary>
    public class LabPresenter : IDisposable
    {
        private readonly ILabView _view;

        private GameManager _gm;
        private QuestionFlowPresenter _questionFlow;
        private bool _subscribed;

        public LabPresenter(ILabView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        // ─────────────────────────────────────────────
        // Ciclo de vida
        // ─────────────────────────────────────────────

        public void Initialize(QuestionFlowPresenter questionFlow)
        {
            _questionFlow = questionFlow;
            _gm = GameManager.Instance;

            if (_gm == null)
            {
                Debug.LogWarning("[LabPresenter] GameManager.Instance é nulo em Initialize.");
                return;
            }

            if (!_subscribed)
            {
                _gm.OnDifficultyChanged += HandleDifficultyChanged;
                _gm.OnLivesChanged += HandleLivesChanged;
                _gm.OnGameOver += HandleGameOver;
                _gm.OnProgressChanged += HandleProgressChanged;
                _subscribed = true;
            }

            // Sincroniza View com estado atual do Model
            if (_gm.CurrentDifficulty != null)
                HandleDifficultyChanged(_gm.CurrentDifficulty);

            HandleLivesChanged(_gm.PlayerLives);
            HandleProgressChanged(_gm.GetProgressPercentage());
        }

        public void Dispose()
        {
            if (_gm != null && _subscribed)
            {
                _gm.OnDifficultyChanged -= HandleDifficultyChanged;
                _gm.OnLivesChanged -= HandleLivesChanged;
                _gm.OnGameOver -= HandleGameOver;
                _gm.OnProgressChanged -= HandleProgressChanged;
                _subscribed = false;
            }
        }

        // ─────────────────────────────────────────────
        // Handlers de eventos do Model → comandos para a View
        // ─────────────────────────────────────────────

        private void HandleDifficultyChanged(DifficultyLevelData data)
        {
            if (data == null) return;

            var rules = GameModeRulesFactory.For(data.mode);

            _view.RenderDifficulty(data, data.ModeLabel);
            _view.SetTreeAvailable(rules.AllowsDecisionTree);

            int lives = _gm != null ? _gm.PlayerLives : 0;
            _view.RenderLives(lives, data.mode);
        }

        private void HandleLivesChanged(int lives)
        {
            var mode = CurrentMode;
            _view.RenderLives(lives, mode);
        }

        private void HandleGameOver()
        {
            _view.ShowDefeatPanel();
        }

        private void HandleProgressChanged(int percentage)
        {
            _view.RenderProgress(percentage);
        }

        // ─────────────────────────────────────────────
        // Consultas auxiliares para a View
        // ─────────────────────────────────────────────

        public GameMode CurrentMode =>
            _gm != null && _gm.CurrentDifficulty != null
                ? _gm.CurrentDifficulty.mode
                : GameMode.Estudo_Livre;

        public int CurrentLives => _gm != null ? _gm.PlayerLives : 0;

        public bool IsTreeAllowed => GameModeRulesFactory.For(CurrentMode).AllowsDecisionTree;

        // ─────────────────────────────────────────────
        // Comandos de fluxo (chamados pela View em reação a cliques)
        // ─────────────────────────────────────────────

        public void OnNextPhaseRequested()
        {
            _view.ResetFlowState(resetScore: false, resetLives: false);

            if (_questionFlow != null)
                _questionFlow.PrepareNextCompound(forceNew: true);
            else
                Debug.LogWarning("[LabPresenter] QuestionFlowPresenter ausente em OnNextPhaseRequested.");
        }

        public void OnRestartRequested()
        {
            _view.ResetFlowState(resetScore: true, resetLives: true);
            Time.timeScale = 1f;
            _gm?.LoadScene(SceneNames.Lab);
        }

        public void OnDefeatRestartRequested()
        {
            _gm?.ResetQuestionRun();
            _view.ResetFlowState(resetScore: true, resetLives: true);
            Time.timeScale = 1f;
            _gm?.LoadScene(SceneNames.Lab);
        }

        public void OnReturnToMenuAfterFlowRequested()
        {
            _view.ResetFlowState(resetScore: false, resetLives: true);
            Time.timeScale = 1f;
            _gm?.LoadScene(SceneNames.Menu);
        }

        public void OnReturnToMenuFromPauseRequested()
        {
            Time.timeScale = 1f;
            _gm?.LoadScene(SceneNames.Menu);
        }
    }
}
