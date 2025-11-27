namespace Domain
{
    /// <summary>
    /// Chave para acessar uma Solution por (compoundId, solventId).
    /// </summary>
    public readonly struct SolutionKey
    {
        public int CompoundId { get; }
        public int SolventId { get; }

        public SolutionKey(int compoundId, int solventId)
        {
            CompoundId = compoundId;
            SolventId = solventId;
        }

        public override int GetHashCode() =>
            (CompoundId * 397) ^ SolventId;

        public override bool Equals(object obj) =>
            obj is SolutionKey other &&
            other.CompoundId == CompoundId &&
            other.SolventId == SolventId;
    }
}
