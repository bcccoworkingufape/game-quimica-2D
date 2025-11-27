namespace Data
{
    /// <summary>
    /// Espelha CompoundsData.json
    /// </summary>
    [System.Serializable]
    public class CompoundDto
    {
        public int id;
        public string name;
        public string state;      // "LIQUID" | "SOLID"
        public string group;      // S1, S2, SA...
        public float density;
        public float meltingPoint;
        public float boilingPoint;
    }
}
