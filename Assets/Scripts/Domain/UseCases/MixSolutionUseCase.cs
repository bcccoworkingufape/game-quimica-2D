namespace Domain
{
    /// <summary>
    /// Dados de entrada: composto misterioso + solvente clicado.
    /// </summary>
    public readonly struct MixSolutionRequest
    {
        public int CompoundId { get; }
        public int SolventId { get; }

        public MixSolutionRequest(int compoundId, int solventId)
        {
            CompoundId = compoundId;
            SolventId = solventId;
        }
    }

    /// <summary>
    /// Saída: resultado de solubilidade + entrada no histórico.
    /// </summary>
    public readonly struct MixSolutionResponse
    {
        public SolubilityOutcome Outcome { get; }
        public MixtureHistoryEntry HistoryEntry { get; }

        public MixSolutionResponse(SolubilityOutcome outcome, MixtureHistoryEntry historyEntry)
        {
            Outcome = outcome;
            HistoryEntry = historyEntry;
        }
    }

    /// <summary>
    /// Caso de uso: mistura o composto "misterioso" com um solvente
    /// (consulta banco/cache) e registra no histórico.
    /// </summary>
    public class MixSolutionUseCase
    {
        private readonly ISolubilityService _solubilityService;
        private readonly IHistoryService _historyService;

        public MixSolutionUseCase(
            ISolubilityService solubilityService,
            IHistoryService historyService)
        {
            _solubilityService = solubilityService;
            _historyService = historyService;
        }

        public MixSolutionResponse Execute(MixSolutionRequest request)
        {
            // 1) consulta serviço de solubilidade (usa repositórios com cache)
            var outcome = _solubilityService.GetOutcome(
                request.CompoundId,
                request.SolventId);

            // 2) registra no histórico
            var entry = _historyService.Register(outcome);

            // 3) devolve pra camada de apresentação
            return new MixSolutionResponse(outcome, entry);
        }
    }
}
