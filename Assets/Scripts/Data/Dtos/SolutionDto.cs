namespace Data
{
    /// <summary>
    /// Espelha SolubilityData.json
    /// </summary>
    [System.Serializable]
    public class SolutionDto
    {
        public int id;
        public int compoundId;
        public int solventId;
        public string solutionName;
        public string solubilityResult; // "Soluble", "InsolubleFloat", "InsolubleSink"
        public string litmusResult;     // "None", "Neutral", "Acidic", "Basic"
    }
}
