using UnityEngine;
using Domain;
using Core;
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
    public class MixingRoundController : MonoBehaviour
    {
        // A View chega por composicao explicita (LabUIController.Awake), nao
        // por campo serializado: assim este orquestrador nao conhece o tipo
        // concreto do MonoBehaviour de UI.
        private IMixingView _view;

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
        private IAudioService _audio;
        private GameManager _gameManager;
        private TestMessageHandler _messageHandler = new TestMessageHandler();

        /// <summary>
        /// Chamado por <see cref="LabScripts.LabUIController"/> no Awake dele.
        /// Nenhum uso da View ocorre em Awake/OnEnable/Start deste componente,
        /// entao a ordem relativa entre os Awake nao importa.
        /// </summary>
        public void SetView(IMixingView view)
        {
            _view = view;
        }

        private void Start()
        {
            _solubilityService = ServiceLocator.Resolve<ISolubilityService>();
            _historyService = ServiceLocator.Resolve<IHistoryService>();
            // Fallback identico ao ja usado em LabUIController.Awake: a ausencia da
            // porta nao pode transformar audio silencioso em NullReferenceException.
            _audio = ServiceLocator.TryResolve<IAudioService>(out var a)
                ? a : new UnityAudioService();
            _gameManager = GameManager.Instance;

            if (_solubilityService == null)
                Debug.LogError("[MixingRoundController] ISolubilityService não resolvido.");
            if (_historyService == null)
                Debug.LogError("[MixingRoundController] IHistoryService não resolvido.");
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

            // A identidade quimica vem do dominio; a View so recebe a decisao.
            bool isIndicator = solventId == Solvent.LitmusIndicatorId;
            _view?.ShowConfirmationPanel(solventName, isIndicator);
        }

        /// <summary>
        /// Chamado pelo LabUIController quando o jogador clica em "Sim" no popup.
        /// </summary>
        public void OnConfirmMix()
        {
            if (_solubilityService == null || _historyService == null)
            {
                Debug.LogError("[MixingRoundController] Serviços não inicializados (solubility/history).");
                return;
            }

            if (currentCompoundId <= 0 || _selectedSolventId <= 0)
            {
                Debug.LogWarning($"[MixingRoundController] Composto ou solvente não configurados. compound={currentCompoundId}, solvent={_selectedSolventId}");
                return;
            }

            _audio?.PlayBottleFill();
            _audio?.PlayMix();

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
                Debug.LogWarning("[MixingRoundController] FlaskAnimationController não atribuído. Animação não será atualizada.");

            _view?.SetSolutionResultText(_messageHandler.GetTestMessage(response));
            _view?.ShowSolutionAnimationPanel();
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
