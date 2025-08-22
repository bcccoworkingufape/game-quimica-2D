using System.Collections;
using UnityEngine;
using TMPro;

namespace LabScripts
{
    public class UIController : MonoBehaviour
    {
        public GameObject solutionAnimationPanel;
        public GameObject confirmationPanel;
        public GameObject questionPanel;
        public GameObject historyPanel;
        public GameObject treePanel;
        public TextMeshProUGUI confirmationPanelText;
        public TextMeshProUGUI solutionAnimationText;

        [HideInInspector]
        public string currentItemName;

        // Hide the confirmation panel at the start
        void Start()
        {
            HideAllPanels();
        }

        public void HideAllPanels()
        {
            solutionAnimationPanel?.SetActive(false);
            confirmationPanel?.SetActive(false);
            questionPanel?.SetActive(false);
            historyPanel?.SetActive(false);
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

    }
}