using System.Collections.Generic;
using System.Linq;
namespace Domain
{
    public class ScoringService : IScoringService
    {
        public const int MaximumScore = 100;
        public const int PointsLostPerError = 15;
        public const int PointsPerStreakBlock = 5;
        public const int AnswersPerStreakBlock = 3;

        // Valores sorteados uma vez e mantidos fixos. A ordem das perguntas pode
        // mudar sem alterar a importância de uma questão entre duas partidas.
        private static readonly IReadOnlyDictionary<int, int> QuestionValues =
            new Dictionary<int, int>
            {
                { 1, 8 }, { 2, 10 }, { 3, 8 }, { 4, 9 }, { 5, 12 },
                { 6, 10 }, { 7, 9 }, { 8, 10 }, { 9, 14 }, { 10, 10 }
            };

        public int QuestionValue(int questionId) =>
            QuestionValues.TryGetValue(questionId, out int value) ? value : 0;

        public int CorrectStreakBonus(int consecutiveCorrectAnswers) =>
            consecutiveCorrectAnswers > 0 && consecutiveCorrectAnswers % AnswersPerStreakBlock == 0
                ? PointsPerStreakBlock
                : 0;

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
