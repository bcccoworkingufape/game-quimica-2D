using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // logica de "Voltar"
using Data;


namespace Core
{
    public class GameManager : MonoBehaviour
    {
        // --- Singleton ---
        public static GameManager Instance { get; private set; }

        // --- Estados do Jogo ---
        public DifficultyLevelData CurrentDifficulty { get; private set; }
        public int PlayerLives { get; private set; }
        public int PlayerScore { get; private set; }

        // --- Histórico de Navegação (para o botão "Voltar") ---
        private Stack<string> sceneHistory = new Stack<string>();

        private void Awake()
        {
            // Garante que exista apenas uma instância do GameManager
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Não destrói o GameManager ao carregar nova cena
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // --- Métodos de Gerenciamento de Estado ---

        public void SetDifficulty(DifficultyLevelData newDifficulty)
        {
            CurrentDifficulty = newDifficulty;
            Debug.Log("Dificuldade selecionada: " + CurrentDifficulty.difficultyName);
        }

        public void StartGame()
        {
            if (CurrentDifficulty == null)
            {
                Debug.LogError("Nenhuma dificuldade foi selecionada antes de iniciar o jogo!");
                return;
            }

            // Inicializa o estado do jogador com base na dificuldade
            PlayerLives = CurrentDifficulty.startingLives;
            PlayerScore = 0;
            LoadScene("2_LabScene");
        }

        public void LoseLife()
        {
            if (PlayerLives > 0)
            {
                PlayerLives--;
            }

            // Adicionar lógica de derrota aqui se as vidas chegarem a 0
            if (PlayerLives <= 0)
            {
                Debug.Log("Game Over!");
                // TODO: Chamar o painel de derrota
            }
        }

        public void AddScore(int points)
        {
            PlayerScore += (int)(points * CurrentDifficulty.scoreMultiplier);
        }


        // --- Sistema de Navegação ---

        public void LoadScene(string sceneName)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(currentScene))
            {
                sceneHistory.Push(currentScene);
            }

            SceneManager.LoadScene(sceneName);
        }

        public void GoBack()
        {
            if (sceneHistory.Count > 0)
            {
                string previousScene = sceneHistory.Pop();
                SceneManager.LoadScene(previousScene);
            }
            else
            {
                Debug.LogWarning("Não há cenas no histórico para voltar.");
                // TODO: voltar para o menu principal como fallback
                // SceneManager.LoadScene("1_MenuScene"); 
            }
        }
    }
}