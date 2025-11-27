namespace Data
{
    /// <summary>
    /// Espelha SolventsData.json
    /// </summary>
    [System.Serializable]
    public class SolventDto
    {
        public int id;
        public string name;
        public string state;
        public float meltingPoint;
        public float boilingPoint;
        public string flaskType; // "FLASK_01" etc.
    }
}
