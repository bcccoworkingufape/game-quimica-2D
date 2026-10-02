using System.Collections.Generic;

namespace Domain
{
    public interface IScoringService
    {
        int QuestionValue(int questionId);
        int CorrectStreakBonus(int consecutiveCorrectAnswers);
        int ErrorPenalty();
        int ComputePoints(int questionId, bool correct, bool firstAttempt, int consecutiveCorrectAnswers);
        int MaximumPoints(IEnumerable<int> questionIds);
        float CalculateStars(int points, bool completed);
    }
}

