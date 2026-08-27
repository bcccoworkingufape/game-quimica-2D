using Domain;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// F10 — multiplicador de dificuldade sobre a pontuacao.
    ///
    /// Escrito ANTES da Etapa E1, enquanto a regra ainda esta partida em duas
    /// camadas. O caminho do Estado C e:
    ///
    ///   Domain/Services/ScoringService.cs:17-18   base = 100 + streak*10
    ///   Core/GameManager.cs:228-229               PlayerScore += (int)(base * multiplier)
    ///
    /// Os valores esperados abaixo sao a ancora de paridade: foram estabelecidos
    /// contra o caminho de C e devem continuar valendo depois que a regra for
    /// consolidada. Multiplicadores lidos dos assets reais:
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
        /// Reproduz literalmente o caminho de C: servico de dominio calcula o
        /// base, e a multiplicacao acontece fora dele com cast para int.
        /// </summary>
        private static int PontuacaoFinalComoEmC(bool isCorrect, int streak, float multiplicador)
        {
            var basePoints = new ScoringService().ComputePoints(isCorrect, streak);
            return (int)(basePoints * multiplicador);
        }

        [Test]
        public void F10_Facil_AlwaysScoresZero()
        {
            // scoreMultiplier: 0 no asset. Nao corrigir — comportamento congelado.
            foreach (var streak in new[] { 0, 1, 5, 20 })
                Assert.AreEqual(0, PontuacaoFinalComoEmC(true, streak, Facil),
                    "streak " + streak + " no modo Facil deve pontuar 0");
        }

        [Test]
        public void F10_Medio_ScoresBaseUnchanged()
        {
            Assert.AreEqual(100, PontuacaoFinalComoEmC(true, 0, Medio));
            Assert.AreEqual(110, PontuacaoFinalComoEmC(true, 1, Medio));
            Assert.AreEqual(150, PontuacaoFinalComoEmC(true, 5, Medio));
        }

        [Test]
        public void F10_Dificil_DoublesTheBase()
        {
            Assert.AreEqual(200, PontuacaoFinalComoEmC(true, 0, Dificil));
            Assert.AreEqual(220, PontuacaoFinalComoEmC(true, 1, Dificil));
            Assert.AreEqual(300, PontuacaoFinalComoEmC(true, 5, Dificil));
        }

        [Test]
        public void F10_WrongAnswer_ScoresZeroRegardlessOfMultiplier()
        {
            foreach (var m in new[] { Facil, Medio, Dificil })
                Assert.AreEqual(0, PontuacaoFinalComoEmC(false, 7, m));
        }

        [Test]
        public void F10_TruncationHappensAfterMultiplication()
        {
            // GameManager.cs:229 faz (int)(points * multiplier). Com um
            // multiplicador fracionario o resultado trunca para baixo, e nao
            // arredonda. Nenhum asset usa fracao hoje, mas a politica e do
            // Estado C e precisa sobreviver a consolidacao.
            Assert.AreEqual(50, PontuacaoFinalComoEmC(true, 0, 0.5f));   // 100 * 0.5 = 50
            Assert.AreEqual(55, PontuacaoFinalComoEmC(true, 1, 0.5f));   // 110 * 0.5 = 55
            Assert.AreEqual(52, PontuacaoFinalComoEmC(true, 0, 0.529f)); // 52.9 -> 52
        }

        [Test]
        public void F10_NullDifficultyDefaultsToMultiplierOne()
        {
            // GameManager.cs:228: CurrentDifficulty != null ? scoreMultiplier : 1f
            Assert.AreEqual(100, PontuacaoFinalComoEmC(true, 0, 1f));
        }
    }
}
