using System;

namespace Domain
{
    /// <summary>
    /// Serviço de domínio que combina repositórios para montar o SolubilityOutcome.
    /// </summary>
    public class SolubilityService : ISolubilityService
    {
        private readonly ICompoundRepository _compounds;
        private readonly ISolventRepository _solvents;
        private readonly ISolutionRepository _solutions;

        public SolubilityService(
            ICompoundRepository compounds,
            ISolventRepository solvents,
            ISolutionRepository solutions)
        {
            _compounds = compounds ?? throw new ArgumentNullException(nameof(compounds));
            _solvents = solvents ?? throw new ArgumentNullException(nameof(solvents));
            _solutions = solutions ?? throw new ArgumentNullException(nameof(solutions));
        }

        public SolubilityOutcome GetOutcome(int compoundId, int solventId)
        {
            var compound = _compounds.GetById(compoundId);
            var solvent = _solvents.GetById(solventId);
            var solution = _solutions.GetByIds(compoundId, solventId);

            if (compound == null || solvent == null || solution == null)
            {
                throw new InvalidOperationException(
                    $"Dados de solubilidade não encontrados para compound={compoundId}, solvent={solventId}.");
            }

            // Regra simples: LL/SL derivado dos estados
            var mixtureType = (compound.State == PhysicalState.SOLID &&
                               solvent.State == PhysicalState.LIQUID)
                ? MixtureType.SL
                : MixtureType.LL;

            return new SolubilityOutcome(
                compound,
                solvent,
                mixtureType,
                solution.SolubilityResult,
                solution.LitmusResult);
        }
    }
}
