using System;
using System.Linq;
using Domain;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// Caracteriza pontuacao, validacao de resposta e regras por modo de jogo.
    /// Cobre F09, F11, F12 e a parte de dominio de F21.
    /// </summary>
    [TestFixture]
    public class GameplayRulesTests
    {
        // ---------------------------------------------------------------- F09
        [Test]
        public void F09_Scoring_CorrectAnswer_IsBasePlusStreakTimesTen()
        {
            var scoring = new ScoringService();

            Assert.AreEqual(100, scoring.ComputePoints(true, 0));
            Assert.AreEqual(110, scoring.ComputePoints(true, 1));
            Assert.AreEqual(120, scoring.ComputePoints(true, 2));
            Assert.AreEqual(150, scoring.ComputePoints(true, 5));
        }

        [Test]
        public void F09_Scoring_WrongAnswer_IsAlwaysZero()
        {
            var scoring = new ScoringService();

            foreach (var streak in new[] { 0, 1, 5, 100 })
                Assert.AreEqual(0, scoring.ComputePoints(false, streak));
        }

        [Test]
        public void F09_Scoring_BasePointsIsConfigurableButDefaultsToHundred()
        {
            Assert.AreEqual(100, new ScoringService().ComputePoints(true, 0));
            Assert.AreEqual(250, new ScoringService(250).ComputePoints(true, 0));
        }

        // ---------------------------------------------------------------- F11
        [Test]
        public void F11_SubmitAnswer_CorrectAnswer_IsCaseInsensitive()
        {
            var useCase = new SubmitAnswerUseCase(new ScoringService());
            var question = FirstQuestion();

            var response = useCase.Execute(
                new SubmitAnswerRequest(question, question.CorrectAnswer.ToUpperInvariant(), 0));

            Assert.IsTrue(response.IsCorrect, "comparacao usa OrdinalIgnoreCase");
        }

        [Test]
        public void F11_SubmitAnswer_CorrectAnswer_IncrementsStreakAndScores()
        {
            var useCase = new SubmitAnswerUseCase(new ScoringService());
            var question = FirstQuestion();

            var response = useCase.Execute(
                new SubmitAnswerRequest(question, question.CorrectAnswer, 3));

            Assert.IsTrue(response.IsCorrect);
            Assert.AreEqual(4, response.NewStreak);
            Assert.AreEqual(130, response.EarnedPoints, "100 + 3*10, com o streak ANTERIOR");
            Assert.AreEqual(question.Feedback, response.Feedback);
        }

        [Test]
        public void F11_SubmitAnswer_WrongAnswer_ResetsStreakAndReturnsHint()
        {
            var useCase = new SubmitAnswerUseCase(new ScoringService());
            var question = FirstQuestion();

            var response = useCase.Execute(
                new SubmitAnswerRequest(question, "resposta obviamente errada", 7));

            Assert.IsFalse(response.IsCorrect);
            Assert.AreEqual(0, response.NewStreak);
            Assert.AreEqual(0, response.EarnedPoints);
            StringAssert.Contains(question.Hint, response.Feedback);
            StringAssert.StartsWith("Resposta incorreta.", response.Feedback);
        }

        [Test]
        public void F11_SubmitAnswer_NullQuestion_Throws()
        {
            var useCase = new SubmitAnswerUseCase(new ScoringService());
            Assert.Throws<ArgumentNullException>(
                () => useCase.Execute(new SubmitAnswerRequest(null, "x", 0)));
        }

        [Test]
        public void F11_SubmitAnswer_NullScoringService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SubmitAnswerUseCase(null));
        }

        // -------------------------------------------------------- F12 e F21
        [Test]
        public void F12_EstudoLivre_HasNoLifePenaltyAndAllowsTree()
        {
            var rules = GameModeRulesFactory.For(GameMode.Estudo_Livre);

            Assert.AreEqual(GameMode.Estudo_Livre, rules.Mode);
            Assert.AreEqual("Estudo Livre", rules.Label);
            Assert.IsFalse(rules.AppliesLifePenalty);
            Assert.IsTrue(rules.AllowsDecisionTree);
            Assert.AreEqual("Sem penalidade", rules.LivesOverrideText);
            Assert.AreEqual(5, rules.VisibleLifeIcons(1, 5),
                "no Estudo Livre todos os icones ficam visiveis, independente das vidas");
        }

        [Test]
        public void F12_Experimentos_AppliesPenaltyAndAllowsTree()
        {
            var rules = GameModeRulesFactory.For(GameMode.Experimentos);

            Assert.AreEqual("Experimentos", rules.Label);
            Assert.IsTrue(rules.AppliesLifePenalty);
            Assert.IsTrue(rules.AllowsDecisionTree);
            Assert.IsNull(rules.LivesOverrideText);
            Assert.AreEqual(2, rules.VisibleLifeIcons(2, 5));
        }

        [Test]
        public void F21_Desafio_AppliesPenaltyAndBlocksTree()
        {
            var rules = GameModeRulesFactory.For(GameMode.Desafio);

            Assert.AreEqual("Desafio", rules.Label);
            Assert.IsTrue(rules.AppliesLifePenalty);
            Assert.IsFalse(rules.AllowsDecisionTree, "so o Desafio bloqueia a arvore de decisao");
            Assert.IsNull(rules.LivesOverrideText);
            Assert.AreEqual(3, rules.VisibleLifeIcons(3, 5));
        }

        [Test]
        public void F12_UnknownMode_FallsBackToEstudoLivre()
        {
            var rules = GameModeRulesFactory.For((GameMode)99);
            Assert.AreEqual(GameMode.Estudo_Livre, rules.Mode);
        }

        [Test]
        public void F12_Factory_AllocatesANewInstancePerCall()
        {
            // Registra o comportamento descrito no Decision Gate: a factory e
            // estatica e faz `new` a cada chamada. Consumidores a invocam de
            // dentro de propriedades e metodos de render.
            var a = GameModeRulesFactory.For(GameMode.Desafio);
            var b = GameModeRulesFactory.For(GameMode.Desafio);

            Assert.AreNotSame(a, b);
        }

        private static Question FirstQuestion() =>
            Dataset.Questions().ListAll().First();
    }
}
