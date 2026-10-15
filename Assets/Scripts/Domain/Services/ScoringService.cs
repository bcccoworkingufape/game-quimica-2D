using System.Collections.Generic;
using System.Linq;
namespace Domain
{
    public class ScoringService : IScoringService
    {
        public const int MaximumScore = 100;
        public const int BasePointsPerCorrectAnswer = 10;
        public const int PointsLostPerError = 5;
        public const int PointsPerStreakBlock = 0;
        public const int AnswersPerStreakBlock = 3;

        // Todas as questões têm a mesma recompensa base. A ordem das perguntas
        // pode mudar sem alterar a pontuação possível da fase.
        private static readonly IReadOnlyDictionary<int, int> QuestionValues =
            new Dictionary<int, int>
            {
                { 1, 10 }, { 2, 10 }, { 3, 10 }, { 4, 10 }, { 5, 10 },
                { 6, 10 }, { 7, 10 }, { 8, 10 }, { 9, 10 }, { 10, 10 }
            };

        public int QuestionValue(int questionId) =>
            QuestionValues.TryGetValue(questionId, out int value) ? value : 0;

        public int CorrectStreakBonus(int consecutiveCorrectAnswers) => 0;

        public int ErrorPenalty() => PointsLostPerError;

        public int ComputePoints(int questionId, bool correct, bool firstAttempt, int consecutiveCorrectAnswers) =>
            correct
                ? (firstAttempt ? QuestionValue(questionId) : 0) + CorrectStreakBonus(consecutiveCorrectAnswers)
                : -ErrorPenalty();

        public int MaximumPoints(IEnumerable<int> questionIds) =>
            System.Math.Min(MaximumScore, questionIds?.Distinct().Sum(QuestionValue) ?? 0);

        public float CalculateStars(int points, bool completed)
        {
            if (!completed) return 0;
            int score = System.Math.Max(0, System.Math.Min(MaximumScore, points));
            return System.Math.Max(1f, (score / 10) * 0.5f);
        }
    }
}
