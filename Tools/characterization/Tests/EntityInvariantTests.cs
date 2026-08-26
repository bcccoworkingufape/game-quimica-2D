using System.Collections.Generic;
using Domain;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// Caracteriza as invariantes que vivem DENTRO das entidades de dominio,
    /// independentes de como elas sao materializadas.
    ///
    /// Antes da Etapa B estes testes ficavam em MappingTests.cs e exercitavam
    /// os mappers. Removidos os mappers, as invariantes seguem exatamente as
    /// mesmas — porque nunca estiveram no mapper: sempre estiveram no
    /// construtor da entidade, que era o que o QuestionMapper apenas repassava.
    ///
    /// As politicas de conversao e de erro da camada Data passaram para
    /// RepositoryMaterializationTests, que as exercita pelo caminho real.
    /// </summary>
    [TestFixture]
    public class EntityInvariantTests
    {
        [Test]
        public void Question_KeepsFieldsAsGiven()
        {
            var q = new Question(
                id: 7,
                compoundId: 3,
                description: "Q7",
                alternatives: new List<string> { "A", "B", "C" },
                correctAnswer: "B",
                hint: "dica",
                feedback: "feedback");

            Assert.AreEqual(7, q.Id);
            Assert.AreEqual(3, q.CompoundId);
            Assert.AreEqual("Q7", q.Description);
            Assert.AreEqual("B", q.CorrectAnswer);
            Assert.AreEqual(3, q.Alternatives.Count);
            Assert.AreEqual("dica", q.Hint);
            Assert.AreEqual("feedback", q.Feedback);
        }

        [Test]
        public void Question_NormalizesNullsToEmpty()
        {
            // Question.cs:33-36
            var q = new Question(1, 1, null, null, null, null, null);

            Assert.AreEqual(string.Empty, q.Description);
            Assert.AreEqual(string.Empty, q.CorrectAnswer);
            Assert.AreEqual(string.Empty, q.Hint);
            Assert.AreEqual(string.Empty, q.Feedback);
            Assert.IsNotNull(q.Alternatives);
        }

        [Test]
        public void Question_EmptyAlternatives_GetsCorrectAnswerAsOnlyOption()
        {
            // Question.cs:39-46 garante pelo menos uma alternativa.
            var q = new Question(
                id: 1,
                compoundId: 1,
                description: "Q1",
                alternatives: new List<string>(),
                correctAnswer: "Resposta",
                hint: "",
                feedback: "");

            Assert.AreEqual(1, q.Alternatives.Count);
            Assert.AreEqual("Resposta", q.Alternatives[0]);
        }

        [Test]
        public void Question_CopiesAlternatives_DoesNotAliasCallerList()
        {
            // Question.cs:45 faz new List<string>(alternatives). Mutar a lista de
            // origem depois nao pode alterar a entidade.
            var origem = new List<string> { "A", "B" };
            var q = new Question(1, 1, "Q", origem, "A", "", "");

            origem.Add("C");

            Assert.AreEqual(2, q.Alternatives.Count);
        }

        [Test]
        public void SolubilityOutcome_FlaskTypeIsDerivedFromSolvent()
        {
            // SolubilityOutcome.cs:14 — propriedade derivada, nao campo proprio.
            var compound = new Compound(1, "C", PhysicalState.LIQUID, "S1", 1f, 0f, 100f);
            var solvent = new Solvent(2, "S", PhysicalState.LIQUID, 0f, 100f, FlaskType.FLASK_03, "solvent");

            var outcome = new SolubilityOutcome(
                compound, solvent, MixtureType.LL,
                SolubilityResultKind.Soluble, LitmusResultKind.None);

            Assert.AreEqual(FlaskType.FLASK_03, outcome.FlaskType);
        }
    }
}
