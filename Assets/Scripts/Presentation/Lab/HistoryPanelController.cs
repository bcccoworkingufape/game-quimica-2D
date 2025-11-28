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

        private void Awake()
        {
            _historyService = ServiceLocator.Resolve<IHistoryService>();
        }

        /// <summary>
        /// Atualiza a lista de histórico na UI.
        /// </summary>
        public void RefreshHistory()
        {
            // Limpa itens antigos
            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            var entries = _historyService.GetAll();

            foreach (var entry in entries)
            {
                var go = Instantiate(historyItemPrefab, listContainer);
                var text = go.GetComponentInChildren<TextMeshProUGUI>();

                if (text != null)
                {
                    var o = entry.Outcome;
                    text.text =
                        $"{entry.Order}) {o.Compound.Name} + {o.Solvent.Name} → " +
                        $"{o.SolubilityResult} ({o.MixtureType}) / Litmus: {o.LitmusResult}";
                }
            }
        }
    }
}
