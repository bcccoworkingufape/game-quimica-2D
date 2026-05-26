using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Data;
using Domain;
using LabScripts;
using Presentation.Lab;
using Core.Audio;

namespace Core
{
    /// <summary>
    /// Facade que coordena o estado global do jogo (dificuldade, vidas, score),
    /// delegando responsabilidades para <see cref="QuestionRunTracker"/> (progresso do run),
    /// <see cref="SceneRouter"/> (carregamento de cenas + histórico) e
    /// <see cref="IGameModeRules"/> (regras específicas do modo).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene Transition")]
        [SerializeField] private GameObject fadeCanvasPrefab;
        [SerializeField] private float fadeDuration = 0.2f;

        [SerializeField] private DifficultyLevelData defaultDifficulty;

        public DifficultyLevelData CurrentDifficulty { get; private set; }
        public int PlayerLives { get; private set; }
        public int PlayerScore { get; private set; }

        private SceneFader _sceneFader;
        private SceneRouter _router;
        private readonly QuestionRunTracker _runTracker = new QuestionRunTracker();

        public event Action<DifficultyLevelData> OnDifficultyChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int> OnScoreChanged;
        public event Action OnGameStarted;
        public event Action OnGameOver;
        public event Action<int> OnProgressChanged;

        public int TotalQuestionsInRun => _runTracker.TotalQuestionsInRun;
        public int ActiveQuestionId => _runTracker.ActiveQuestionId;
        public bool ActiveQuestionAnsweredCorrect => _runTracker.ActiveQuestionAnsweredCorrect;

        private IGameModeRules CurrentRules =>
            CurrentDifficulty != null ? GameModeRulesFactory.For(CurrentDifficulty.mode) : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _runTracker.OnProgressChanged += pct => OnProgressChanged?.Invoke(pct);
            _runTracker.OnRunCleared += TryHandleGameCleared;
            _runTracker.Reset();

            InitializeSceneRouter();
        }

        private void InitializeSceneRouter()
        {
            if (fadeCanvasPrefab == null)
            {
                Debug.LogError("FadeCanvas Prefab não foi atribuído no GameManager!");
                return;
            }

            var fadeCanvasInstance = Instantiate(fadeCanvasPrefab);
            _sceneFader = fadeCanvasInstance.GetComponent<SceneFader>();
            DontDestroyOnLoad(fadeCanvasInstance);

            _sceneFader.SetInstantVisible();
            StartCoroutine(_sceneFader.FadeIn(fadeDuration));

            _router = new SceneRouter(this, _sceneFader, fadeDuration);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _router?.NotifySceneLoaded();

            EnsureDefaultDifficulty();
            EnsurePlayerStateInitialized();

            if (scene.name == SceneNames.Menu)
            {
                _runTracker.Reset();
                Time.timeScale = 1f;
            }

            if (scene.name == SceneNames.Lab)
                ResetLabSceneState();

            OnDifficultyChanged?.Invoke(CurrentDifficulty);
            OnLivesChanged?.Invoke(PlayerLives);
            OnScoreChanged?.Invoke(PlayerScore);
            OnProgressChanged?.Invoke(GetProgressPercentage());
        }

        private void ResetLabSceneState()
        {
            Time.timeScale = 1f;

            var ui = FindAnyObjectByType<LabUIController>();
            ui?.HideAllPanels();

            var mixingRoundController = FindAnyObjectByType<MixingRoundController>();
            mixingRoundController?.ResetRoundState(clearCompound: false);

            try
            {
                var history = ServiceLocator.Resolve<IHistoryService>();
                history?.Clear();
            }
            catch { /* ignore */ }
        }

        // ─────────────────────────────────────────────
        // Run de Perguntas (delegado para QuestionRunTracker)
        // ─────────────────────────────────────────────

        public void SetTotalQuestions(int total) => _runTracker.SetTotal(total);
        public int GetProgressPercentage() => _runTracker.GetProgressPercentage();
        public bool IsQuestionCompleted(int questionId) => _runTracker.IsCompleted(questionId);
        public void SetActiveQuestion(int questionId) => _runTracker.SetActive(questionId);
        public void MarkActiveQuestionCorrect() => _runTracker.MarkActiveCorrect();
        public void CommitActiveQuestionAsCompleted() => _runTracker.Commit();
        public void ResetQuestionRun() => _runTracker.Reset();

        private void TryHandleGameCleared()
        {
            var rules = CurrentRules;
            if (rules != null && rules.AppliesLifePenalty && PlayerLives <= 0)
                return;

            HandleGameCleared();
        }

        public void HandleGameCleared()
        {
            Debug.Log("[GameManager] Zerou o game!");
            Time.timeScale = 1f;

            SfxManager.Instance?.PlayWin();

            if (!string.IsNullOrWhiteSpace(SceneNames.WinGame))
                LoadScene(SceneNames.WinGame);
            else
                Debug.LogWarning("[GameManager] WinGame scene não configurada.");
        }

        // ─────────────────────────────────────────────
        // Dificuldade / estado
        // ─────────────────────────────────────────────

        public void SetDifficulty(DifficultyLevelData newDifficulty)
        {
            CurrentDifficulty = newDifficulty;
            OnDifficultyChanged?.Invoke(CurrentDifficulty);
        }

        public void StartGame()
        {
            EnsureDefaultDifficulty();
            _runTracker.Reset();

            if (CurrentDifficulty == null)
            {
                Debug.LogError("Nenhuma dificuldade foi selecionada antes de iniciar o jogo!");
                return;
            }

            PlayerLives = CurrentDifficulty.startingLives;
            PlayerScore = 0;

            OnLivesChanged?.Invoke(PlayerLives);
            OnScoreChanged?.Invoke(PlayerScore);
            OnGameStarted?.Invoke();

            LoadScene(SceneNames.Lab);
        }

        public void LoseLife()
        {
            var rules = CurrentRules;
            if (rules != null && !rules.AppliesLifePenalty)
            {
                OnLivesChanged?.Invoke(PlayerLives);
                return;
            }

            if (PlayerLives > 0)
                PlayerLives--;

            OnLivesChanged?.Invoke(PlayerLives);

            if (PlayerLives <= 0)
            {
                Debug.Log("Game Over!");
                OnGameOver?.Invoke();
            }
        }

        public void AddScore(int points)
        {
            float multiplier = CurrentDifficulty != null ? CurrentDifficulty.scoreMultiplier : 1f;
            PlayerScore += (int)(points * multiplier);
            OnScoreChanged?.Invoke(PlayerScore);
        }

        private void EnsureDefaultDifficulty()
        {
            if (CurrentDifficulty != null) return;

            if (defaultDifficulty == null)
            {
                Debug.LogWarning("[GameManager] defaultDifficulty NÃO atribuído. Dificuldade ficará nula até alguém chamar SetDifficulty().");
                return;
            }

            SetDifficulty(defaultDifficulty);
            Debug.Log($"[GameManager] Default difficulty aplicada: {defaultDifficulty.difficultyName}");
        }

        private void EnsurePlayerStateInitialized()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Lab) return;
            if (CurrentDifficulty == null) return;

            if (PlayerLives <= 0)
                PlayerLives = Mathf.Max(1, CurrentDifficulty.startingLives);

            if (PlayerScore < 0)
                PlayerScore = 0;
        }

        public void ResetRunState(bool resetScore, bool resetLives)
        {
            EnsureDefaultDifficulty();

            if (resetLives && CurrentDifficulty != null)
                PlayerLives = Mathf.Max(1, CurrentDifficulty.startingLives);

            if (resetScore)
                PlayerScore = 0;

            OnLivesChanged?.Invoke(PlayerLives);
            if (resetScore)
                OnScoreChanged?.Invoke(PlayerScore);
        }

        // ─────────────────────────────────────────────
        // Controle de cenas (delegado para SceneRouter)
        // ─────────────────────────────────────────────

        public void LoadScene(string sceneName) => _router?.LoadScene(sceneName);
        public void GoBack() => _router?.GoBack();
    }
}
