namespace Domain
{
    /// <summary>
    /// Contrato comum para substâncias químicas (composto ou solvente).
    /// </summary>
    public interface IChemicalSubstance
    {
        int Id { get; }
        string Name { get; }
        PhysicalState State { get; }
        float MeltingPoint { get; }
        float BoilingPoint { get; }
    }
}
