using System.Collections.Generic;

namespace Data
{
    /// <summary>Espelha o JSON de questões (Resources/Data/QuestionsData.json).</summary>
    [System.Serializable]
    public class QuestionDto
    {
        public int id;
        public int compoundId;
        public string description;
        public string correctAnswer;
        public List<string> alternatives;
        public string hint;
        public string feedback;
    }
}
