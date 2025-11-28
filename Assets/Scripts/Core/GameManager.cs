using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Data;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene Transition")]
        [SerializeField] private GameObject fadeCanvasPrefab;
        [SerializeField] private float fadeDuration = 0.5f;

        private SceneFader sceneFader;

        public DifficultyLevelData CurrentDifficulty { get; private set; }
        public int PlayerLives { get; private set; }
        public int PlayerScore { get; private set; }

        private Stack<string> sceneHistory = new Stack<string>();

        public event Action<DifficultyLevelData> OnDifficultyChanged;
        public event Action<int> OnLivesChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

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

                // Inicializa o ServiceLocator / GameContext
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
            if (sceneFader != null && sceneFader.Alpha > 0.01f)
                StartCoroutine(sceneFader.FadeIn(fadeDuration));
        }

        public void SetDifficulty(DifficultyLevelData newDifficulty)
        {
            CurrentDifficulty = newDifficulty;
            OnDifficultyChanged?.Invoke(CurrentDifficulty);
        }

        public void StartGame()
        {
            if (CurrentDifficulty == null)
            {
                Debug.LogError("Nenhuma dificuldade foi selecionada antes de iniciar o jogo!");
                return;
            }

            PlayerLives = CurrentDifficulty.startingLives;
            PlayerScore = 0;
            OnLivesChanged?.Invoke(PlayerLives);

            LoadScene("2_LabScene");
        }

        public void LoseLife()
        {
            if (PlayerLives > 0) PlayerLives--;
            OnLivesChanged?.Invoke(PlayerLives);

            if (PlayerLives <= 0)
            {
                Debug.Log("Game Over!");
            }
        }

        public void AddScore(int points)
        {
            PlayerScore += (int)(points * CurrentDifficulty.scoreMultiplier);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneWithFade(sceneName));
        }

        private IEnumerator LoadSceneWithFade(string sceneName)
        {
            if (sceneFader != null)
                yield return StartCoroutine(sceneFader.FadeOut(fadeDuration));

            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(currentScene))
                sceneHistory.Push(currentScene);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone) yield return null;
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
            while (!asyncLoad.isDone) yield return null;
        }
    }
}
