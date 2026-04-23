using System;

namespace Domain
{
    public readonly struct SubmitAnswerRequest
    {
        public Question Question { get; }
        public string SelectedAnswer { get; }
        public int CurrentStreak { get; }

        public SubmitAnswerRequest(Question question, string selectedAnswer, int currentStreak)
        {
            Question = question;
            SelectedAnswer = selectedAnswer;
            CurrentStreak = currentStreak;
        }
    }

    public readonly struct SubmitAnswerResponse
    {
        public bool IsCorrect { get; }
        public int EarnedPoints { get; }
        public int NewStreak { get; }
        public string Feedback { get; }

        public SubmitAnswerResponse(bool isCorrect, int earnedPoints, int newStreak, string feedback)
        {
            IsCorrect = isCorrect;
            EarnedPoints = earnedPoints;
            NewStreak = newStreak;
            Feedback = feedback;
        }
    }

    /// <summary>
    /// Valida a resposta, calcula pontuação e devolve feedback.
    /// </summary>
    public class SubmitAnswerUseCase
    {
        private readonly IScoringService _scoringService;

        public SubmitAnswerUseCase(IScoringService scoringService)
        {
            _scoringService = scoringService
                ?? throw new ArgumentNullException(nameof(scoringService));
        }

        public SubmitAnswerResponse Execute(SubmitAnswerRequest request)
        {
            if (request.Question == null)
                throw new ArgumentNullException(nameof(request.Question));

            bool isCorrect = string.Equals(
                request.SelectedAnswer,
                request.Question.CorrectAnswer,
                StringComparison.OrdinalIgnoreCase);

            int points = _scoringService.ComputePoints(isCorrect, request.CurrentStreak);
            int newStreak = isCorrect ? request.CurrentStreak + 1 : 0;

            string feedback = isCorrect
                ? request.Question.Feedback
                : $"Resposta incorreta. {request.Question.Hint}";

            return new SubmitAnswerResponse(isCorrect, points, newStreak, feedback);
        }
    }
}
