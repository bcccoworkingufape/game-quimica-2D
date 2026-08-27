using System.Linq;
using Domain;
using NUnit.Framework;
using Presentation.Lab;

namespace Molz.Characterization
{
    /// <summary>
    /// Caracteriza a semantica do reagente indicador acido-base (Tornassol).
    ///
    /// Escrito ANTES da Etapa F1, quando a identidade quimica era decidida por
    /// comparacao com o texto de exibicao em dois pontos independentes:
    ///
    ///   LabScripts/LabUIController.cs:217   currentItemName == "Tornassol"
    ///   Presentation/Lab/TestMessageHandler.cs:88  outcome.Solvent.Name == "Tornassol"
    ///
    /// TestMessageHandler e POCO e testavel fora da Unity, entao ele e o seam
    /// que trava o comportamento. LabUIController e MonoBehaviour e permanece
    /// coberto por roteiro manual.
    ///
    /// Os textos esperados sao a ancora de paridade: foram estabelecidos contra
    /// o comportamento de C e nao mudam com a intervencao.
    /// </summary>
    [TestFixture]
    public class IndicatorSemanticsTests
    {
        private const int IdTornassol = 3;

        private static Solvent SolventeDoDataset(int id) =>
            Dataset.Solvents().GetById(id);

        private static SolubilityOutcome Outcome(int compoundId, int solventId) =>
            Dataset.SolubilityService().GetOutcome(compoundId, solventId);

        // ------------------------------------------------ o dataset congelado
        [Test]
        public void Dataset_IndicatorIsSolventThree()
        {
            var s = SolventeDoDataset(IdTornassol);

            Assert.IsNotNull(s);
            Assert.AreEqual("Tornassol", s.Name,
                "a identidade do indicador no dataset congelado e o id 3");
        }

        [Test]
        public void Dataset_NoOtherSolventIsNamedTornassol()
        {
            var todos = Dataset.Solvents().ListAll();
            Assert.AreEqual(1, todos.Count(s => s.Name == "Tornassol"),
                "so um solvente carrega esse nome; id e nome coincidem hoje");
        }

        [Test]
        public void Dataset_OnlyTheIndicatorProducesLitmusReactions()
        {
            // Reforca que a identidade quimica esta ligada ao solvente 3 e nao
            // a um atributo generico: as 4 reacoes de tornassol vem so dele.
            var reativas = Dataset.Solutions().ListAll()
                .Where(s => s.LitmusResult != LitmusResultKind.None)
                .ToList();

            Assert.AreEqual(4, reativas.Count);
            Assert.IsTrue(reativas.All(s => s.SolventId == IdTornassol));
        }

        // ------------------------- caso positivo: mensagem de indicador
        [Test]
        public void Message_ForIndicator_UsesLitmusPattern()
        {
            // Composto 3 x solvente 3 = Acidic (verificado no dataset).
            var msg = new TestMessageHandler().GetTestMessage(
                new MixSolutionResponse(Outcome(3, IdTornassol), null));

            StringAssert.StartsWith("O composto é <b>", msg);
            StringAssert.Contains("vermelho", msg,
                "reacao acida do tornassol descrita pela cor");
            StringAssert.DoesNotContain("Tornassol", msg,
                "o padrao de indicador nao cita o nome do solvente");
        }

        [Test]
        public void Message_ForIndicator_BasicReaction()
        {
            // Composto 2 x solvente 3 = Basic.
            var msg = new TestMessageHandler().GetTestMessage(
                new MixSolutionResponse(Outcome(2, IdTornassol), null));

            StringAssert.Contains("azul", msg);
        }

        // ------------------------- casos negativos: mensagem de solubilidade
        [Test]
        public void Message_ForWater_UsesSolubilityPattern()
        {
            var msg = new TestMessageHandler().GetTestMessage(
                new MixSolutionResponse(Outcome(1, 1), null));

            StringAssert.Contains("Água", msg,
                "o padrao normal cita o nome do solvente");
            StringAssert.DoesNotContain("vermelho", msg);
            StringAssert.DoesNotContain("azul", msg);
        }

        [Test]
        public void Message_ForEther_UsesSolubilityPattern()
        {
            var msg = new TestMessageHandler().GetTestMessage(
                new MixSolutionResponse(Outcome(1, 2), null));

            StringAssert.Contains("Éter dietílico", msg);
            StringAssert.DoesNotContain("vermelho", msg);
        }

        [Test]
        public void Message_ForNaOH_IsNotTreatedAsIndicator()
        {
            // NaOH tem chemical_class "solution", igual ao Tornassol. Este teste
            // prova que chemical_class NAO e o discriminador: apesar de
            // compartilhar a classe, o NaOH usa o padrao de solubilidade.
            var naoh = SolventeDoDataset(4);
            Assert.AreEqual("solution", naoh.ChemicalClass);

            var msg = new TestMessageHandler().GetTestMessage(
                new MixSolutionResponse(Outcome(1, 4), null));

            StringAssert.Contains("NaOH 5%", msg);
            StringAssert.DoesNotContain("vermelho", msg);
            StringAssert.DoesNotContain("azul", msg);
        }

        // ------------------------- historico usa a mesma decisao
        [Test]
        public void HistoryMessage_ForIndicator_KeepsTheSameSemantics()
        {
            var entry = new MixtureHistoryEntry(1, System.DateTime.Now, Outcome(3, IdTornassol));
            var msg = new TestMessageHandler().GetHistoryMessage(entry);

            StringAssert.StartsWith("1) ", msg);
            StringAssert.Contains("vermelho", msg);
        }

        [Test]
        public void HistoryMessage_ForOrdinarySolvent_KeepsTheSameSemantics()
        {
            var entry = new MixtureHistoryEntry(2, System.DateTime.Now, Outcome(1, 1));
            var msg = new TestMessageHandler().GetHistoryMessage(entry);

            StringAssert.StartsWith("2) ", msg);
            StringAssert.Contains("Água", msg);
        }
    }
}
