using System.Collections.Generic;
using System.Linq;
namespace Domain
{
    /// <summary>A phase is the complete question set. Owned by GameManager across scene reloads.</summary>
    public sealed class ScoringRun
    {
        private readonly HashSet<int> _questions = new HashSet<int>();
        private readonly HashSet<int> _attempted = new HashSet<int>();
        private readonly HashSet<int> _solved = new HashSet<int>();
        public GameMode Mode { get; }
        private int _balance;
        public int Score => System.Math.Max(0, System.Math.Min(ScoringService.MaximumScore, _balance));
        public int CorrectAnswers => _solved.Count;
        public bool Failed { get; private set; }
        public int Errors { get; private set; }
        public int ConsecutiveErrors { get; private set; }
        public int ConsecutiveCorrectAnswers { get; private set; }
        public int LivesLost { get; private set; }
        public int FirstAttemptCorrect { get; private set; }
        public int StreakBonus { get; private set; }
        public int ErrorPenaltyTotal { get; private set; }
        public int QuestionPointsEarned { get; private set; }
        public int Bonus { get; private set; }
        public int TotalBonus => StreakBonus + Bonus;
        public float Stars { get; private set; }
        public int Maximum { get; private set; }
        public bool Completed { get; private set; }
        public string PhaseId => string.Join("-", _questions.OrderBy(id => id));

        public ScoringRun(GameMode mode) { Mode = mode; }
        public void SetQuestions(IEnumerable<int> ids, IScoringService scoring)
        {
            if (_questions.Count > 0) return;
            foreach (int id in ids) if (id > 0) _questions.Add(id);
            Maximum = scoring.MaximumPoints(_questions);
        }
        public bool RecordAnswer(int questionId, bool correct, IScoringService scoring)
        {
            if (Completed || Failed || !_questions.Contains(questionId) || _solved.Contains(questionId)) return false;
            bool first = _attempted.Add(questionId);
            if (correct)
            {
                _solved.Add(questionId);
                ConsecutiveErrors = 0;
                if (first)
                {
                    FirstAttemptCorrect++;
                    ConsecutiveCorrectAnswers++;
                    QuestionPointsEarned += scoring.QuestionValue(questionId);
                    StreakBonus += scoring.CorrectStreakBonus(ConsecutiveCorrectAnswers);
                }
                else
                {
                    // Uma questão corrigida permite avançar, mas não reativa a
                    // sequência: só acertos de primeira tentativa a constroem.
                    ConsecutiveCorrectAnswers = 0;
                }
            }
            else
            {
                Errors++;
                ConsecutiveErrors++;
                ConsecutiveCorrectAnswers = 0;
                ErrorPenaltyTotal += scoring.ErrorPenalty();
            }
            _balance += scoring.ComputePoints(
                questionId,
                correct,
                first,
                first ? ConsecutiveCorrectAnswers : 0);
            return true;
        }
        public void RecordLifeLost(int remainingLives)
        {
            if (Completed || Failed) return;
            LivesLost++;
            if (remainingLives <= 0) Failed = true;
        }
        public bool Complete(IScoringService scoring)
        {
            if (Completed || Failed || _questions.Count == 0 || _solved.Count != _questions.Count) return false;
            Bonus = 0;
            Completed = true;
            Stars = scoring.CalculateStars(Score, true);
            return true;
        }
    }
}

