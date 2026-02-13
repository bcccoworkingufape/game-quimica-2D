using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Data;

using Domain;
using LabScripts;
using Presentation.Lab;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        private const string LabSceneName = "2_LabScene";
        private const string MenuSceneName = "1_MenuScene";
        private const string gameClearSceneName = "3_Win_Game";

        public static GameManager Instance { get; private set; }

        [Header("Scene Transition")]
        [SerializeField] private GameObject fadeCanvasPrefab;
        [SerializeField] private float fadeDuration = 0.2f;

        private SceneFader sceneFader;

        // Estado do jogo
        [SerializeField] private DifficultyLevelData defaultDifficulty;

        public DifficultyLevelData CurrentDifficulty { get; private set; }
        public int PlayerLives { get; private set; }
        public int PlayerScore { get; private set; }

        private readonly Stack<string> sceneHistory = new Stack<string>();

        // Eventos para UI / outros sistemas
        public event Action<DifficultyLevelData> OnDifficultyChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int> OnScoreChanged;
        public event Action OnGameStarted;
        public event Action OnGameOver;

        // ─────────────────────────────────────────────
        // Estado do "run" de perguntas (precisa sobreviver a Restart do Lab)
        // ─────────────────────────────────────────────
        private readonly HashSet<int> _completedQuestionIds = new HashSet<int>();

        public int TotalQuestionsInRun { get; private set; } = 0;

        public int ActiveQuestionId { get; private set; } = 0;

        // true quando acertou a questão ativa, mas ainda NÃO avançou (Próxima fase)
        public bool ActiveQuestionAnsweredCorrect { get; private set; } = false;

        private string _lastSceneBeforeLoad = null;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Inicializa estado do run (novo play)
                ResetQuestionRun();

                // Inicializa fade
                if (fadeCanvasPrefab != null)
                {
                    GameObject fadeCanvasInstance = Instantiate(fadeCanvasPrefab);
                    sceneFader = fadeCanvasInstance.GetComponent<SceneFader>();
                    DontDestroyOnLoad(fadeCanvasInstance);

                    sceneFader.SetInstantVisible();
                    StartCoroutine(sceneFader.FadeIn(fadeDuration));
                }
                else
                {
                    Debug.LogError("FadeCanvas Prefab não foi atribuído no GameManager!");
                }

                // pode-se chamar Bootstrapper / GameContext
                // GameContext.Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
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
            // Fade in
            if (sceneFader != null && sceneFader.Alpha > 0.01f)
                StartCoroutine(sceneFader.FadeIn(fadeDuration));

            // Fallback para cenas rodando direto (ex: abrir 2_LabScene e apertar Play)
            EnsureDefaultDifficulty();
            EnsurePlayerStateInitialized();

            // Se voltou pro MENU, o próximo "Jogar" deve começar do zero (todas as questões)
            if (scene.name == MenuSceneName)
            {
                ResetQuestionRun();
                Time.timeScale = 1f;
            }

            // Ao entrar no LAB: reset visual/rodada/histórico (mas NÃO reseta o run de questões)
            if (scene.name == LabSceneName)
                ResetLabSceneState();

            // Força UI sincronizar
            OnDifficultyChanged?.Invoke(CurrentDifficulty);
            OnLivesChanged?.Invoke(PlayerLives);
            OnScoreChanged?.Invoke(PlayerScore);
        }

        private void ResetLabSceneState()
        {
            // 1) nunca “vazar” pause/gameover pra dentro do lab
            Time.timeScale = 1f;

            // 2) fecha painéis (se algum ficou ativo por qualquer motivo)
            var ui = FindFirstObjectByType<LabUIController>();
            ui?.HideAllPanels();

            // 3) limpa seleção de solvente / estado da rodada (não mexe no compoundId por padrão)
            var testManager = FindFirstObjectByType<TestManager>();
            testManager?.ResetRoundState(clearCompound: false);

            // 4) limpa histórico ao entrar no lab
            try
            {
                var history = ServiceLocator.Resolve<IHistoryService>();
                history?.Clear();
            }
            catch { /* ignore */ }
        }

        // ─────────────────────────────────────────────
        // Run de Perguntas (persistente entre Restart do Lab)
        // ─────────────────────────────────────────────

        public void SetTotalQuestions(int total)
        {
            TotalQuestionsInRun = Mathf.Max(0, total);
        }

        public bool IsQuestionCompleted(int questionId) => _completedQuestionIds.Contains(questionId);

        public void SetActiveQuestion(int questionId)
        {
            ActiveQuestionId = questionId;
        }

        public void MarkActiveQuestionCorrect()
        {
            ActiveQuestionAnsweredCorrect = true;
        }

        /// <summary>
        /// Chamado quando o jogador clica "Próxima fase" (aqui sim a questão é considerada concluída no run).
        /// </summary>
        public void CommitActiveQuestionAsCompleted()
        {
            if (ActiveQuestionId > 0)
                _completedQuestionIds.Add(ActiveQuestionId);

            // limpa a questão ativa para a próxima fase realmente escolher outra
            ActiveQuestionId = 0;

            ActiveQuestionAnsweredCorrect = false;

            TryHandleGameCleared();
        }


        public void ResetQuestionRun()
        {
            _completedQuestionIds.Clear();
            TotalQuestionsInRun = 0;
            ActiveQuestionId = 0;
            ActiveQuestionAnsweredCorrect = false;
        }

        private void TryHandleGameCleared()
        {
            if (TotalQuestionsInRun <= 0) return;

            if (_completedQuestionIds.Count < TotalQuestionsInRun)
                return;

            // Não precisa estar com "vidas cheias", só não pode ter dado game over.
            // Em Experimentos, não existe game over, então também pode zerar.
            if (CurrentDifficulty != null && CurrentDifficulty.mode != GameMode.Experimentos)
            {
                if (PlayerLives <= 0) return;
            }

            HandleGameCleared();
        }

        public void HandleGameCleared()
        {
            Debug.Log("[GameManager] Zerou o game!");
            Time.timeScale = 1f;

            if (!string.IsNullOrWhiteSpace(gameClearSceneName))
                LoadScene(gameClearSceneName);
            else
                Debug.LogWarning("[GameManager] gameClearSceneName não configurado.");
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

            // Novo "Jogar" (menu → lab) começa do zero nas questões
            ResetQuestionRun();

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

            LoadScene(LabSceneName);
        }

        public void LoseLife()
        {
            if (CurrentDifficulty != null && CurrentDifficulty.mode == GameMode.Experimentos)
            {
                // Sem penalidade
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
            if (CurrentDifficulty == null)
            {
                PlayerScore += points;
            }
            else
            {
                PlayerScore += (int)(points * CurrentDifficulty.scoreMultiplier);
            }

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
            if (SceneManager.GetActiveScene().name != LabSceneName) return;
            if (CurrentDifficulty == null) return;

            if (PlayerLives <= 0)
                PlayerLives = Mathf.Max(1, CurrentDifficulty.startingLives);

            if (PlayerScore < 0)
                PlayerScore = 0;
        }

        // Reseta o estado de gameplay mantendo a dificuldade/modo atual.
        // Use resetScore=true para "Reiniciar" e false para "Próxima fase" (mantém score).
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
        // Controle de cenas + fade + histórico
        // ─────────────────────────────────────────────

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneWithFade(sceneName));
        }

        private IEnumerator LoadSceneWithFade(string sceneName)
        {
            if (sceneFader != null)
                yield return StartCoroutine(sceneFader.FadeOut(fadeDuration));

            _lastSceneBeforeLoad = SceneManager.GetActiveScene().name;

            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(currentScene))
                sceneHistory.Push(currentScene);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
                yield return null;
            // Fade-in será chamado em OnSceneLoaded
        }

        public void GoBack()
        {
            StartCoroutine(GoBackWithFade());
        }

        private IEnumerator GoBackWithFade()
        {
            if (sceneHistory.Count == 0)
            {
                Debug.LogWarning("Não há cenas no histórico para voltar.");
                yield break;
            }

            if (sceneFader != null)
                yield return StartCoroutine(sceneFader.FadeOut(fadeDuration));

            string previousScene = sceneHistory.Pop();
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(previousScene);
            while (!asyncLoad.isDone)
                yield return null;
            // Fade-in será chamado em OnSceneLoaded
        }
    }
}
