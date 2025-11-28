using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Data;
using Core;
using System.Collections;
using System;
using TMPro;


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
            {
                GameManager.Instance.OnDifficultyChanged += HandleDifficultyChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDifficultyChanged -= HandleDifficultyChanged;
            }
        }


        private void Start()
        {
            GameManager.Instance.SetDifficulty(easyDifficulty);
            DeselectAllDifficulties();
            ShowLoadingPanel();
            UpdateDifficultyLabel(GameManager.Instance.CurrentDifficulty);
        }

        private void HandleDifficultyChanged(DifficultyLevelData data)
        {
            UpdateDifficultyLabel(data);
        }

        private void UpdateDifficultyLabel(DifficultyLevelData data)
        {
            if (difficultyText == null || data == null) return;

            // Exemplo: "Fácil * 3 vidas * x1.0 pontos"
            difficultyText.text = $"{data.difficultyName} * {data.startingLives} vidas * x{data.scoreMultiplier:0.#} pontos";

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

            homeButton.interactable = false;
            shopButton.interactable = true;
            settingsButton.interactable = true;

            OnHomeButtonClick();

        }

        public void ShowShopPanel()
        {
            homePanel?.SetActive(false);
            shopPanel?.SetActive(true);
            settingsPanel?.SetActive(false);
            loadingPanel?.SetActive(false);

            shopButton.interactable = false;
            homeButton.interactable = true;
            settingsButton.interactable = true;

            OnShopButtonClick();
        }

        public void ShowSettingsPanel()
        {
            homePanel?.SetActive(false);
            shopPanel?.SetActive(false);
            settingsPanel?.SetActive(true);
            loadingPanel?.SetActive(false);

            settingsButton.interactable = false;
            homeButton.interactable = true;
            shopButton.interactable = true;

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
            GameManager.Instance.StartGame();
        }

        // Seleção de dificuldade
        public void SelectEasy()
        {
            Debug.Log("Selecionado: EASY");

            easy0Image?.SetActive(false);
            easy1Image?.SetActive(true);

            medium0Image?.SetActive(true);
            medium1Image?.SetActive(false);

            hard0Image?.SetActive(true);
            hard1Image?.SetActive(false);

            GameManager.Instance.SetDifficulty(easyDifficulty);
            //UpdateDifficultySelectionVisuals(easy1Image);
        }

        public void SelectMedium()
        {
            Debug.Log("Selecionado: MEDIUM");
            easy0Image?.SetActive(true);
            easy1Image?.SetActive(false);

            medium0Image?.SetActive(false);
            medium1Image?.SetActive(true);

            hard0Image?.SetActive(true);
            hard1Image?.SetActive(false);

            GameManager.Instance.SetDifficulty(mediumDifficulty);
            //UpdateDifficultySelectionVisuals(medium1Image);
        }

        public void SelectHard()
        {
            Debug.Log("Selecionado: HARD");

            easy0Image?.SetActive(true);
            easy1Image?.SetActive(false);

            medium0Image?.SetActive(true);
            medium1Image?.SetActive(false);

            hard0Image?.SetActive(false);
            hard1Image?.SetActive(true);

            GameManager.Instance.SetDifficulty(hardDifficulty);
            //UpdateDifficultySelectionVisuals(hard1Image);
        }

        /*TODO: lógica refatorada para selecionar imagens ativadas/desativadas
        private void UpdateDifficultySelectionVisuals(GameObject activeImage)
        {
            easy1Image?.SetActive(false);
            medium1Image?.SetActive(false);
            hard1Image?.SetActive(false);
            activeImage?.SetActive(true);
        }*/

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

        // Deselect all difficulty highlights
        public void DeselectAllDifficulties()
        {
            easy1Image?.SetActive(false);
            medium1Image?.SetActive(false);
            hard1Image?.SetActive(false);
        }
    }
}