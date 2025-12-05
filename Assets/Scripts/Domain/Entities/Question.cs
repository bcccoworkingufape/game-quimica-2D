using System.Collections.Generic;

namespace Domain
{
    /// <summary>
    /// Pergunta de múltipla escolha associada a um composto orgânico.
    /// </summary>
    public class Question
    {
        public int Id { get; }
        /// <summary>
        /// Id do composto (CompoundsData.json) que o jogador deve descobrir.
        /// </summary>
        public int CompoundId { get; }

        public string Description { get; }        // pode ficar invisível na UI por enquanto
        public string CorrectAnswer { get; }
        public IReadOnlyList<string> Alternatives { get; }
        public string Hint { get; }
        public string Feedback { get; }

        public Question(
            int id,
            int compoundId,
            string description,
            IList<string> alternatives,
            string correctAnswer,
            string hint,
            string feedback)
        {
            Id = id;
            CompoundId = compoundId;
            Description = description ?? string.Empty;
            CorrectAnswer = correctAnswer ?? string.Empty;
            Hint = hint ?? string.Empty;
            Feedback = feedback ?? string.Empty;

            // Garante que sempre tenha pelo menos uma alternativa
            if (alternatives == null || alternatives.Count == 0)
            {
                Alternatives = new List<string> { CorrectAnswer };
            }
            else
            {
                Alternatives = new List<string>(alternatives);
            }
        }
    }
}
