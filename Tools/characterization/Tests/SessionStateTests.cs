using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Domain;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// Caracteriza historico de misturas, progressao do run e o caso de uso de
    /// mistura. Cobre F14, F15 e a orquestracao coberta por F07/F15.
    /// </summary>
    [TestFixture]
    public class SessionStateTests
    {
        // ------------------------------------------------- F15 · historico
        [Test]
        public void F15_History_OrderStartsAtOneAndIncrements()
        {
            var history = new InMemoryHistoryService();
            var service = Dataset.SolubilityService();

            var a = history.Register(service.GetOutcome(1, 1));
            var b = history.Register(service.GetOutcome(1, 2));
            var c = history.Register(service.GetOutcome(2, 3));

            Assert.AreEqual(1, a.Order);
            Assert.AreEqual(2, b.Order);
            Assert.AreEqual(3, c.Order);
            Assert.AreEqual(3, history.GetAll().Count);
        }

        [Test]
        public void F15_History_Clear_ResetsBothListAndCounter()
        {
            var history = new InMemoryHistoryService();
            var service = Dataset.SolubilityService();

            history.Register(service.GetOutcome(1, 1));
            history.Register(service.GetOutcome(1, 2));
            history.Clear();

            Assert.AreEqual(0, history.GetAll().Count);

            var afterClear = history.Register(service.GetOutcome(1, 3));
            Assert.AreEqual(1, afterClear.Order, "o contador volta para 1, nao continua de 3");
        }

        [Test]
        public void F15_History_PreservesOutcomeIdentity()
        {
            var history = new InMemoryHistoryService();
            var outcome = Dataset.SolubilityService().GetOutcome(3, 3);

            var entry = history.Register(outcome);

            Assert.AreSame(outcome, entry.Outcome);
            Assert.AreEqual(LitmusResultKind.Acidic, entry.Outcome.LitmusResult);
        }

        [Test]
        public void F15_History_NullOutcome_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new InMemoryHistoryService().Register(null));
        }

        // ------------------------------------------------ F14 · progressao
        [Test]
        public void F14_Progress_IsZeroBeforeTotalIsSet()
        {
            var tracker = new QuestionRunTracker();
            Assert.AreEqual(0, tracker.GetProgressPercentage());
        }

        [Test]
        public void F14_Progress_RoundsToNearestInteger()
        {
            var tracker = new QuestionRunTracker();
            tracker.SetTotal(3);

            tracker.SetActive(1);
            tracker.Commit();
            // 1/3 = 33,33% -> 33
            Assert.AreEqual(33, tracker.GetProgressPercentage());

            tracker.SetActive(2);
            tracker.Commit();
            // 2/3 = 66,67% -> 67
            Assert.AreEqual(67, tracker.GetProgressPercentage());
        }

        [Test]
        public void F14_RunCleared_FiresOnlyWhenAllQuestionsAreCommitted()
        {
            var tracker = new QuestionRunTracker();
            var clearedEvents = 0;
            tracker.OnRunCleared += () => clearedEvents++;
            tracker.SetTotal(10);

            for (var id = 1; id <= 9; id++)
            {
                tracker.SetActive(id);
                Assert.IsFalse(tracker.Commit(), "questao " + id + " nao encerra o run");
            }

            Assert.AreEqual(0, clearedEvents);

            tracker.SetActive(10);
            Assert.IsTrue(tracker.Commit(), "a decima encerra o run");
            Assert.AreEqual(1, clearedEvents);
            Assert.AreEqual(100, tracker.GetProgressPercentage());
        }

        [Test]
        public void F14_CommitIsIdempotentForTheSameQuestionId()
        {
            // O conjunto e um HashSet: commitar o mesmo id duas vezes nao avanca.
            var tracker = new QuestionRunTracker();
            tracker.SetTotal(10);

            tracker.SetActive(1);
            tracker.Commit();
            tracker.SetActive(1);
            tracker.Commit();

            Assert.AreEqual(1, tracker.CompletedCount);
            Assert.AreEqual(10, tracker.GetProgressPercentage());
        }

        [Test]
        public void F14_CommitWithoutActiveQuestion_DoesNotCount()
        {
            var tracker = new QuestionRunTracker();
            tracker.SetTotal(5);

            tracker.Commit(); // ActiveQuestionId == 0

            Assert.AreEqual(0, tracker.CompletedCount);
        }

        [Test]
        public void F14_CommitClearsActiveQuestionAndCorrectFlag()
        {
            var tracker = new QuestionRunTracker();
            tracker.SetTotal(5);
            tracker.SetActive(2);
            tracker.MarkActiveCorrect();

            Assert.AreEqual(2, tracker.ActiveQuestionId);
            Assert.IsTrue(tracker.ActiveQuestionAnsweredCorrect);

            tracker.Commit();

            Assert.AreEqual(0, tracker.ActiveQuestionId);
            Assert.IsFalse(tracker.ActiveQuestionAnsweredCorrect);
            Assert.IsTrue(tracker.IsCompleted(2));
        }

        [Test]
        public void F14_Reset_ClearsEverythingAndEmitsZero()
        {
            var tracker = new QuestionRunTracker();
            var lastProgress = -1;
            tracker.SetTotal(4);
            tracker.SetActive(1);
            tracker.Commit();

            tracker.OnProgressChanged += p => lastProgress = p;
            tracker.Reset();

            Assert.AreEqual(0, tracker.CompletedCount);
            Assert.AreEqual(0, tracker.TotalQuestionsInRun);
            Assert.AreEqual(0, tracker.ActiveQuestionId);
            Assert.AreEqual(0, lastProgress);
        }

        [Test]
        public void F14_SetTotal_ClampsNegativeToZero()
        {
            var tracker = new QuestionRunTracker();
            tracker.SetTotal(-5);
            Assert.AreEqual(0, tracker.TotalQuestionsInRun);
        }

        // ------------------------------------ MixSolutionUseCase · F07 + F15
        [Test]
        public void MixSolution_ReturnsOutcomeAndRegistersHistoryInOneCall()
        {
            var history = new InMemoryHistoryService();
            var useCase = new MixSolutionUseCase(Dataset.SolubilityService(), history);

            var response = useCase.Execute(new MixSolutionRequest(1, 1));

            Assert.IsNotNull(response.Outcome);
            Assert.AreEqual(1, response.Outcome.Compound.Id);
            Assert.AreEqual(1, response.Outcome.Solvent.Id);

            Assert.IsNotNull(response.HistoryEntry);
            Assert.AreEqual(1, response.HistoryEntry.Order);
            Assert.AreEqual(1, history.GetAll().Count);
            Assert.AreSame(response.Outcome, response.HistoryEntry.Outcome);
        }

        [Test]
        public void MixSolution_EveryMixIsRecorded()
        {
            var history = new InMemoryHistoryService();
            var useCase = new MixSolutionUseCase(Dataset.SolubilityService(), history);

            useCase.Execute(new MixSolutionRequest(1, 1));
            useCase.Execute(new MixSolutionRequest(1, 2));
            useCase.Execute(new MixSolutionRequest(1, 3));

            Assert.AreEqual(3, history.GetAll().Count);
            CollectionAssert.AreEqual(
                new[] { 1, 2, 3 },
                history.GetAll().Select(e => e.Order).ToArray());
        }

        [Test]
        public void MixSolution_InvalidPair_PropagatesAndRecordsNothing()
        {
            var history = new InMemoryHistoryService();
            var useCase = new MixSolutionUseCase(Dataset.SolubilityService(), history);

            Assert.Throws<InvalidOperationException>(
                () => useCase.Execute(new MixSolutionRequest(99, 1)));

            Assert.AreEqual(0, history.GetAll().Count,
                "a excecao vem do servico antes do Register");
        }
    }
}
