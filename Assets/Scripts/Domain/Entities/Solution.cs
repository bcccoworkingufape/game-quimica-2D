namespace Domain
{
    /// <summary>
    /// Linha da tabela de solubilidade: resultado de compound+solvent.
    /// </summary>
    public class Solution
    {
        public int Id { get; }
        public int CompoundId { get; }
        public int SolventId { get; }
        public string Name { get; }
        public SolubilityResultKind SolubilityResult { get; }
        public LitmusResultKind LitmusResult { get; }

        public Solution(
            int id,
            int compoundId,
            int solventId,
            string name,
            SolubilityResultKind solubilityResult,
            LitmusResultKind litmusResult)
        {
            Id = id;
            CompoundId = compoundId;
            SolventId = solventId;
            Name = name;
            SolubilityResult = solubilityResult;
            LitmusResult = litmusResult;
        }

        public override string ToString() =>
            $"{Name} ({SolubilityResult}, litmus: {LitmusResult})";
    }
}
