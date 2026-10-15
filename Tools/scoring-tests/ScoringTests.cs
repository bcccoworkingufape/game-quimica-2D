using System;
using System.Linq;
using Domain;

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

    private static void Finish(ScoringRun run)
    {
        for (int id = 1; id <= 10; id++) run.RecordAnswer(id, true, Rules);
        Equal(true, run.Complete(Rules), "phase completed");
    }

    public static int Main()
    {
        try
        {
            for (int id = 1; id <= 10; id++)
                Equal(10, Rules.QuestionValue(id), "standard question reward");
            Equal(0, Rules.QuestionValue(99), "unknown question has no value");
            Equal(5, Rules.ErrorPenalty(), "base error penalty");
            for (int streak = 1; streak <= 10; streak++)
                Equal(0, Rules.CorrectStreakBonus(streak), "standard reward has no streak bonus");

            var perfect = New();
            Finish(perfect);
            Equal(100, perfect.Maximum, "phase maximum");
            Equal(100, perfect.Score, "perfect score");
            Equal(5f, perfect.Stars, "perfect five stars");

            var penalties = New();
            Wrong(penalties, 1);
            Equal(5, penalties.ErrorPenaltyTotal, "first consecutive error loses five");
            Wrong(penalties, 1);
            Equal(15, penalties.ErrorPenaltyTotal, "second consecutive error loses ten");
            Wrong(penalties, 1);
            Equal(30, penalties.ErrorPenaltyTotal, "third consecutive error loses fifteen");
            Correct(penalties, 1);
            Equal(0, penalties.ConsecutiveErrors, "retry resets error sequence");
            Equal(0, penalties.ConsecutiveCorrectAnswers, "retry does not count in correct sequence");
            Equal(0, penalties.QuestionPointsEarned, "retry earns no points");
            Wrong(penalties, 2);
            Equal(35, penalties.ErrorPenaltyTotal, "new sequence starts at five");
            Correct(penalties, 2);
            Finish(penalties);
            Equal(45, penalties.Score, "retained penalties reduce future rewards");
            Equal(2f, penalties.Stars, "retained penalties affect final rating");

            var early = New();
            Wrong(early, 1);
            Correct(early, 1);
            Finish(early);
            var late = New();
            for (int id = 2; id <= 10; id++) Correct(late, id);
            Wrong(late, 1);
            Correct(late, 1);
            Equal(true, late.Complete(Rules), "late error phase completed");
            Equal(85, early.Score, "one mistaken question loses reward and five points");
            Equal(early.Score, late.Score, "order does not discard penalties");

            foreach (GameMode mode in Enum.GetValues(typeof(GameMode)))
            {
                bool scored = mode != GameMode.Estudo_Livre;
                Equal(scored, GameModeRulesFactory.For(mode).UsesScoring, "mode scoring policy");
                for (int mask = 0; mask < (1 << 10); mask++)
                {
                    var run = New(mode);
                    int mistakes = 0;
                    for (int id = 1; id <= 10; id++)
                    {
                        if ((mask & (1 << (id - 1))) != 0)
                        {
                            run.RecordAnswer(id, false, Rules);
                            mistakes++;
                        }
                        run.RecordAnswer(id, true, Rules);
                    }
                    Equal(true, run.Complete(Rules), "all question patterns can complete without life exhaustion");
                    int expected = scored ? Math.Max(0, 100 - mistakes * 15) : 0;
                    Equal(expected, run.Score, "fixed rewards and independent mistake penalties");
                    Equal(scored ? 100 - mistakes * 10 : 0, run.QuestionPointsEarned, "retry never recovers points");
                    Equal(0, run.StreakBonus, "no extra streak points");
                    Equal(scored ? Rules.CalculateStars(expected, true) : 0f, run.Stars, "mode classification");
                    Equal(false, run.RecordAnswer(1, true, Rules), "completed run rejects duplicate");
                }
            }

            foreach (var mode in new[] { GameMode.Experimentos, GameMode.Desafio })
            {
                int lives = mode == GameMode.Experimentos ? 4 : 3;
                var failed = New(mode);
                for (int i = 1; i <= lives; i++)
                {
                    Wrong(failed, 1);
                    failed.RecordLifeLost(lives - i);
                    Equal(i == lives, failed.Failed, "only zero remaining lives fails");
                }
                Equal(false, failed.Complete(Rules), "failed phase cannot complete");
                Equal(false, failed.RecordAnswer(1, true, Rules), "failed phase rejects answers");
                Equal(0f, failed.Stars, "defeat has no stars");
            }

            var study = New(GameMode.Estudo_Livre);
            for (int i = 0; i < 100; i++) Wrong(study, 1);
            Finish(study);
            Equal(0, study.Score, "study remains unscored");
            Equal(0f, study.Stars, "study has no classification");
            Equal(false, new StarRecordBook().Update(study), "study does not save a ranking");
            var records = new StarRecordBook();
            Equal(true, records.Update(perfect), "scored completed record saved");
            Equal(5f, records.Best(perfect.PhaseId, perfect.Mode), "record retains stars");

            for (int boundary = 30; boundary <= 100; boundary += 10)
            {
                Equal(boundary / 20f, Rules.CalculateStars(boundary, true), "half-star threshold");
                Equal(boundary / 20f - .5f, Rules.CalculateStars(boundary - 1, true), "below half-star threshold");
            }
            Equal(0f, Rules.CalculateStars(100, false), "unfinished phase has no classification");

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
