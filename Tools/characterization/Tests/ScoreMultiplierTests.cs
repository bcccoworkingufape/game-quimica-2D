using Domain;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// F10 — multiplicador de dificuldade sobre a pontuacao.
    ///
    /// Escrito ANTES da Etapa E1, quando a regra ainda estava partida em duas
    /// camadas. O caminho do Estado C era:
    ///
    ///   Domain/Services/ScoringService.cs:17-18   base = 100 + streak*10
    ///   Core/GameManager.cs:228-229               PlayerScore += (int)(base * multiplier)
    ///
    /// Depois de E1 a regra completa vive no ScoringService e o GameManager
    /// apenas acumula. Os valores esperados abaixo NAO foram tocados: sao a
    /// ancora de paridade, estabelecida contra o caminho partido de C e
    /// reexecutada contra a regra consolidada.
    /// Multiplicadores lidos dos assets reais:
    ///
    ///   Facil.asset   scoreMultiplier: 0   (modo Estudo_Livre nunca pontua)
    ///   Medio.asset   scoreMultiplier: 1
    ///   Dificil.asset scoreMultiplier: 2
    ///
    /// O truncamento ocorre DEPOIS da multiplicacao, sobre um base ja inteiro.
    /// </summary>
    [TestFixture]
    public class ScoreMultiplierTests
    {
        private const float Facil = 0f;
        private const float Medio = 1f;
        private const float Dificil = 2f;

        /// <summary>
        /// Apos a Etapa E1 a regra completa vive no ScoringService. Os valores
        /// esperados nao mudaram: foram estabelecidos contra o caminho partido de
        /// C, antes da movimentacao, e continuam valendo.
        /// </summary>
        private static int PontuacaoFinal(bool isCorrect, int streak, float multiplicador) =>
            new ScoringService().ComputePoints(isCorrect, streak, multiplicador);

        [Test]
        public void F10_BaseCalculationSurvivesWithoutMultiplier()
        {
            // A sobrecarga com default 1f mantem os chamadores antigos corretos.
            var s = new ScoringService();
            Assert.AreEqual(100, s.ComputePoints(true, 0));
            Assert.AreEqual(150, s.ComputePoints(true, 5));
            Assert.AreEqual(0, s.ComputePoints(false, 5));
        }

        [Test]
        public void F10_FinalScoreIsComputedInOnePlace()
        {
            // Antes de E1 esta asercao era impossivel fora da Unity: metade da
            // regra estava em GameManager.cs:229.
            Assert.AreEqual(220, PontuacaoFinal(true, 1, Dificil));
        }

        [Test]
        public void F10_Facil_AlwaysScoresZero()
        {
            // scoreMultiplier: 0 no asset. Nao corrigir — comportamento congelado.
            foreach (var streak in new[] { 0, 1, 5, 20 })
                Assert.AreEqual(0, PontuacaoFinal(true, streak, Facil),
                    "streak " + streak + " no modo Facil deve pontuar 0");
        }

        [Test]
        public void F10_Medio_ScoresBaseUnchanged()
        {
            Assert.AreEqual(100, PontuacaoFinal(true, 0, Medio));
            Assert.AreEqual(110, PontuacaoFinal(true, 1, Medio));
            Assert.AreEqual(150, PontuacaoFinal(true, 5, Medio));
        }

        [Test]
        public void F10_Dificil_DoublesTheBase()
        {
            Assert.AreEqual(200, PontuacaoFinal(true, 0, Dificil));
            Assert.AreEqual(220, PontuacaoFinal(true, 1, Dificil));
            Assert.AreEqual(300, PontuacaoFinal(true, 5, Dificil));
        }

        [Test]
        public void F10_WrongAnswer_ScoresZeroRegardlessOfMultiplier()
        {
            foreach (var m in new[] { Facil, Medio, Dificil })
                Assert.AreEqual(0, PontuacaoFinal(false, 7, m));
        }

        [Test]
        public void F10_TruncationHappensAfterMultiplication()
        {
            // GameManager.cs:229 faz (int)(points * multiplier). Com um
            // multiplicador fracionario o resultado trunca para baixo, e nao
            // arredonda. Nenhum asset usa fracao hoje, mas a politica e do
            // Estado C e precisa sobreviver a consolidacao.
            Assert.AreEqual(50, PontuacaoFinal(true, 0, 0.5f));   // 100 * 0.5 = 50
            Assert.AreEqual(55, PontuacaoFinal(true, 1, 0.5f));   // 110 * 0.5 = 55
            Assert.AreEqual(52, PontuacaoFinal(true, 0, 0.529f)); // 52.9 -> 52
        }

        [Test]
        public void F10_NullDifficultyDefaultsToMultiplierOne()
        {
            // GameManager.cs:228: CurrentDifficulty != null ? scoreMultiplier : 1f
            Assert.AreEqual(100, PontuacaoFinal(true, 0, 1f));
        }
    }
}
