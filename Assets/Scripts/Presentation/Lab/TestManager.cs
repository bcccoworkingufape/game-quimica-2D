using UnityEngine;
using Domain;
using Core;
using LabScripts;
using Core.Audio;
namespace Presentation.Lab
{
    /// <summary>
    /// Orquestra o fluxo da fase de laboratório:
    /// - recebe o composto misterioso atual
    /// - é avisado quando um solvente é clicado
    /// - confirma a mistura (botão "Sim" do popup)
    /// - chama o caso de uso de solubilidade e atualiza a UI/animação
    /// </summary>
    public class TestManager : MonoBehaviour
    {
        [Header("Referências de UI")]
        [SerializeField] private LabUIController uiController;

        [Header("Referência de Animação")]
        [Tooltip("Controller que atualiza os parâmetros do Animator do frasco")]
        [SerializeField] private FlaskAnimationController flaskAnimationController;

        [Header("Questão atual")]
        [Tooltip("ID do composto 'misterioso' que o jogador precisa descobrir.")]
        [SerializeField] private int currentCompoundId;

        public int CurrentCompoundId => currentCompoundId;

        private int _selectedSolventId = -1;
        private string _selectedSolventName;

        // Serviços de domínio
        private ISolubilityService _solubilityService;
        private IHistoryService _historyService;
        private GameManager _gameManager;
        private TestMessageHandler _messageHandler = new TestMessageHandler();

        private void Awake()
        {
            if (uiController == null)
            {
                uiController = FindFirstObjectByType<LabUIController>();
            }
        }

        private void Start()
        {
            _solubilityService = ServiceLocator.Resolve<ISolubilityService>();
            _historyService = ServiceLocator.Resolve<IHistoryService>();
            _gameManager = GameManager.Instance;

            if (_solubilityService == null)
                Debug.LogError("[TestManager] ISolubilityService não resolvido.");
            if (_historyService == null)
                Debug.LogError("[TestManager] IHistoryService não resolvido.");
        }

        /// <summary>
        /// Chamado quando uma nova pergunta (novo composto misterioso) é iniciada.
        /// </summary>
        public void SetCurrentCompound(int compoundId)
        {
            currentCompoundId = compoundId;
        }


        /// <summary>
        /// Chamado pelo script de clique do solvente (SolventClickHandler).
        /// </summary>
        public void OnSolventClicked(int solventId, string solventName)
        {
            _selectedSolventId = solventId;
            _selectedSolventName = solventName;

            if (uiController != null)
            {
                uiController.ShowConfirmationPanel(solventName);
            }
        }

        /// <summary>
        /// Chamado pelo LabUIController quando o jogador clica em "Sim" no popup.
        /// </summary>
        public void OnConfirmMix()
        {
            if (_solubilityService == null || _historyService == null)
            {
                Debug.LogError("[TestManager] Serviços não inicializados (solubility/history).");
                return;
            }

            if (currentCompoundId <= 0 || _selectedSolventId <= 0)
            {
                Debug.LogWarning($"[TestManager] Composto ou solvente não configurados. compound={currentCompoundId}, solvent={_selectedSolventId}");
                return;
            }

            SfxManager.Instance?.PlayBottleFill();
            SfxManager.Instance?.PlayMix();

            var useCase = new MixSolutionUseCase(_solubilityService, _historyService);
            var request = new MixSolutionRequest(currentCompoundId, _selectedSolventId);
            var response = useCase.Execute(request);
            var outcome = response.Outcome;

            Debug.Log($"[MIX] compoundId={outcome.Compound.Id}, solventId={outcome.Solvent.Id}, " +
                      $"mixtureType={outcome.MixtureType}, solubility={outcome.SolubilityResult}, " +
                      $"litmus={outcome.LitmusResult}, flask={outcome.FlaskType}");

            if (flaskAnimationController != null)
                flaskAnimationController.SetAnimationFromOutcome(outcome);
            else
                Debug.LogWarning("[TestManager] FlaskAnimationController não atribuído. Animação não será atualizada.");

            if (uiController != null && uiController.solutionAnimationText != null)
            {
                uiController.solutionAnimationText.text =
                    _messageHandler.GetTestMessage(response);
            }

            uiController?.ShowSolutionAnimationPanel();
        }
        public void ResetRoundState(bool clearCompound = false)
        {
            if (clearCompound)
                currentCompoundId = 0;

            _selectedSolventId = -1;
            _selectedSolventName = null;
        }


    }
}
