using UnityEngine;
using UnityEngine.UI;
using Data;
using Core;
using TMPro;
using Domain;

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

        [Header("Botões e Ícones")]
        public Button homeButton;
        public Button shopButton;
        public Button settingsButton;
        public GameObject homeIcon0;
        public GameObject homeIcon1;
        public GameObject shopIcon0;
        public GameObject shopIcon1;
        public GameObject settingsIcon0;
        public GameObject settingsIcon1;

        [Header("Imagens de Seleção Dificuldade")]
        public GameObject easy0Image;
        public GameObject easy1Image;
        public GameObject medium0Image;
        public GameObject medium1Image;
        public GameObject hard0Image;
        public GameObject hard1Image;

        [Header("UI Texts")]
        public TextMeshProUGUI difficultyText;
        public TextMeshProUGUI modeText;

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
                data.mode == GameMode.Experimentos
                    ? "sem penalidade"
                    : $"{data.startingLives} vidas";

            difficultyText.text =
                $"{data.difficultyName} * {livesLabel} * x{data.scoreMultiplier:0.#} pontos";

            if (modeText != null)
                modeText.text = data.ModeLabel;
        }

        // Painéis principais
        public void ShowHomePanel()
        {
            homePanel?.SetActive(true);
            shopPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
            loadingPanel?.SetActive(false);

            if (homeButton != null) homeButton.interactable = false;
            if (shopButton != null) shopButton.interactable = true;
            if (settingsButton != null) settingsButton.interactable = true;

            OnHomeButtonClick();
        }

        public void ShowShopPanel()
        {
            homePanel?.SetActive(false);
            shopPanel?.SetActive(true);
            settingsPanel?.SetActive(false);
            loadingPanel?.SetActive(false);

            if (shopButton != null) shopButton.interactable = false;
            if (homeButton != null) homeButton.interactable = true;
            if (settingsButton != null) settingsButton.interactable = true;

            OnShopButtonClick();
        }

        public void ShowSettingsPanel()
        {
            homePanel?.SetActive(false);
            shopPanel?.SetActive(false);
            settingsPanel?.SetActive(true);
            loadingPanel?.SetActive(false);

            if (settingsButton != null) settingsButton.interactable = false;
            if (homeButton != null) homeButton.interactable = true;
            if (shopButton != null) shopButton.interactable = true;

            OnSettingsButtonClick();
        }

        public void ShowLoadingPanel()
        {
            homePanel?.SetActive(false);
            shopPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
            loadingPanel?.SetActive(true);
        }

        // Ação do botão "Jogar"
        public void LoadLabScene()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.StartGame();
        }

        // Seleção de dificuldade (OnClick)
        public void SelectEasy()
        {
            Debug.Log("Selecionado: EASY");
            SelectDifficulty(easyDifficulty);
        }

        public void SelectMedium()
        {
            Debug.Log("Selecionado: MEDIUM");
            SelectDifficulty(mediumDifficulty);
        }

        public void SelectHard()
        {
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

            easy0Image?.SetActive(!isEasy);
            easy1Image?.SetActive(isEasy);

            medium0Image?.SetActive(!isMedium);
            medium1Image?.SetActive(isMedium);

            hard0Image?.SetActive(!isHard);
            hard1Image?.SetActive(isHard);
        }

        // Funções auxiliares
        private void OnHomeButtonClick()
        {
            homeIcon0?.SetActive(false);
            homeIcon1?.SetActive(true);

            shopIcon0?.SetActive(true);
            shopIcon1?.SetActive(false);

            settingsIcon0?.SetActive(true);
            settingsIcon1?.SetActive(false);
        }

        private void OnShopButtonClick()
        {
            shopIcon0?.SetActive(false);
            shopIcon1?.SetActive(true);

            homeIcon0?.SetActive(true);
            homeIcon1?.SetActive(false);

            settingsIcon0?.SetActive(true);
            settingsIcon1?.SetActive(false);
        }

        private void OnSettingsButtonClick()
        {
            settingsIcon0?.SetActive(false);
            settingsIcon1?.SetActive(true);

            homeIcon0?.SetActive(true);
            homeIcon1?.SetActive(false);

            shopIcon0?.SetActive(true);
            shopIcon1?.SetActive(false);
        }
    }
}
