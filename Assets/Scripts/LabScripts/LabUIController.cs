using System.Collections;
using UnityEngine;
using TMPro;
using Data;
using Core;
using Presentation.Lab;
using Presentation.Common;
using Domain;
using UnityEngine.UI;
using Core.Audio;

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
        [SerializeField] private Button treeButton;
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

        [Header("Painel de Informações")]
        public GameObject infoPanel;

        [Header("Toggle de Música")]
        [SerializeField] private GameObject musicOnObject;
        [SerializeField] private GameObject musicOffObject;

        [Header("Toggle de SFX")]
        [SerializeField] private GameObject sfxOnObject;
        [SerializeField] private GameObject sfxOffObject;

        [Header("Zoom da Árvore")]
        [SerializeField] private Slider treeSlider;
        [SerializeField] private Image treeImage;


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
            Time.timeScale = 1f;

            HideAllPanels();
            ShowInfoPanel();

            var gm = GameManager.Instance;
            if (gm != null && gm.CurrentDifficulty != null)
            {
                UpdateDifficultyLabel(gm.CurrentDifficulty);
                UpdateLivesLabel(gm.PlayerLives);
                RefreshHearts(gm.PlayerLives);
            }

            if (gm != null)
                UpdatePercentageText(gm.GetProgressPercentage());

            RefreshMusicToggleVisual();
            RefreshSfxToggleVisual();
        }

        // ─────────────────────────────────────────────
        // Painel de Informações (overlay inicial)
        // ─────────────────────────────────────────────

        public void ShowInfoPanel()
        {
            OverlayAnimator.Show(infoPanel);
        }

        /// <summary>
        /// Chamado pelo botão "Entendi!" no painel de informações.
        /// </summary>
        public void HideInfoPanel()
        {
            SfxManager.Instance?.PlayButtonClick();
            OverlayAnimator.Hide(infoPanel);
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

            if (!treeAllowed)
                OverlayAnimator.HideImmediate(treePanel);

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
                if (treeButton != null && treeButton.targetGraphic != null)
                    treeButton.targetGraphic.color = tint;
            }
        }

        // ─────────────────────────────────────────────
        // Controle de painéis
        // ─────────────────────────────────────────────

        /// <summary>
        /// Fecha todos os painéis imediatamente (sem animação), usada na inicialização.
        /// O solutionAnimationPanel permanece ativo pois possui tratamento próprio.
        /// </summary>
        public void HideAllPanels()
        {
            // solutionAnimationPanel precisa estar ativo todo momento — não entra aqui.
            solutionAnimationPanel?.SetActive(true);

            OverlayAnimator.HideImmediate(confirmationPanel);
            OverlayAnimator.HideImmediate(questionPanel);
            OverlayAnimator.HideImmediate(historyPanel);
            OverlayAnimator.HideImmediate(treePanel);
            OverlayAnimator.HideImmediate(pauseMenuPanel);
            OverlayAnimator.HideImmediate(questionErrorPanel);
            OverlayAnimator.HideImmediate(questionVictoryPanel);
            OverlayAnimator.HideImmediate(defeatPanel);
            OverlayAnimator.HideImmediate(infoPanel);
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
                confirmationPanelText.text = currentItemName == "Tornassol" ? litmusText : defaultText;

            OverlayAnimator.Show(confirmationPanel);
        }

        public void HideConfirmationPanel()
        {
            OverlayAnimator.Hide(confirmationPanel);
        }

        /// <summary>
        /// Botão "Sim" do popup.
        /// </summary>
        public void OnConfirmAction()
        {
            SfxManager.Instance?.PlayButtonClick();

            Debug.Log("Ação Confirmada para o item: " + currentItemName);
            HideConfirmationPanel();

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
            SfxManager.Instance?.PlayButtonClick();

            Debug.Log("Ação Cancelada para o item: " + currentItemName);
            HideConfirmationPanel();
        }

        public void OnRepeatMixButton()
        {
            SfxManager.Instance?.PlayButtonClick();
            SfxManager.Instance?.PlayMix();

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, -1, 0f);
        }

        // ─────────────────────────────────────────────
        // Solution Animation Panel
        // ─────────────────────────────────────────────

        // O solutionAnimationPanel possui tratamento próprio via SolutionPanelAnimator,
        // portanto não entra no fluxo do OverlayAnimator.

        public void ShowSolutionAnimationPanel()
        {
            if (solutionPanelAnimator != null)
            {
                solutionPanelAnimator.Open();
                animator.enabled = true;
            }
            else
            {
                solutionAnimationPanel?.SetActive(true);
            }
        }

        public void HideSolutionAnimationPanel()
        {
            if (solutionPanelAnimator != null)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                animator.Play(stateInfo.fullPathHash, -1, 0f);
                animator.enabled = false;

                solutionPanelAnimator.Close();
            }
            else
            {
                solutionAnimationPanel?.SetActive(true); // true para não desativar o painel
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
            OverlayAnimator.Show(questionPanel);
        }

        public void HideQuestionPanel()
        {
            OverlayAnimator.Hide(questionPanel);
        }

        /// <summary>
        /// Chamado pelos botões de alternativa. O índice vem direto do OnClick do botão.
        /// </summary>
        public void OnQuestionSelect(int optionIndex)
        {
            SfxManager.Instance?.PlayButtonClick();

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
            SfxManager.Instance?.PlayButtonClick();

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
            SfxManager.Instance?.PlayHistoryClick();

            OverlayAnimator.Show(historyPanel);

            if (historyPanelController != null)
                historyPanelController.RefreshHistory();
        }

        public void HideHistoryPanel()
        {
            SfxManager.Instance?.PlayButtonClick();
            OverlayAnimator.Hide(historyPanel);
        }

        // ─────────────────────────────────────────────
        // Tree Panel
        // ─────────────────────────────────────────────

        public void ShowTreePanel()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.CurrentDifficulty != null && gm.CurrentDifficulty.mode == GameMode.Desafio)
                return;

            SfxManager.Instance?.PlayTreeClick();
            OverlayAnimator.Show(treePanel);
        }

        public void HideTreePanel()
        {
            SfxManager.Instance?.PlayButtonClick();
            OverlayAnimator.Hide(treePanel);
        }

        // ─────────────────────────────────────────────
        // Question Error Panel
        // ─────────────────────────────────────────────

        public void ShowQuestionErrorPanel()
        {
            OverlayAnimator.Show(questionErrorPanel);
        }

        /// <summary>
        /// Fecha o painel de erro e reabre o painel de alternativas.
        /// O questionPanel abre com animação após o errorPanel terminar de fechar.
        /// </summary>
        public void HideQuestionErrorPanel()
        {
            SfxManager.Instance?.PlayButtonClick();

            // Aguarda a animação de saída do errorPanel terminar antes de abrir
            // o questionPanel, mantendo a transição visualmente limpa.
            OverlayAnimator.Hide(questionErrorPanel, onComplete: () =>
            {
                OverlayAnimator.Show(questionPanel);
            });
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
            OverlayAnimator.Show(questionVictoryPanel);
        }

        // Fallback caso alguém chame sem nome.
        public void ShowQuestionVictoryPanel()
        {
            ShowQuestionVictoryPanel(string.Empty);
        }

        public void HideQuestionVictoryPanel()
        {
            OverlayAnimator.Hide(questionVictoryPanel, onComplete: () =>
            {
                SetIconsActive(starIcons, 0);
            });
        }

        /// <summary>
        /// Botão "Próxima fase" no painel de vitória.
        /// </summary>
        public void OnVictoryNextPhase()
        {
            SfxManager.Instance?.PlayButtonClick();

            ResetFlowState(resetScore: false, resetLives: false);

            if (questionFlowPresenter != null)
                questionFlowPresenter.PrepareNextCompound(forceNew: true);
            else
                Debug.LogWarning("[LabUIController] QuestionFlowPresenter não atribuído em OnVictoryNextPhase.");
        }

        /// <summary>
        /// Botão "Reiniciar" no painel de vitória.
        /// </summary>
        public void OnVictoryRestart()
        {
            SfxManager.Instance?.PlayButtonClick();

            ResetFlowState(resetScore: true, resetLives: true);
            RestartLab();
        }

        /// <summary>
        /// Botão "Voltar ao menu" no painel de vitória.
        /// </summary>
        public void OnVictoryReturnToMenu()
        {
            SfxManager.Instance?.PlayButtonClick();

            ResetFlowState(resetScore: false, resetLives: true);
            ReturnToMainMenu();
        }

        // ─────────────────────────────────────────────
        // Defeat Panel (Derrota)
        // ─────────────────────────────────────────────

        public void ShowDefeatPanel()
        {
            SfxManager.Instance?.PlayLose();

            OverlayAnimator.Show(defeatPanel, ignoreTimeScale: true);
            Time.timeScale = 0f;
        }

        public void HideDefeatPanel()
        {
            SfxManager.Instance?.PlayButtonClick();

            OverlayAnimator.Hide(defeatPanel, onComplete: () =>
            {
                Time.timeScale = 1f;
            }, ignoreTimeScale: true);
        }

        /// <summary>
        /// Botão "Reiniciar" no painel de derrota.
        /// </summary>
        public void OnDefeatRestart()
        {
            SfxManager.Instance?.PlayButtonClick();

            GameManager.Instance?.ResetQuestionRun();
            ResetFlowState(resetScore: true, resetLives: true);
            RestartLab();
        }

        /// <summary>
        /// Botão "Voltar ao menu" no painel de derrota.
        /// </summary>
        public void OnDefeatReturnToMenu()
        {
            SfxManager.Instance?.PlayButtonClick();

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
            SfxManager.Instance?.PlayButtonClick();

            // timeScale é zerado DEPOIS de iniciar a animação para que o LeanTween
            // ainda consiga processar o primeiro frame. ignoreTimeScale:true garante
            // que a animação rode mesmo com timeScale == 0.
            OverlayAnimator.Show(pauseMenuPanel, ignoreTimeScale: true);
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Botão "Retomar" dentro do painel de pausa.
        /// </summary>
        public void ResumeGame()
        {
            SfxManager.Instance?.PlayButtonClick();

            // Restaura o timeScale antes de animar para que o jogo retome
            // imediatamente; ignoreTimeScale:true mantém a animação de saída fluida.
            Time.timeScale = 1f;
            OverlayAnimator.Hide(pauseMenuPanel, ignoreTimeScale: true);
        }

        /// <summary>
        /// Botão "Voltar ao Menu Principal" no painel de pausa.
        /// </summary>
        public void ReturnToMainMenu()
        {
            SfxManager.Instance?.PlayButtonClick();

            Time.timeScale = 1f;
            GameManager.Instance.LoadScene("1_MenuScene");
        }

        /// <summary>
        /// Botão "Fechar o jogo" (encerra a aplicação).
        /// </summary>
        public void QuitGame()
        {
            SfxManager.Instance?.PlayButtonClick();

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

            // solutionAnimationPanel tem tratamento próprio — permanece ativo.
            if (solutionAnimationPanel) solutionAnimationPanel.SetActive(true);

            // Todos os outros overlays fecham imediatamente (reset de estado, sem animação).
            OverlayAnimator.HideImmediate(confirmationPanel);
            OverlayAnimator.HideImmediate(questionPanel);
            OverlayAnimator.HideImmediate(pauseMenuPanel);
            OverlayAnimator.HideImmediate(questionErrorPanel);
            OverlayAnimator.HideImmediate(questionVictoryPanel);
            OverlayAnimator.HideImmediate(defeatPanel);
            OverlayAnimator.HideImmediate(historyPanel);
            OverlayAnimator.HideImmediate(treePanel);
            OverlayAnimator.HideImmediate(infoPanel);

            if (victoryCompoundText) victoryCompoundText.text = "Composto X:";

            SetIconsActive(starIcons, 0);

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

        // ─────────────────────────────────────────────
        // Música
        // ─────────────────────────────────────────────

        public void OnClickEnableMusic()
        {
            SfxManager.Instance?.PlayButtonClick();

            if (MusicManager.Instance == null) return;

            MusicManager.Instance.EnableMusic();
            RefreshMusicToggleVisual();
        }

        public void OnClickDisableMusic()
        {
            SfxManager.Instance?.PlayButtonClick();

            if (MusicManager.Instance == null) return;

            MusicManager.Instance.DisableMusic();
            RefreshMusicToggleVisual();
        }

        public void RefreshMusicToggleVisual()
        {
            if (MusicManager.Instance == null) return;

            bool isEnabled = MusicManager.Instance.IsMusicEnabled();

            if (musicOnObject != null)
                musicOnObject.SetActive(isEnabled);

            if (musicOffObject != null)
                musicOffObject.SetActive(!isEnabled);
        }

        // ─────────────────────────────────────────────
        // SFX
        // ─────────────────────────────────────────────

        public void OnClickEnableSfx()
        {
            Debug.Log("=== LAB: CLICOU EM LIGAR SFX ===");
            SfxManager.Instance?.EnableSfx();
            RefreshSfxToggleVisual();
        }

        public void OnClickDisableSfx()
        {
            Debug.Log("=== LAB: CLICOU EM DESLIGAR SFX ===");
            SfxManager.Instance?.DisableSfx();
            RefreshSfxToggleVisual();
        }

        public void RefreshSfxToggleVisual()
        {
            if (SfxManager.Instance == null) return;

            bool isEnabled = SfxManager.Instance.IsSfxEnabled();

            if (sfxOnObject != null)
                sfxOnObject.SetActive(isEnabled);

            if (sfxOffObject != null)
                sfxOffObject.SetActive(!isEnabled);
        }

        public void OnTreeSliderChange()
        {
            if (treeImage != null && treeSlider != null)
            {
                float zoomValue = treeSlider.value;
                treeImage.transform.localScale = Vector3.one * zoomValue;
            }
        }
    }
}