namespace Domain
{
    public interface ISolubilityService
    {
        /// <summary>
        /// Retorna o resultado da mistura (para animação / feedback).
        /// </summary>
        SolubilityOutcome GetOutcome(int compoundId, int solventId);
    }
}
