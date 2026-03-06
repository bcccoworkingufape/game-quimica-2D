using System.Collections;
using UnityEngine;
using TMPro;
using Data;
using Core;
using Presentation.Lab;
using Domain;
using UnityEngine.UI;

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

        [Header("Animação do Painel de Solução")]
        [Tooltip("Controlador de animação de escala do painel de solução (opcional)")]
        [SerializeField] private SolutionPanelAnimator solutionPanelAnimator;

        [Header("Textos de UI")]
        public TextMeshProUGUI confirmationPanelText;
        public TextMeshProUGUI solutionAnimationText;
        public TextMeshProUGUI[] questionAlternativeTexts;
        public TextMeshProUGUI victoryCompoundText;
        public TextMeshProUGUI questionPanelTitle;

        [HideInInspector]
        public string currentItemName;

        [Header("HUD")]
        public TextMeshProUGUI difficultyText;
        public TextMeshProUGUI livesText;
        public TextMeshProUGUI modeText;
        public TextMeshProUGUI percentageText;
        [Header("Botões HUD")]
        [SerializeField] private Button treeButton; //Button do ícone da árvore aqui
        // arraste aqui os Graphics que devem ficar PB (Image do ícone, TMP do texto, etc)
        [SerializeField] private Graphic[] treeButtonGraphicsToTint;
        [SerializeField] private Color treeEnabledColor = Color.white;
        [SerializeField] private Color treeDisabledColor = Color.black;

        [Header("Vidas (corações)")]
        [SerializeField] private GameObject[] heartIcons;

        [Header("Estrelas (painel de vitória)")]
        [SerializeField] private GameObject[] starIcons;

        [Header("Integração com lógica da fase")]
        [SerializeField] private TestManager testManager;

        [Header("Integração histórico")]
        [SerializeField] private HistoryPanelController historyPanelController;

        [Header("Integração fluxo de perguntas")]
        [SerializeField] private QuestionFlowPresenter questionFlowPresenter;

        [Header("Animator Controller")]
        [SerializeField] private Animator animator;

        //[Header("UI")]

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
                GameManager.Instance.OnProgressChanged += HandleProgressChanged;
            }
        }

        void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDifficultyChanged -= HandleDifficultyChanged;
                GameManager.Instance.OnLivesChanged -= HandleLivesChanged;
                GameManager.Instance.OnGameOver -= HandleGameOver;
                GameManager.Instance.OnProgressChanged -= HandleProgressChanged;
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
                RefreshHearts(gm.PlayerLives);
            }

            if (gm != null)
                UpdatePercentageText(gm.GetProgressPercentage());
        }

        // ─────────────────────────────────────────────
        // Handlers de eventos do GameManager
        // ─────────────────────────────────────────────

        private void HandleDifficultyChanged(DifficultyLevelData data)
        {
            UpdateDifficultyLabel(data);
            ApplyModeRules(data);

            int lives = GameManager.Instance != null ? GameManager.Instance.PlayerLives : 0;
            UpdateLivesLabel(lives);
            RefreshHearts(lives);
        }

        private void HandleLivesChanged(int lives)
        {
            UpdateLivesLabel(lives);
            RefreshHearts(lives);
        }

        private void HandleGameOver()
        {
            ShowDefeatPanel();
        }

        private void HandleProgressChanged(int percentage)
        {
            UpdatePercentageText(percentage);
        }

        private void UpdatePercentageText(int percentage)
        {
            if (percentageText != null)
                percentageText.text = $"{percentage}%";
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

            var gm = GameManager.Instance;
            var mode = gm != null && gm.CurrentDifficulty != null
                ? gm.CurrentDifficulty.mode
                : GameMode.Estudo_Livre;

            if (mode == GameMode.Estudo_Livre)
                livesText.text = "Sem penalidade";
            else
                livesText.text = $"Vidas: {lives}";
        }

        private void RefreshHearts(int lives)
        {
            var mode = GetCurrentMode();
            bool nopenalty = mode == GameMode.Estudo_Livre;
            SetIconsActive(heartIcons, nopenalty ? heartIcons.Length : lives);
        }

        private void ApplyModeRules(DifficultyLevelData data)
        {
            if (data == null) return;

            bool treeAllowed = data.mode != GameMode.Desafio;

            // se não pode usar, fecha o painel se estiver aberto
            if (!treeAllowed)
                treePanel?.SetActive(false);

            // mantém o botão visível, só desabilita + PB
            SetTreeButtonEnabled(treeAllowed);
        }


        private void SetTreeButtonEnabled(bool enabled)
        {
            if (treeButton != null)
                treeButton.interactable = enabled;

            var tint = enabled ? treeEnabledColor : treeDisabledColor;

            if (treeButtonGraphicsToTint != null && treeButtonGraphicsToTint.Length > 0)
            {
                foreach (var g in treeButtonGraphicsToTint)
                {
                    if (g != null) g.color = tint;
                }
            }
            else
            {
                // fallback: tenta tingir apenas o targetGraphic do Button
                if (treeButton != null && treeButton.targetGraphic != null)
                    treeButton.targetGraphic.color = tint;
            }
        }

        // ─────────────────────────────────────────────
        // Controle de painéis
        // ─────────────────────────────────────────────

        public void HideAllPanels()
        {
            solutionAnimationPanel?.SetActive(true); // solutionAnimationPanel precisa estar ativo todo momento
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
            string defaultText = "Deseja realizar o teste de solubilidade da substância desconhecida em <b>" + itemName + "</b>?";
            string litmusText = "Deseja adicionar <b>tornassol</b> à substância desconhecida?";

            if (confirmationPanelText != null)
                if (currentItemName == "Tornassol")
                    confirmationPanelText.text = litmusText;
                else
                    confirmationPanelText.text = defaultText;

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
            // HideSolutionAnimationPanel();
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, -1, 0f);

            // TODO: lógica de repetir visualmente a animação da mistura
        }

        // ─────────────────────────────────────────────
        // Solution Animation Panel
        // ─────────────────────────────────────────────

        public void ShowSolutionAnimationPanel()
        {
            // Se temos o SolutionPanelAnimator, usa animação de escala (painel sempre ativo)
            if (solutionPanelAnimator != null)
            {
                solutionPanelAnimator.Open();
                animator.enabled = true;
            }
            else
            {
                // Fallback: ativa/desativa o GameObject (pode causar problemas com Animator)
                solutionAnimationPanel?.SetActive(true);
            }
        }

        public void HideSolutionAnimationPanel()
        {
            // Se temos o SolutionPanelAnimator, usa animação de escala
            if (solutionPanelAnimator != null)
            {
                // Reseta a animação
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                animator.Play(stateInfo.fullPathHash, -1, 0f);
                animator.enabled = false;

                solutionPanelAnimator.Close();
            }
            else
            {
                // Fallback: ativa/desativa o GameObject
                solutionAnimationPanel?.SetActive(true); // true para nao desativar o painel
            }
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
            var gm = GameManager.Instance;
            if (gm != null && gm.CurrentDifficulty != null && gm.CurrentDifficulty.mode == GameMode.Desafio)
                return;

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

            RefreshStars();
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
            SetIconsActive(starIcons, 0);
        }

        /// <summary>
        /// Botão "Próxima fase" no painel de vitória.
        /// </summary>
        public void OnVictoryNextPhase()
        {
            ResetFlowState(resetScore: false, resetLives: false); // fecha tudo + reseta vidas/seleções

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
            ResetFlowState(resetScore: true, resetLives: true);
            RestartLab();
        }


        /// <summary>
        /// Botão "Voltar ao menu" no painel de vitória.
        /// </summary>
        public void OnVictoryReturnToMenu()
        {
            ResetFlowState(resetScore: false, resetLives: true);
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
            GameManager.Instance?.ResetQuestionRun();
            ResetFlowState(resetScore: true, resetLives: true);
            RestartLab();
        }



        /// <summary>
        /// Botão "Voltar ao menu" no painel de derrota.
        /// </summary>
        public void OnDefeatReturnToMenu()
        {
            ResetFlowState(resetScore: false, resetLives: true);
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

        /// <summary>
        /// Botão "Fechar o jogo" (encerra a aplicacao).
        /// </summary>
        public void QuitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ─────────────────────────────────────────────
        // Utilidades internas
        // ─────────────────────────────────────────────

        private void RestartLab()
        {
            Time.timeScale = 1f;
            GameManager.Instance.LoadScene(LabSceneName);
        }

        private void ResetFlowState(bool resetScore, bool resetLives)
        {
            Time.timeScale = 1f;

            if (solutionAnimationPanel) solutionAnimationPanel.SetActive(true);
            if (confirmationPanel) confirmationPanel.SetActive(false);
            if (questionPanel) questionPanel.SetActive(false);
            if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
            if (questionErrorPanel) questionErrorPanel.SetActive(false);
            if (questionVictoryPanel) questionVictoryPanel.SetActive(false);
            if (defeatPanel) defeatPanel.SetActive(false);
            if (historyPanel) historyPanel.SetActive(false);
            if (treePanel) treePanel.SetActive(false);

            if (victoryCompoundText) victoryCompoundText.text = "Composto X:";

            SetIconsActive(starIcons, 0);

            // controla se reseta vidas ou não
            GameManager.Instance?.ResetRunState(resetScore, resetLives);

            testManager?.ResetRoundState(clearCompound: false);
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────

        private GameMode GetCurrentMode()
        {
            var gm = GameManager.Instance;
            return gm != null && gm.CurrentDifficulty != null
                ? gm.CurrentDifficulty.mode
                : GameMode.Estudo_Livre;
        }

        private void RefreshStars()
        {
            var gm = GameManager.Instance;
            int lives = gm != null ? gm.PlayerLives : 0;
            bool nopenalty = GetCurrentMode() == GameMode.Estudo_Livre;
            SetIconsActive(starIcons, nopenalty ? starIcons.Length : lives);
        }

        /// <summary>
        /// Ativa os primeiros 'count' ícones do array e desativa o restante.
        /// </summary>
        private static void SetIconsActive(GameObject[] icons, int count)
        {
            if (icons == null) return;
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] != null)
                    icons[i].SetActive(i < count);
            }
        }

    }
}
