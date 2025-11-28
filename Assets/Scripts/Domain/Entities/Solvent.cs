namespace Domain
{
    /// <summary>
    /// Solvente usado nas misturas (água, éter, NaOH, etc.).
    /// </summary>
    public class Solvent : IChemicalSubstance
    {
        public int Id { get; }
        public string Name { get; }
        public PhysicalState State { get; }
        public float MeltingPoint { get; }
        public float BoilingPoint { get; }
        public FlaskType FlaskType { get; }

        int IChemicalSubstance.Id => Id;
        string IChemicalSubstance.Name => Name;

        float IChemicalSubstance.MeltingPoint => MeltingPoint;
        float IChemicalSubstance.BoilingPoint => BoilingPoint;

        public Solvent(
            int id,
            string name,
            PhysicalState state,
            float meltingPoint,
            float boilingPoint,
            FlaskType flaskType)
        {
            Id = id;
            Name = name;
            State = state;
            MeltingPoint = meltingPoint;
            BoilingPoint = boilingPoint;
            FlaskType = flaskType;
        }

        public override string ToString() => $"{Id}: {Name} ({FlaskType})";
    }
}
