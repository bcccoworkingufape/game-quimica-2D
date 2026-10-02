using System;
using System.Linq;
using Domain;
using Core;

internal static class ScoringTests
{
    private static readonly ScoringService Rules = new ScoringService();
    private static int checks;

    private static void Equal<T>(T expected, T actual, string message)
    {
        checks++;
        if (!Equals(expected, actual))
            throw new Exception(message + ": expected " + expected + ", got " + actual);
    }

    private static ScoringRun New(GameMode mode = GameMode.Experimentos)
    {
        var run = new ScoringRun(mode);
        run.SetQuestions(Enumerable.Range(1, 10), Rules);
        return run;
    }

    private static void Correct(ScoringRun run, int id) =>
        Equal(true, run.RecordAnswer(id, true, Rules), "correct answer accepted");

    private static void Wrong(ScoringRun run, int id) =>
        Equal(true, run.RecordAnswer(id, false, Rules), "wrong answer accepted");

    private static void SolveRemaining(ScoringRun run)
    {
        for (int id = 1; id <= 10; id++)
            run.RecordAnswer(id, true, Rules);
        Equal(true, run.Complete(Rules), "phase completed");
    }

    public static int Main()
    {
        try
        {
            int[] values = { 8, 10, 8, 9, 12, 10, 9, 10, 14, 10 };
            for (int id = 1; id <= values.Length; id++)
                Equal(values[id - 1], Rules.QuestionValue(id), "fixed value Q" + id);
            Equal(100, values.Sum(), "question values total 100");
            Equal(0, Rules.QuestionValue(99), "unknown question has no value");
            Equal(15, Rules.ErrorPenalty(), "error penalty");
            Equal(0, Rules.CorrectStreakBonus(1), "first answer no bonus");
            Equal(0, Rules.CorrectStreakBonus(2), "second answer no bonus");
            Equal(5, Rules.CorrectStreakBonus(3), "third answer bonus");
            Equal(0, Rules.CorrectStreakBonus(4), "fourth answer no additional bonus");
            Equal(5, Rules.CorrectStreakBonus(6), "sixth answer completes another block");

            var perfect = New();
            Equal(100, perfect.Maximum, "phase maximum");
            SolveRemaining(perfect);
            Equal(100, perfect.QuestionPointsEarned, "perfect question points");
            Equal(15, perfect.StreakBonus, "three complete streak blocks");
            Equal(100, perfect.Score, "display score capped at 100");
            Equal(5, perfect.Stars, "perfect five stars");

            // Example requested: misses the two most valuable questions.
            var example = New();
            foreach (int id in new[] { 1, 2, 3, 4 }) Correct(example, id);
            Wrong(example, 5); Correct(example, 5);
            foreach (int id in new[] { 6, 7, 8 }) Correct(example, id);
            Wrong(example, 9); Correct(example, 9);
            Correct(example, 10);
            Equal(true, example.Complete(Rules), "example completed");
            Equal(74, example.QuestionPointsEarned, "wrong questions do not recover their values");
            Equal(10, example.StreakBonus, "two streak blocks");
            Equal(30, example.ErrorPenaltyTotal, "two error penalties");
            Equal(54, example.Score, "example final score");
            Equal(2.5f, example.Stars, "example final stars");

            // Com duas respostas erradas, a fase concluida pode ir de duas a
            // tres estrelas. Com oito acertos de primeira tentativa, pelo
            // menos um bloco de tres acertos existe e entrega +5 de bonus.
            var minimumTwoErrorResult = New(GameMode.Desafio);
            foreach (int id in new[] { 1, 2, 3, 4 }) Correct(minimumTwoErrorResult, id);
            Wrong(minimumTwoErrorResult, 5); Correct(minimumTwoErrorResult, 5);
            Correct(minimumTwoErrorResult, 6); Correct(minimumTwoErrorResult, 7);
            Wrong(minimumTwoErrorResult, 9); Correct(minimumTwoErrorResult, 9);
            Correct(minimumTwoErrorResult, 8); Correct(minimumTwoErrorResult, 10);
            Equal(true, minimumTwoErrorResult.Complete(Rules), "minimum two-error result completes");
            Equal(49, minimumTwoErrorResult.Score, "two high-value errors with minimum streak bonus score 49");
            Equal(2f, minimumTwoErrorResult.Stars, "two high-value errors earn two stars");

            var maximumTwoErrorResult = New(GameMode.Desafio);
            foreach (int id in new[] { 2, 4, 5, 6, 7, 8, 9, 10 }) Correct(maximumTwoErrorResult, id);
            Wrong(maximumTwoErrorResult, 1); Correct(maximumTwoErrorResult, 1);
            Wrong(maximumTwoErrorResult, 3); Correct(maximumTwoErrorResult, 3);
            Equal(true, maximumTwoErrorResult.Complete(Rules), "maximum two-error result completes");
            Equal(64, maximumTwoErrorResult.Score, "two low-value errors with maximum streak bonus score 64");
            Equal(3f, maximumTwoErrorResult.Stars, "two errors never exceed three stars");

            // Corrigir uma questão errada libera a fase, porém não entra na
            // sequência nem pode conceder bônus de sequência.
            var retryBreaksStreak = New();
            Correct(retryBreaksStreak, 1);
            Correct(retryBreaksStreak, 2);
            Wrong(retryBreaksStreak, 3);
            Correct(retryBreaksStreak, 3);
            Equal(0, retryBreaksStreak.ConsecutiveCorrectAnswers, "retry resets correct streak");
            Equal(0, retryBreaksStreak.StreakBonus, "retry receives no streak bonus");
            Correct(retryBreaksStreak, 4);
            Correct(retryBreaksStreak, 5);
            Equal(2, retryBreaksStreak.ConsecutiveCorrectAnswers, "new streak starts after retry");
            Equal(0, retryBreaksStreak.StreakBonus, "new streak has no early bonus");

            // Error position alone cannot change the result when streaks match.
            var early = New();
            Wrong(early, 1); Wrong(early, 1); SolveRemaining(early);
            var late = New();
            for (int id = 2; id <= 10; id++) Correct(late, id);
            Wrong(late, 1); Wrong(late, 1); Correct(late, 1);
            Equal(true, late.Complete(Rules), "late-error run completed");
            Equal(early.Score, late.Score, "early and late equivalent streaks score equally");
            Equal(early.Stars, late.Stars, "early and late equivalent streaks classify equally");

            Equal(5, Rules.CalculateStars(100, true), "five-star boundary");
            Equal(4.5f, Rules.CalculateStars(99, true), "four-and-half upper range");
            Equal(4, Rules.CalculateStars(80, true), "four-star boundary");
            Equal(3.5f, Rules.CalculateStars(79, true), "three-and-half upper range");
            Equal(3, Rules.CalculateStars(60, true), "three-star boundary");
            Equal(2.5f, Rules.CalculateStars(59, true), "two-and-half upper range");
            Equal(2, Rules.CalculateStars(40, true), "two-star boundary");
            Equal(1.5f, Rules.CalculateStars(39, true), "one-and-half range");
            Equal(1, Rules.CalculateStars(0, true), "completed zero still receives one star");
            Equal(0, Rules.CalculateStars(100, false), "unfinished has no classification");

            var failed = New(GameMode.Desafio);
            Wrong(failed, 1); failed.RecordLifeLost(2);
            Wrong(failed, 1); failed.RecordLifeLost(1);
            Wrong(failed, 1); failed.RecordLifeLost(0);
            Equal(true, failed.Failed, "third lost life ends run");
            Equal(false, failed.Complete(Rules), "defeat cannot complete");
            Equal(0, failed.Stars, "defeat has no stars");

            var useCase = new SubmitAnswerUseCase(Rules);
            var question = new Question(5, 5, "", new[] { "A", "B" }, "A", "", "");
            var answerRun = New(GameMode.Estudo_Livre);
            Equal(0, useCase.Execute(question, "B", answerRun).EarnedPoints, "displayed score does not go below zero");
            Equal(0, useCase.Execute(question, "A", answerRun).EarnedPoints, "retry pays retained penalty but not question value");
            Equal(false, useCase.Execute(question, "A", answerRun).Accepted, "duplicate answer rejected");

            var book = new StarRecordBook();
            Equal(true, book.Update(perfect), "completed record saved");
            Equal(5, book.Best(perfect.PhaseId, perfect.Mode), "five-star record retained");

            // Every question at every position: a single mistake must stay at
            // four or four-and-a-half stars, regardless of mode or shuffle.
            foreach (GameMode mode in Enum.GetValues(typeof(GameMode)))
            for (int wrongId = 1; wrongId <= 10; wrongId++)
            for (int position = 0; position < 10; position++)
            {
                var run = New(mode);
                var order = Enumerable.Range(1, 10).Where(id => id != wrongId).ToList();
                order.Insert(position, wrongId);
                foreach (int id in order)
                {
                    if (id == wrongId) run.RecordAnswer(id, false, Rules);
                    run.RecordAnswer(id, true, Rules);
                }
                Equal(true, run.Complete(Rules), "single mistake completes");
                Equal(100 - Rules.QuestionValue(wrongId), run.QuestionPointsEarned, "retry earns zero");
                Equal(true, run.Stars == 4f || run.Stars == 4.5f, "fair single mistake rating");
                if (run.Stars == 4.5f)
                {
                    var fractionalBook = new StarRecordBook();
                    fractionalBook.Update(run);
                    Equal(4.5f, fractionalBook.Best(run.PhaseId, mode), "fractional best preserved");
                    Equal(4.5f, fractionalBook.Total(mode), "fractional total preserved");
                }
            }
            for (int boundary = 30; boundary <= 100; boundary += 10)
            {
                Equal(boundary / 20f, Rules.CalculateStars(boundary, true), "half-star threshold");
                Equal(boundary / 20f - .5f, Rules.CalculateStars(boundary - 1, true), "below threshold");
            }

            Console.WriteLine("PASS: " + checks + " scoring checks.");
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return 1;
        }
    }
}
