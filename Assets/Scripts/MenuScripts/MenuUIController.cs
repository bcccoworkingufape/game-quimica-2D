using UnityEngine;
using UnityEngine.UI;
using Data;
using Core;
using TMPro;
using Domain;
using System.Collections;
using Core.Audio;
using Presentation.Common;
using Presentation.Menu;

namespace MenuScripts
{
    /// <summary>
    /// View do menu no padrão MVP. Mantém todos os SerializeField/métodos públicos
    /// usados pelos OnClick das scenes/prefabs e delega decisões ao <see cref="MenuPresenter"/>.
    /// </summary>
    public class MenuUIController : MonoBehaviour, IMenuView
    {
        private MenuPresenter _presenter;
        // --- Referências aos ScriptableObjects de Dificuldade ---
        [Header("Dados de Dificuldade")]
        public DifficultyLevelData easyDifficulty;
        public DifficultyLevelData mediumDifficulty;
        public DifficultyLevelData hardDifficulty;

        [Header("Painéis")]
        public GameObject homePanel;
        public GameObject shopPanel;
        public GameObject settingsPanel;
        public GameObject navbarPanel;
        public GameObject loadingPanel;
        public GameObject aboutPanel;
        public GameObject helpPanel;
        public GameObject fadePanel;

        [Header("UI Texts")]
        public TextMeshProUGUI difficultyText;
        public TextMeshProUGUI modeText;
        [Header("Toggle de Música")]
        [SerializeField] private GameObject musicOnObject;
        [SerializeField] private GameObject musicOffObject;

        [Header("Toggle de SFX")]
        [SerializeField] private GameObject sfxOnObject;
        [SerializeField] private GameObject sfxOffObject;

        [Header("Loading Screen - Dicas")]
        [SerializeField] private TextMeshProUGUI hintText;

        [SerializeField] private float hintChangeInterval = 3f;

        private Coroutine _hintCoroutine;

        // Dicas
        private static readonly string[] LoadingHints = new string[]
        {
            // ═══════════════════════════════════════════
            // DICAS DE GAMEPLAY
            // ═══════════════════════════════════════════
            "Se flutua, é menos denso. Se afunda, é mais denso. Molz sempre observando!",
            "O tornassol fica vermelho em ácidos e azul em bases. Anota aí!!",
            "A árvore de decisão é sua melhor amiga... exceto no modo Desafio..",
            "Nem todo composto reage igual com NaOH e HCl. Teste ambos!",
            "Sais orgânicos geralmente são solúveis em água. Comece por aí!",
            "Quando em dúvida, teste com água primeiro. É o solvente universal!",
            "Ésteres costumam ser solúveis em éter dietílico. Lembre-se disso!",
            "No modo Estudo Livre você pode errar à vontade. Aproveite para aprender!",
            "Responder errado no modo Experimentos custa uma vida. Pense bem!",
            "O modo Desafio testa tudo que você aprendeu. Sem árvore, sem moleza!",
            "Preste atenção no tipo de mistura: líquido-líquido ou sólido-líquido.",
            // ═══════════════════════════════════════════
            // CURIOSIDADES CIENTÍFICAS
            // ═══════════════════════════════════════════
            "Você sabia? 'Semelhante dissolve semelhante' é a regra de ouro da solubilidade!",
            "O etanoato de etila dá o cheiro característico de esmalte de unhas.",
            "Aminas como a butan-1-amina têm cheiro de peixe. Molz não é fã.",
            "O ácido oleico é encontrado no azeite de oliva. Química no almoço!",
            "Compostos aromáticos têm anéis de benzeno, não necessariamente cheiro.",
            "A 4-aminobenzenossulfonamida é usada em antibióticos! Química salva vidas.",
            "O metilbenzeno também é conhecido como tolueno. Nome de laboratório!",
            "Cicloexanona é usada na produção de nylon. Química está em tudo!",
            "Ácidos carboxílicos como o propanoico doam H<sup>+</sup> facilmente.",
            "Bases orgânicas como aminas aceitam H<sup>+</sup>. É o oposto dos ácidos!",
            //"Densidade determina se algo flutua ou afunda.",
            "O pH neutro é 7. Abaixo é ácido, acima é básico. Simples assim!",
            // ═══════════════════════════════════════════
            // MOLZ - O RATINHO CIENTISTA (divertidas)
            // ═══════════════════════════════════════════
            "Dizem que Molz sonha com fórmulas químicas. E com queijo do reino.",
            "Molz não erra experimentos. Ele apenas descobre resultados inesperados.",
            "O jaleco do Molz tem manchas de todos os solventes..",
            //"Molz acredita em você! ... Mas confere o histórico só pra ter certeza.",
            "Perguntaram ao Molz qual seu elemento favorito. Ele disse: 'Queijônio'..",
            "Molz tentou fazer café no laboratório. O orientador não aprovou.",
            "Molz acha que todo problema se resolve com mais um experimento.",
            "Se Molz pudesse, colocaria queijo na tabela periódica.",
            "Molz leu todos os rótulos de produtos de limpeza. Por diversão.",
            "O bigode do Molz vibra quando ele encontra a mistura correta!",
            "Molz já derrubou um erlenmeyer. Só um. Ele jura.",
            "Curiosidade: Molz vem de 'Molécula'. (E quase que seu nome era 'Moléquim')",
            // ═══════════════════════════════════════════
            // MOTIVACIONAIS / ESTILO LoL e Terraria 👀
            // ═══════════════════════════════════════════
            "A química é como a vida: questão de encontrar o equilíbrio certo.",
            "Cada erro é um experimento. Cada acerto, uma descoberta!",
            "Cientistas não falham. Eles eliminam hipóteses que não funcionam!",
            "O laboratório é seu. Os compostos aguardam. Boa sorte, cientista!!",
            "Conhecimento é a única coisa que aumenta quando compartilhado.",
        };

        private void Awake()
        {
            _presenter = new MenuPresenter(this);
        }

        private void OnEnable()
        {
            // O Presenter assina OnDifficultyChanged do Model (GameManager) e
            // executa a sincronização inicial (seleção, rótulo, toggles de áudio).
            _presenter?.Initialize(easyDifficulty, mediumDifficulty, hardDifficulty);
        }

        private void OnDisable()
        {
            _presenter?.Dispose();
        }

        private void Start()
        {
            OverlayAnimator.HideImmediate(aboutPanel);
            OverlayAnimator.HideImmediate(helpPanel);
            OverlayAnimator.HideImmediate(fadePanel);

            ShowLoadingPanel();
            // Sincronização inicial (dificuldade, toggles) já ocorreu em
            // MenuPresenter.Initialize() via OnEnable.
        }

        // ─────────────────────────────────────────────
        // IMenuView — comandos disparados pelo MenuPresenter
        // ─────────────────────────────────────────────

        public void RenderDifficulty(DifficultyLevelData data)
        {
            if (difficultyText == null || data == null) return;

            string livesLabel =
                data.mode == GameMode.Estudo_Livre
                    ? "sem penalidade"
                    : $"{data.startingLives} vidas";

            difficultyText.text =
                $"{data.difficultyName} * {livesLabel} * x{data.scoreMultiplier:0.#} pontos";

            if (modeText != null)
                modeText.text = data.ModeLabel;
        }

        public void ApplySelectionVisuals(DifficultyLevelData data)
        {
            // Hook para destacar visualmente a dificuldade ativa.
            // Hoje não há sprites de highlight conectados; o método permanece como
            // ponto de extensão para o Presenter sinalizar a seleção atual.
            if (data == null) data = easyDifficulty;
        }

        // Painéis principais

        public void ShowLoadingPanel()
        {
            homePanel?.SetActive(false);
            shopPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
            loadingPanel?.SetActive(true);

            StartHintCycle();

            if (MusicManager.Instance != null && MusicManager.Instance.IsMusicEnabled())
            {
                MusicManager.Instance.StartMenuMusicFromLoading(targetVolume: 0.55f, duration: 1.8f);
            }
        }

        public void HideLoadingPanel()
        {
            loadingPanel?.SetActive(false);
            homePanel?.SetActive(true);

            StopHintCycle();
        }
        // ─────────────────────────────────────────────
        // Sistema de Dicas (Loading Screen)
        // ─────────────────────────────────────────────

        private void StartHintCycle()
        {
            StopHintCycle();
            ShowRandomHint();
            _hintCoroutine = StartCoroutine(HintCycleCoroutine());
        }

        private void StopHintCycle()
        {
            if (_hintCoroutine != null)
            {
                StopCoroutine(_hintCoroutine);
                _hintCoroutine = null;
            }
        }

        private IEnumerator HintCycleCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(hintChangeInterval);
                ShowRandomHint();
            }
        }

        private void ShowRandomHint()
        {
            if (hintText == null || LoadingHints.Length == 0) return;

            int index = Random.Range(0, LoadingHints.Length);
            hintText.text = LoadingHints[index];
        }

        // Ação do botão "Jogar" — delega ao Presenter (que cuida do fade da música
        // e dispara GameManager.StartGame()).
        public void LoadLabScene()
        {
            SfxManager.Instance?.PlayButtonClick();
            if (GameManager.Instance == null) return;

            StopHintCycle();
            _presenter?.StartGame();
        }

        // Seleção de dificuldade (OnClick) — delegam ao Presenter
        public void SelectEasy()
        {
            SfxManager.Instance?.PlayButtonClick();
            Debug.Log("Selecionado: EASY");
            _presenter?.SelectEasy();
        }

        public void SelectMedium()
        {
            SfxManager.Instance?.PlayButtonClick();
            Debug.Log("Selecionado: MEDIUM");
            _presenter?.SelectMedium();
        }

        public void SelectHard()
        {
            SfxManager.Instance?.PlayButtonClick();
            Debug.Log("Selecionado: HARD");
            _presenter?.SelectHard();
        }

        // garante que o highlight bate com a dificuldade atual
        private void ApplyDifficultySelectionVisuals(DifficultyLevelData data)
        {
            ApplySelectionVisuals(data);
        }

        // Botão "Fechar o jogo" (encerra a aplicacao).
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
        // Painéis Sobre o Jogo / Ajuda
        // ─────────────────────────────────────────────

        public void ShowAboutPanel()
        {
            SfxManager.Instance?.PlayButtonClick();

            navbarPanel?.SetActive(false);
            OverlayAnimator.Show(fadePanel);
            OverlayAnimator.Show(aboutPanel);
        }

        public void HideAboutPanel()
        {
            SfxManager.Instance?.PlayButtonClick();

            OverlayAnimator.Hide(aboutPanel, onComplete: () =>
            {
                navbarPanel?.SetActive(true);
            });

            OverlayAnimator.Hide(fadePanel);
        }

        public void ShowHelpPanel()
        {
            SfxManager.Instance?.PlayButtonClick();

            navbarPanel?.SetActive(false);
            OverlayAnimator.Show(fadePanel);
            OverlayAnimator.Show(helpPanel);
        }

        public void HideHelpPanel()
        {
            SfxManager.Instance?.PlayButtonClick();

            OverlayAnimator.Hide(helpPanel, onComplete: () =>
            {
                navbarPanel?.SetActive(true);
            });

            OverlayAnimator.Hide(fadePanel);
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
        // Sfx
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


    }
}