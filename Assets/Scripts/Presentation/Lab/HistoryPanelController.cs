using UnityEngine;
using TMPro;
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
        [SerializeField] private Transform listContainer;       // Content do ScrollView
        [SerializeField] private GameObject historyItemPrefab;  // Prefab com um TextMeshProUGUI

        private IHistoryService _historyService;

        // IMPORTANTE: só Start, sem Awake nem OnEnable
        private void Start()
        {
            _historyService = ServiceLocator.Resolve<IHistoryService>();

            if (_historyService == null)
            {
                Debug.LogError("[HistoryPanelController] IHistoryService não resolvido no Start.");
            }

            if (listContainer == null)
                Debug.LogError("[HistoryPanelController] ListContainer não atribuído.");
            if (historyItemPrefab == null)
                Debug.LogError("[HistoryPanelController] HistoryItemPrefab não atribuído.");
        }

        private bool EnsureHistoryService()
        {
            if (_historyService != null) return true;

            _historyService = ServiceLocator.Resolve<IHistoryService>();
            return _historyService != null;
        }


        public void RefreshHistory()
        {
            if (!EnsureHistoryService())
            {
                Debug.LogWarning("[HistoryPanelController] HistoryService nulo ao tentar atualizar histórico.");
                return;
            }
            if (_historyService == null)
            {
                Debug.LogWarning("[HistoryPanelController] HistoryService nulo ao tentar atualizar histórico.");
                return;
            }
            if (listContainer == null || historyItemPrefab == null)
            {
                Debug.LogWarning("[HistoryPanelController] ListContainer ou HistoryItemPrefab não atribuídos.");
                return;
            }

            // Limpa itens antigos
            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            var entries = _historyService.GetAll();
            Debug.Log($"[HistoryPanel] Atualizando histórico. Qtd entradas: {entries.Count}");

            foreach (var entry in entries)
            {
                var go = Instantiate(historyItemPrefab, listContainer);
                var rect = go.transform as RectTransform;
                rect.localScale = Vector3.one;

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text == null) continue;

                var o = entry.Outcome;
                text.text =
                    $"{entry.Order}) {o.Compound.Name} + {o.Solvent.Name} → " +
                    $"{o.SolubilityResult} ({o.MixtureType})\n" +
                    $"Litmus: {o.LitmusResult}";
            }
        }
    }
}
