using UnityEngine;
using UnityEngine.UI;
using Data;
using Core;
using TMPro;
using Domain;
using System.Collections;
using Core.Audio;

namespace MenuScripts
{
    public class MenuUIController : MonoBehaviour
    {
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
            "Ácidos carboxílicos como o propanoico doam H⁺ facilmente.",
            "Bases orgânicas como aminas aceitam H⁺. É o oposto dos ácidos!",
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

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnDifficultyChanged += HandleDifficultyChanged;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnDifficultyChanged -= HandleDifficultyChanged;
        }

        private void Start()
        {
            ShowLoadingPanel();

            if (GameManager.Instance == null)
            {
                Debug.LogError("[MenuUIController] GameManager.Instance é nulo. Verifique se o GameManager está na cena inicial.");
                return;
            }

            // respeita a dificuldade já escolhida (se voltou do Lab), senão usa easy
            var current = GameManager.Instance.CurrentDifficulty != null
                ? GameManager.Instance.CurrentDifficulty
                : easyDifficulty;

            // garante que o GameManager não fique nulo
            if (GameManager.Instance.CurrentDifficulty == null && current != null)
                GameManager.Instance.SetDifficulty(current);

            ApplyDifficultySelectionVisuals(current);
            UpdateDifficultyLabel(current);
            RefreshMusicToggleVisual();
            RefreshSfxToggleVisual(); 
        }

        private void HandleDifficultyChanged(DifficultyLevelData data)
        {
            ApplyDifficultySelectionVisuals(data);
            UpdateDifficultyLabel(data);
        }

        private void UpdateDifficultyLabel(DifficultyLevelData data)
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

        // Ação do botão "Jogar"
        public void LoadLabScene()
        {
            SfxManager.Instance?.PlayButtonClick();

            if (GameManager.Instance == null) return;

            StopHintCycle();

            if (MusicManager.Instance != null && MusicManager.Instance.IsMusicEnabled())
            {
                MusicManager.Instance.FadeTo(0.6f, 0.25f);
            }

            GameManager.Instance.StartGame();
        }

        // Seleção de dificuldade (OnClick)
        public void SelectEasy()
        {
            SfxManager.Instance?.PlayButtonClick();
            Debug.Log("Selecionado: EASY");
            SelectDifficulty(easyDifficulty);
        }

        public void SelectMedium()
        {
            SfxManager.Instance?.PlayButtonClick();
            Debug.Log("Selecionado: MEDIUM");
            SelectDifficulty(mediumDifficulty);
        }

        public void SelectHard()
        {
            SfxManager.Instance?.PlayButtonClick();
            Debug.Log("Selecionado: HARD");
            SelectDifficulty(hardDifficulty);
        }

        // centralizar a seleção
        private void SelectDifficulty(DifficultyLevelData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[MenuUIController] DifficultyLevelData nulo em SelectDifficulty().");
                return;
            }

            ApplyDifficultySelectionVisuals(data);

            if (GameManager.Instance != null)
                GameManager.Instance.SetDifficulty(data);

            UpdateDifficultyLabel(data);
        }

        // garante que o highlight bate com a dificuldade atual
        private void ApplyDifficultySelectionVisuals(DifficultyLevelData data)
        {
            if (data == null) data = easyDifficulty;

            bool isEasy = data == easyDifficulty;
            bool isMedium = data == mediumDifficulty;
            bool isHard = data == hardDifficulty;

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

            aboutPanel?.SetActive(true);
            navbarPanel?.SetActive(false);
            fadePanel?.SetActive(true);
        }

        public void HideAboutPanel()
        {
            SfxManager.Instance?.PlayButtonClick();

            aboutPanel?.SetActive(false);
            navbarPanel?.SetActive(true);
            fadePanel?.SetActive(false);
        }

        public void ShowHelpPanel()
        {
            SfxManager.Instance?.PlayButtonClick();

            helpPanel?.SetActive(true);
            navbarPanel?.SetActive(false);
            fadePanel?.SetActive(true);
        }

        public void HideHelpPanel()
        {
            SfxManager.Instance?.PlayButtonClick();

            helpPanel?.SetActive(false);
            navbarPanel?.SetActive(true);
            fadePanel?.SetActive(false);
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