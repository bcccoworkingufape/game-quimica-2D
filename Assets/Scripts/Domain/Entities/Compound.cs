namespace Domain
{
    /// <summary>
    /// Composto orgânico (o “mistério” a ser descoberto).
    /// </summary>
    public class Compound : IChemicalSubstance
    {
        public int Id { get; }
        public string Name { get; }
        public AggregateState State { get; }
        public string Group { get; }
        public float Density { get; }
        public float MeltingPoint { get; }
        public float BoilingPoint { get; }

        public Compound(
            int id,
            string name,
            AggregateState state,
            string group,
            float density,
            float meltingPoint,
            float boilingPoint)
        {
            Id = id;
            Name = name;
            State = state;
            Group = group;
            Density = density;
            MeltingPoint = meltingPoint;
            BoilingPoint = boilingPoint;
        }

        public override string ToString() => $"{Id}: {Name} ({Group})";
    }

}
