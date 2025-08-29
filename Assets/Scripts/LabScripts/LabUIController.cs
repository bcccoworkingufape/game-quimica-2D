using System.Collections;
using UnityEngine;
using TMPro;
using Data;
using Core;

namespace LabScripts
{
    public class UIController : MonoBehaviour
    {
        public GameObject solutionAnimationPanel;
        public GameObject confirmationPanel;
        public GameObject questionPanel;
        public GameObject historyPanel;
        public GameObject treePanel;
        public GameObject pauseMenuPanel;
        public GameObject questionErrorPanel;
        public GameObject questionVictoryPanel;
        public TextMeshProUGUI confirmationPanelText;
        public TextMeshProUGUI solutionAnimationText;

        [HideInInspector]
        public string currentItemName;

        [Header("HUD")]
        public TextMeshProUGUI difficultyText;
        public TextMeshProUGUI livesText;

        // Hide the confirmation panel at the start
        void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDifficultyChanged += HandleDifficultyChanged;
                GameManager.Instance.OnLivesChanged += HandleLivesChanged;
            }
        }

        void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDifficultyChanged -= HandleDifficultyChanged;
                GameManager.Instance.OnLivesChanged -= HandleLivesChanged;
            }
        }

        void Start()
        {
            HideAllPanels();

            var gm = GameManager.Instance;
            if (gm != null && gm.CurrentDifficulty != null)
            {
                UpdateDifficultyLabel(gm.CurrentDifficulty);
                UpdateLivesLabel(gm.PlayerLives);
            }
        }

        private void HandleDifficultyChanged(DifficultyLevelData data) => UpdateDifficultyLabel(data);
        private void HandleLivesChanged(int lives) => UpdateLivesLabel(lives);

        private void UpdateDifficultyLabel(DifficultyLevelData data)
        {
            if (difficultyText == null || data == null) return;
            difficultyText.text = $"{data.difficultyName}";
        }

        private void UpdateLivesLabel(int lives)
        {
            if (livesText == null) return;
            livesText.text = $"Vidas: {lives}";
        }

        public void HideAllPanels()
        {
            solutionAnimationPanel?.SetActive(false);
            confirmationPanel?.SetActive(false);
            questionPanel?.SetActive(false);
            historyPanel?.SetActive(false);
            pauseMenuPanel?.SetActive(false);

        }

        // Confirmation Panel
        public void ShowConfirmationPanel(string itemName)
        {
            currentItemName = itemName;
            confirmationPanelText.text = "Iniciar mistura de solubilidade com " + itemName + "?";

            confirmationPanel?.SetActive(true);
        }

        public void HideConfirmationPanel()
        {
            confirmationPanel?.SetActive(false);
        }

        public void OnConfirmAction()
        {
            Debug.Log("Ação Confirmada para o item: " + currentItemName);
            ShowSolutionAnimationPanel();
        }

        public void OnCancelAction()
        {
            Debug.Log("Ação Cancelada para o item: " + currentItemName);
            HideConfirmationPanel(); // Oculta o painel após o cancelamento
        }

        // Solution Animation Panel
        public void ShowSolutionAnimationPanel()
        {
            solutionAnimationPanel?.SetActive(true);

            HideConfirmationPanel();
        }

        public void HideSolutionAnimationPanel()
        {
            solutionAnimationPanel?.SetActive(false);
        }

        // Question Panel
        public void ShowQuestionPanel()
        {
            questionPanel?.SetActive(true);
        }

        public void HideQuestionPanel()
        {
            questionPanel?.SetActive(false);
        }

        public void OnQuestionSelect(string answer)
        {
            Debug.Log("Resposta selecionada: " + answer);
            HideQuestionPanel();
        }

        // History Panel
        public void ShowHistoryPanel()
        {
            historyPanel?.SetActive(true);
        }

        public void HideHistoryPanel()
        {
            historyPanel?.SetActive(false);
        }

        // Tree Panel
        public void ShowTreePanel()
        {
            treePanel?.SetActive(true);
        }

        public void HideTreePanel()
        {
            treePanel?.SetActive(false);
        }


        // Question Error Panel
        public void ShowQuestionErrorPanel()
        {
            questionErrorPanel?.SetActive(true);
        }

        public void HideQuestionErrorPanel()
        {
            questionErrorPanel?.SetActive(false);
            questionPanel?.SetActive(true);
        }

        // Question Victory Panel
        public void ShowQuestionVictoryPanel()
        {
            questionVictoryPanel?.SetActive(true);
        }

        public void HideQuestionVictoryPanel()
        {
            questionVictoryPanel?.SetActive(false);
        }

        // --- MÉTODOS DO MENU DE PAUSA ---

        /// <summary>
        /// Este método é chamado pelo botão de pausa (||) na tela do laboratório.
        /// </summary>
        public void PauseGame()
        {
            pauseMenuPanel?.SetActive(true);
            // Pausa o tempo do jogo
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Este método é chamado pelo botão "Retomar" dentro do painel de pausa.
        /// </summary>
        public void ResumeGame()
        {
            pauseMenuPanel?.SetActive(false);
            // Volta o tempo do jogo ao normal
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Este método é chamado pelo botão "Voltar ao Menu Principal" no painel de pausa.
        /// </summary>
        public void ReturnToMainMenu()
        {
            // IMPORTANTE: Sempre restaure o Time.timeScale antes de mudar de cena
            Time.timeScale = 1f;
            GameManager.Instance.LoadScene("1_MenuScene");
        }
    }
}