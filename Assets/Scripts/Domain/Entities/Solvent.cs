namespace Domain
{
    /// <summary>
    /// Solvente usado nas misturas (água, éter, NaOH, etc.).
    /// </summary>
    public class Solvent
    {
        public int Id { get; }
        public string Name { get; }
        public PhysicalState State { get; }
        public float MeltingPoint { get; }
        public float BoilingPoint { get; }
        public FlaskType FlaskType { get; }

        public string ChemicalClass { get; }

        /// <summary>
        /// Id do reagente indicador acido-base no dataset congelado do experimento
        /// (SolventsData.json, "Tornassol"). O dataset nao carrega um campo de
        /// papel quimico, entao a identidade estavel disponivel e o id.
        /// </summary>
        public const int LitmusIndicatorId = 3;

        /// <summary>
        /// Se este solvente e o indicador acido-base. Antes da Etapa F1 a
        /// resposta era obtida comparando o nome de exibicao em dois pontos
        /// independentes da Presentation; agora existe um unico dono.
        /// </summary>
        public bool IsIndicator => Id == LitmusIndicatorId;

        public Solvent(
            int id,
            string name,
            PhysicalState state,
            float meltingPoint,
            float boilingPoint,
            FlaskType flaskType,
            string chemicalClass)
        {
            Id = id;
            Name = name;
            State = state;
            MeltingPoint = meltingPoint;
            BoilingPoint = boilingPoint;
            FlaskType = flaskType;
            ChemicalClass = chemicalClass;
        }

        public override string ToString() => $"{Id}: {Name} ({FlaskType})";
    }
}
