using System.Collections;
using UnityEngine;
using TMPro;
using Data;
using Core;
using Presentation.Lab;
using Domain;

namespace LabScripts
{
    public class LabUIController : MonoBehaviour
    {
        private const string LabSceneName = "2_LabScene";

        [Header("Painéis principais")]
        public GameObject solutionAnimationPanel;
        public GameObject confirmationPanel;
        public GameObject questionPanel;
        public GameObject historyPanel;
        public GameObject treePanel;
        public GameObject pauseMenuPanel;
        public GameObject questionErrorPanel;
        public GameObject questionVictoryPanel;
        public GameObject defeatPanel;

        [Header("Textos de UI")]
        public TextMeshProUGUI confirmationPanelText;
        public TextMeshProUGUI solutionAnimationText;
        public TextMeshProUGUI[] questionAlternativeTexts;
        public TextMeshProUGUI victoryCompoundText;

        [HideInInspector]
        public string currentItemName;

        [Header("HUD")]
        public TextMeshProUGUI difficultyText;
        public TextMeshProUGUI livesText;
        public TextMeshProUGUI modeText;

        [Header("Integração com lógica da fase")]
        [SerializeField] private TestManager testManager;

        [Header("Integração histórico")]
        [SerializeField] private HistoryPanelController historyPanelController;

        [Header("Integração fluxo de perguntas")]
        [SerializeField] private QuestionFlowPresenter questionFlowPresenter;

        // ─────────────────────────────────────────────
        // Ciclo de vida
        // ─────────────────────────────────────────────

        void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDifficultyChanged += HandleDifficultyChanged;
                GameManager.Instance.OnLivesChanged += HandleLivesChanged;
                GameManager.Instance.OnGameOver += HandleGameOver;
            }
        }

        void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDifficultyChanged -= HandleDifficultyChanged;
                GameManager.Instance.OnLivesChanged -= HandleLivesChanged;
                GameManager.Instance.OnGameOver -= HandleGameOver;
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

        private void HandleGameOver()
        {
            ShowDefeatPanel();
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
            defeatPanel?.SetActive(false);
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
            if (testManager != null)
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

            // TODO: lógica de repetir visualmente a animação da mistura
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

        public void SetupQuestionPanel(Question question)
        {
            if (questionAlternativeTexts == null || questionAlternativeTexts.Length == 0)
                return;

            for (int i = 0; i < questionAlternativeTexts.Length; i++)
            {
                var label = questionAlternativeTexts[i];
                if (label == null) continue;

                bool hasOption = i < question.Alternatives.Count;
                var parentObj = label.transform.parent != null
                    ? label.transform.parent.gameObject
                    : null;

                if (parentObj != null)
                    parentObj.SetActive(hasOption);

                if (hasOption)
                    label.text = question.Alternatives[i];
            }
        }

        public void ShowQuestionPanel()
        {
            questionPanel?.SetActive(true);
        }

        public void HideQuestionPanel()
        {
            questionPanel?.SetActive(false);
        }

        /// <summary>
        /// Chamado pelos botões de alternativa. O índice vem direto do OnClick do botão.
        /// </summary>
        public void OnQuestionSelect(int optionIndex)
        {
            Debug.Log($"[LabUI] Alternativa escolhida index={optionIndex}");
            HideQuestionPanel();

            if (questionFlowPresenter != null)
            {
                questionFlowPresenter.OnAnswerSelected(optionIndex);
            }
            else
            {
                Debug.LogWarning("[LabUIController] QuestionFlowPresenter não atribuído.");
            }
        }

        /// <summary>
        /// Botão na HUD para abrir a pergunta da rodada atual.
        /// </summary>
        public void OpenQuestionForCurrentCompound()
        {
            if (questionFlowPresenter != null)
            {
                questionFlowPresenter.ShowQuestionForCurrentCompound();
            }
            else
            {
                Debug.LogWarning("[LabUIController] QuestionFlowPresenter não atribuído.");
            }
        }

        // ─────────────────────────────────────────────
        // History Panel
        // ─────────────────────────────────────────────

        public void ShowHistoryPanel()
        {
            historyPanel?.SetActive(true);

            if (historyPanelController != null)
            {
                historyPanelController.RefreshHistory();
            }
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
            questionPanel?.SetActive(true); // volta para as alternativas
        }

        // ─────────────────────────────────────────────
        // Question Victory Panel
        // ─────────────────────────────────────────────

        /// <summary>
        /// Exibe o painel de vitória preenchendo o texto "Composto X".
        /// </summary>
        public void ShowQuestionVictoryPanel(string compoundName)
        {
            if (victoryCompoundText != null)
                victoryCompoundText.text = string.IsNullOrEmpty(compoundName)
                    ? "Composto X:"
                    : $"Composto X:\n {compoundName}";

            questionVictoryPanel?.SetActive(true);
        }

        // fallback caso alguém chame sem nome
        public void ShowQuestionVictoryPanel()
        {
            ShowQuestionVictoryPanel(string.Empty);
        }

        public void HideQuestionVictoryPanel()
        {
            questionVictoryPanel?.SetActive(false);
        }

        /// <summary>
        /// Botão "Próxima fase" no painel de vitória.
        /// </summary>
        public void OnVictoryNextPhase()
        {
            ResetFlowState(resetScore: false); // fecha tudo + reseta vidas/seleções

            if (questionFlowPresenter != null)
                questionFlowPresenter?.PrepareNextCompound(forceNew: true);
            else
                Debug.LogWarning("[LabUIController] QuestionFlowPresenter não atribuído em OnVictoryNextPhase.");
        }



        /// <summary>
        /// Botão "Reiniciar" no painel de vitória.
        /// </summary>
        public void OnVictoryRestart()
        {
            ResetFlowState(resetScore: true);
            RestartLab();
        }


        /// <summary>
        /// Botão "Voltar ao menu" no painel de vitória.
        /// </summary>
        public void OnVictoryReturnToMenu()
        {
            ResetFlowState(resetScore: false);
            ReturnToMainMenu();
        }


        // ─────────────────────────────────────────────
        // Defeat Panel (Derrota)
        // ─────────────────────────────────────────────

        public void ShowDefeatPanel()
        {
            defeatPanel?.SetActive(true);
            Time.timeScale = 0f;
        }

        public void HideDefeatPanel()
        {
            defeatPanel?.SetActive(false);
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Botão "Reiniciar" no painel de derrota.
        /// </summary>
        public void OnDefeatRestart()
        {
            ResetFlowState(resetScore: true);
            RestartLab();
        }


        /// <summary>
        /// Botão "Voltar ao menu" no painel de derrota.
        /// </summary>
        public void OnDefeatReturnToMenu()
        {
            ResetFlowState(resetScore: false);
            ReturnToMainMenu();
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

        // ─────────────────────────────────────────────
        // Utilidades internas
        // ─────────────────────────────────────────────

        private void RestartLab()
        {
            Time.timeScale = 1f;
            GameManager.Instance.LoadScene(LabSceneName);
        }

        private void ResetFlowState(bool resetScore)
        {
            // garante que o jogo não fique pausado por causa do painel de derrota
            Time.timeScale = 1f;

            // fecha “painéis de fluxo” (sem efeitos colaterais tipo reabrir questionPanel)
            if (solutionAnimationPanel) solutionAnimationPanel.SetActive(false);
            if (confirmationPanel) confirmationPanel.SetActive(false);
            if (questionPanel) questionPanel.SetActive(false);
            if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
            if (questionErrorPanel) questionErrorPanel.SetActive(false);
            if (questionVictoryPanel) questionVictoryPanel.SetActive(false);
            if (defeatPanel) defeatPanel.SetActive(false);
            if (historyPanel) historyPanel.SetActive(false);
            if (treePanel) treePanel.SetActive(false);

            // opcional: limpa texto do composto no painel de vitória (evita “sobra” visual)
            if (victoryCompoundText) victoryCompoundText.text = "Composto X:";

            // reseta vidas/score conforme pedido
            GameManager.Instance?.ResetRunState(resetScore);

            // reseta seleção (compound/solvente) da rodada
            testManager?.ResetRoundState(clearCompound: false);
        }

    }
}
