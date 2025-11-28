using System.Collections;
using UnityEngine;
using TMPro;
using Data;
using Core;
using Presentation.Lab;

namespace LabScripts
{
    public class LabUIController : MonoBehaviour
    {
        [Header("Painéis principais")]
        public GameObject solutionAnimationPanel;
        public GameObject confirmationPanel;
        public GameObject questionPanel;
        public GameObject historyPanel;
        public GameObject treePanel;
        public GameObject pauseMenuPanel;
        public GameObject questionErrorPanel;
        public GameObject questionVictoryPanel;

        [Header("Textos de UI")]
        public TextMeshProUGUI confirmationPanelText;
        public TextMeshProUGUI solutionAnimationText;

        [HideInInspector]
        public string currentItemName;

        [Header("HUD")]
        public TextMeshProUGUI difficultyText;
        public TextMeshProUGUI livesText;
        public TextMeshProUGUI modeText;

        [Header("Integração com lógica da fase")]
        [SerializeField] private TestManager testManager;

        // ─────────────────────────────────────────────
        // Ciclo de vida
        // ─────────────────────────────────────────────

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

        // ─────────────────────────────────────────────
        // Handlers de eventos do GameManager
        // ─────────────────────────────────────────────

        private void HandleDifficultyChanged(DifficultyLevelData data)
        {
            UpdateDifficultyLabel(data);
        }

        private void HandleLivesChanged(int lives)
        {
            UpdateLivesLabel(lives);
        }

        private void UpdateDifficultyLabel(DifficultyLevelData data)
        {
            if (data == null) return;

            if (difficultyText != null)
                difficultyText.text = $"{data.difficultyName}";

            if (modeText != null)
                modeText.text = data.ModeLabel;
        }

        private void UpdateLivesLabel(int lives)
        {
            if (livesText == null) return;
            livesText.text = $"Vidas: {lives}";
        }

        // ─────────────────────────────────────────────
        // Controle de painéis
        // ─────────────────────────────────────────────

        public void HideAllPanels()
        {
            solutionAnimationPanel?.SetActive(false);
            confirmationPanel?.SetActive(false);
            questionPanel?.SetActive(false);
            historyPanel?.SetActive(false);
            treePanel?.SetActive(false);
            pauseMenuPanel?.SetActive(false);
            questionErrorPanel?.SetActive(false);
            questionVictoryPanel?.SetActive(false);
        }

        // ─────────────────────────────────────────────
        // Confirmation Panel
        // ─────────────────────────────────────────────

        public void ShowConfirmationPanel(string itemName)
        {
            currentItemName = itemName;

            if (confirmationPanelText != null)
                confirmationPanelText.text = "Iniciar mistura de solubilidade com " + itemName + "?";

            confirmationPanel?.SetActive(true);
        }

        public void HideConfirmationPanel()
        {
            confirmationPanel?.SetActive(false);
        }

        /// <summary>
        /// Botão "Sim" do popup.
        /// </summary>
        public void OnConfirmAction()
        {
            Debug.Log("Ação Confirmada para o item: " + currentItemName);
            HideConfirmationPanel();

            // Dispara a lógica de mistura (consulta banco/cache, histórico, etc)
            if (testManager.name != null)
            {
                testManager.OnConfirmMix();
            }
            else
            {
                Debug.LogWarning("TestManager não atribuído no LabUIController. Exibindo animação mesmo assim.");
                ShowSolutionAnimationPanel();
            }
        }

        /// <summary>
        /// Botão "Não" do popup.
        /// </summary>
        public void OnCancelAction()
        {
            Debug.Log("Ação Cancelada para o item: " + currentItemName);
            HideConfirmationPanel();
        }


        public void OnRepeatMixButton()
        {
            // Fecha o painel de animação
            HideSolutionAnimationPanel();

            // TODO: Adicionar lógica de resetar a animação, e visualmente ver ela ocorrendo novamente
        }


        // ─────────────────────────────────────────────
        // Solution Animation Panel
        // ─────────────────────────────────────────────

        public void ShowSolutionAnimationPanel()
        {
            solutionAnimationPanel?.SetActive(true);
        }

        public void HideSolutionAnimationPanel()
        {
            solutionAnimationPanel?.SetActive(false);
        }

        // ─────────────────────────────────────────────
        // Question Panel
        // ─────────────────────────────────────────────

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

        // ─────────────────────────────────────────────
        // History Panel
        // ─────────────────────────────────────────────

        public void ShowHistoryPanel()
        {
            historyPanel?.SetActive(true);
        }

        public void HideHistoryPanel()
        {
            historyPanel?.SetActive(false);
        }

        // ─────────────────────────────────────────────
        // Tree Panel
        // ─────────────────────────────────────────────

        public void ShowTreePanel()
        {
            treePanel?.SetActive(true);
        }

        public void HideTreePanel()
        {
            treePanel?.SetActive(false);
        }

        // ─────────────────────────────────────────────
        // Question Error Panel
        // ─────────────────────────────────────────────

        public void ShowQuestionErrorPanel()
        {
            questionErrorPanel?.SetActive(true);
        }

        public void HideQuestionErrorPanel()
        {
            questionErrorPanel?.SetActive(false);
            questionPanel?.SetActive(true);
        }

        // ─────────────────────────────────────────────
        // Question Victory Panel
        // ─────────────────────────────────────────────

        public void ShowQuestionVictoryPanel()
        {
            questionVictoryPanel?.SetActive(true);
        }

        public void HideQuestionVictoryPanel()
        {
            questionVictoryPanel?.SetActive(false);
        }

        // ─────────────────────────────────────────────
        // Menu de pausa
        // ─────────────────────────────────────────────

        /// <summary>
        /// Botão de pausa (||) na tela do laboratório.
        /// </summary>
        public void PauseGame()
        {
            pauseMenuPanel?.SetActive(true);
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Botão "Retomar" dentro do painel de pausa.
        /// </summary>
        public void ResumeGame()
        {
            pauseMenuPanel?.SetActive(false);
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Botão "Voltar ao Menu Principal" no painel de pausa.
        /// </summary>
        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            GameManager.Instance.LoadScene("1_MenuScene");
        }
    }
}
