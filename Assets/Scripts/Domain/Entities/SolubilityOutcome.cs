namespace Domain
{
    /// <summary>
    /// Objeto de saída para a View/Presenter:
    /// tudo que é necessário pra escolher animação.
    /// </summary>
    public class SolubilityOutcome
    {
        public Compound Compound { get; }
        public Solvent Solvent { get; }
        public MixtureType MixtureType { get; }
        public SolubilityResultKind SolubilityResult { get; }
        public LitmusResultKind LitmusResult { get; }
        public FlaskType FlaskType => Solvent.FlaskType;

        public SolubilityOutcome(
            Compound compound,
            Solvent solvent,
            MixtureType mixtureType,
            SolubilityResultKind solubilityResult,
            LitmusResultKind litmusResult)
        {
            Compound = compound;
            Solvent = solvent;
            MixtureType = mixtureType;
            SolubilityResult = solubilityResult;
            LitmusResult = litmusResult;
        }
    }
}
