using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Domain;
using Core;

namespace Presentation.Lab
{
    /// <summary>
    /// Preenche o painel de histórico com as misturas registradas.
    /// </summary>
    public class HistoryPanelController : MonoBehaviour
    {
        [Header("Referências de UI")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform listContainer;       // Content do ScrollView
        [SerializeField] private GameObject historyItemPrefab;
        [Header("Empty state")]
        [SerializeField] private GameObject emptyStateObject;
        [SerializeField, TextArea] private string emptyStateMessage = "Inicie Alguma Mistura!";

        private IHistoryService _historyService;

        private bool EnsureHistoryService()
        {
            if (_historyService != null) return true;
            _historyService = ServiceLocator.Resolve<IHistoryService>();
            return _historyService != null;
        }

        private void Awake()
        {
            if (scrollRect == null)
                scrollRect = GetComponentInChildren<ScrollRect>(true);

            if (scrollRect != null && listContainer == null)
                listContainer = scrollRect.content;

            if (emptyStateObject != null)
            {
                var tmp = emptyStateObject.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = emptyStateMessage;

                emptyStateObject.SetActive(false);
            }
        }


        public void RefreshHistory(bool keepScrollPosition = false)
        {
            if (!EnsureHistoryService())
            {
                Debug.LogWarning("[HistoryPanelController] HistoryService nulo ao tentar atualizar histórico.");
                return;
            }
            if (listContainer == null || historyItemPrefab == null)
            {
                Debug.LogWarning("[HistoryPanelController] ListContainer ou HistoryItemPrefab não atribuídos.");
                return;
            }

            // salva scroll atual (se quiser preservar)
            float prevY = scrollRect != null ? scrollRect.verticalNormalizedPosition : 1f;

            // Limpa itens antigos
            for (int i = listContainer.childCount - 1; i >= 0; i--)
                Destroy(listContainer.GetChild(i).gameObject);

            var entries = _historyService.GetAll();
            int count = entries?.Count ?? 0;

            if (emptyStateObject != null)
                emptyStateObject.SetActive(count == 0);

            if (count == 0)
            {
                if (scrollRect != null)
                {
                    scrollRect.horizontalNormalizedPosition = 0f;
                    scrollRect.StopMovement();
                    scrollRect.verticalNormalizedPosition = keepScrollPosition ? prevY : 1f;
                }
                return;
            }

            // renderiza itens
            foreach (var entry in entries)
            {
                var go = Instantiate(historyItemPrefab, listContainer);
                var rect = go.transform as RectTransform;
                if (rect != null) rect.localScale = Vector3.one;

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text == null) continue;

                var outcome = entry.Outcome;

                string solventName = outcome.Solvent.Name;

                string compoundState = "";
                switch (outcome.Compound.State)
                {
                    case PhysicalState.LIQUID:
                        compoundState = "líquido";
                        break;
                    case PhysicalState.SOLID:
                        compoundState = "sólido";
                        break;
                }

                string litmusText = "";
                switch (outcome.LitmusResult)
                {
                    case LitmusResultKind.Acidic:
                        litmusText = "vermelho";
                        break;
                    case LitmusResultKind.Basic:
                        litmusText = "azul";
                        break;
                    default:
                        litmusText = "incolor";
                        break;
                }

                string solubilityText = "";
                switch (outcome.SolubilityResult)
                {
                    case SolubilityResultKind.Soluble:
                        solubilityText = "solúvel";
                        break;
                    case SolubilityResultKind.InsolubleFloat:
                        solubilityText = "boia";
                        break;
                    case SolubilityResultKind.InsolubleSink:
                        solubilityText = "afunda";
                        break;
                }


                if (solventName == "Tornassol")
                {
                    text.text = $"{entry.Order}) " +
                                $"O composto é {compoundState} e {solubilityText} no <b>{solventName}</b> e fica {litmusText}";
                }
                else
                {
                    text.text =
                        $"{entry.Order}) " +
                        $"O composto é {compoundState} e {solubilityText} no <b>{solventName}</b>";
                }
            }

            // força o layout recalcular tamanho do Content (isso destrava o scroll)
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(listContainer);
            Canvas.ForceUpdateCanvases();

            // garante que só rola no Y e não “salta” de volta
            if (scrollRect != null)
            {
                scrollRect.horizontalNormalizedPosition = 0f;
                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition = keepScrollPosition ? prevY : 1f;
            }
        }

    }
}
