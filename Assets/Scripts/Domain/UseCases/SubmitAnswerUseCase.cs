using System;
namespace Domain
{
    public readonly struct SubmitAnswerResponse
    {
        public bool Accepted { get; }
        public bool IsCorrect { get; }
        public int EarnedPoints { get; }
        public SubmitAnswerResponse(bool accepted, bool correct, int points)
        { Accepted = accepted; IsCorrect = correct; EarnedPoints = points; }
    }
    public class SubmitAnswerUseCase
    {
        private readonly IScoringService _scoring;
        public SubmitAnswerUseCase(IScoringService scoring) => _scoring = scoring ?? throw new ArgumentNullException(nameof(scoring));
        public SubmitAnswerResponse Execute(Question question, string answer, ScoringRun run)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            if (run == null) throw new ArgumentNullException(nameof(run));
            bool correct = string.Equals(answer, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            int previous = run.Score;
            bool accepted = run.RecordAnswer(question.Id, correct, _scoring);
            return new SubmitAnswerResponse(accepted, correct, run.Score - previous);
        }
    }
}
