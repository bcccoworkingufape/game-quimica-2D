using System;
using Data;
using Domain;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// Caracteriza as politicas de conversao e de erro dos mappers.
    ///
    /// ATENCAO: varios testes aqui registram COMPORTAMENTO DEFEITUOSO de
    /// proposito. Nao sao especificacoes desejadas — sao a fotografia do
    /// Estado C, para que a intervencao C->D possa provar que nao mudou nada.
    /// Corrigir qualquer um deles durante D e desvio e precisa ser registrado.
    /// </summary>
    [TestFixture]
    public class MappingTests
    {
        // ---------------------------------------------------- CompoundMapper
        [Test]
        public void CompoundMapper_ConvertsSolidState()
        {
            var dto = new CompoundDto { id = 1, name = "X", state = "SOLID", group = "S1" };
            Assert.AreEqual(PhysicalState.SOLID, dto.ToDomain().State);
        }

        [Test]
        public void CompoundMapper_UnknownState_SilentlyFallsBackToLiquid()
        {
            // CompoundMapper.cs:9-11 e um ternario sem validacao:
            // qualquer string que nao seja exatamente "SOLID" vira LIQUID.
            foreach (var input in new[] { "GAS", "solid", "PLASMA", "", null })
            {
                var dto = new CompoundDto { id = 1, name = "X", state = input, group = "S1" };
                Assert.AreEqual(PhysicalState.LIQUID, dto.ToDomain().State,
                    "state=" + (input ?? "null") + " cai em LIQUID sem aviso");
            }
        }

        [Test]
        public void CompoundMapper_GroupStaysAsFreeString()
        {
            // Nao ha enum de grupo no dominio; o vocabulario fechado de 10
            // valores do JSON nao e validado em lugar nenhum.
            var dto = new CompoundDto { id = 1, name = "X", state = "LIQUID", group = "GRUPO_INVENTADO" };
            Assert.AreEqual("GRUPO_INVENTADO", dto.ToDomain().Group);
        }

        // ----------------------------------------------------- SolventMapper
        [Test]
        public void SolventMapper_ParsesFlaskTypeIgnoringCase()
        {
            var dto = NewSolventDto("flask_02");
            Assert.AreEqual(FlaskType.FLASK_02, dto.ToDomain().FlaskType);
        }

        [Test]
        public void SolventMapper_UnknownFlaskType_Throws()
        {
            // Contraste deliberado com o campo state do MESMO metodo:
            // state usa ternario tolerante, flaskType usa Enum.Parse que lanca.
            // Duas politicas de erro contraditorias em SolventMapper.cs:9-17.
            var dto = NewSolventDto("FLASK_05");
            Assert.Throws<ArgumentException>(() => dto.ToDomain());
        }

        [Test]
        public void SolventMapper_UnknownState_FallsBackToLiquid_UnlikeFlaskType()
        {
            var dto = NewSolventDto("FLASK_01");
            dto.state = "GAS";
            Assert.AreEqual(PhysicalState.LIQUID, dto.ToDomain().State);
        }

        // ---------------------------------------------------- SolutionMapper
        [Test]
        public void SolutionMapper_ParsesBothEnumsIgnoringCase()
        {
            var dto = NewSolutionDto("soluble", "acidic");
            var s = dto.ToDomain();

            Assert.AreEqual(SolubilityResultKind.Soluble, s.SolubilityResult);
            Assert.AreEqual(LitmusResultKind.Acidic, s.LitmusResult);
        }

        [Test]
        public void SolutionMapper_UnknownSolubilityResult_Throws()
        {
            // Sem try/catch e sem TryParse. Como o repositorio e construido
            // dentro de Bootstrapper.Awake, um typo em qualquer uma das 80
            // linhas de SolubilityData.json derruba a inicializacao do jogo.
            var dto = NewSolutionDto("Dissolvido", "None");
            Assert.Throws<ArgumentException>(() => dto.ToDomain());
        }

        [Test]
        public void SolutionMapper_UnknownLitmusResult_Throws()
        {
            var dto = NewSolutionDto("Soluble", "Roxo");
            Assert.Throws<ArgumentException>(() => dto.ToDomain());
        }

        [Test]
        public void SolutionMapper_RenamesSolutionNameToName()
        {
            // Unico renomeio real de campo em toda a camada Data.
            var dto = NewSolutionDto("Soluble", "None");
            dto.solutionName = "Solucao de teste";
            Assert.AreEqual("Solucao de teste", dto.ToDomain().Name);
        }

        // ---------------------------------------------------- QuestionMapper
        [Test]
        public void QuestionMapper_IsPurePositionalPassThrough()
        {
            // QuestionMapper.cs:8-18 nao converte nem valida nada.
            // Note a inversao de ordem: alternatives entra antes de correctAnswer
            // porque o construtor de Question declara nessa ordem.
            var dto = new QuestionDto
            {
                id = 7,
                compoundId = 3,
                description = "Q7",
                correctAnswer = "B",
                alternatives = new System.Collections.Generic.List<string> { "A", "B", "C" },
                hint = "dica",
                feedback = "feedback"
            };

            var q = dto.ToDomain();

            Assert.AreEqual(7, q.Id);
            Assert.AreEqual(3, q.CompoundId);
            Assert.AreEqual("Q7", q.Description);
            Assert.AreEqual("B", q.CorrectAnswer);
            Assert.AreEqual(3, q.Alternatives.Count);
            Assert.AreEqual("dica", q.Hint);
            Assert.AreEqual("feedback", q.Feedback);
        }

        [Test]
        public void QuestionEntity_NormalizesNullsItself()
        {
            // A defesa esta na entidade (Question.cs:33-36), nao no mapper.
            var dto = new QuestionDto
            {
                id = 1,
                compoundId = 1,
                description = null,
                correctAnswer = null,
                alternatives = null,
                hint = null,
                feedback = null
            };

            var q = dto.ToDomain();

            Assert.AreEqual(string.Empty, q.CorrectAnswer);
            Assert.AreEqual(string.Empty, q.Hint);
            Assert.AreEqual(string.Empty, q.Feedback);
            Assert.IsNotNull(q.Alternatives);
        }

        [Test]
        public void QuestionEntity_EmptyAlternatives_GetsCorrectAnswerAsOnlyOption()
        {
            // Question.cs:39-46 garante pelo menos uma alternativa.
            var dto = new QuestionDto
            {
                id = 1,
                compoundId = 1,
                description = "Q1",
                correctAnswer = "Resposta",
                alternatives = new System.Collections.Generic.List<string>(),
                hint = "",
                feedback = ""
            };

            var q = dto.ToDomain();

            Assert.AreEqual(1, q.Alternatives.Count);
            Assert.AreEqual("Resposta", q.Alternatives[0]);
        }

        // --------------------------------------------------------- auxiliares
        private static SolventDto NewSolventDto(string flaskType)
        {
            return new SolventDto
            {
                id = 1,
                name = "X",
                state = "LIQUID",
                meltingPoint = 0f,
                boilingPoint = 100f,
                flaskType = flaskType,
                chemical_class = "solvent"
            };
        }

        private static SolutionDto NewSolutionDto(string solubility, string litmus)
        {
            return new SolutionDto
            {
                id = 1,
                compoundId = 1,
                solventId = 1,
                solutionName = "X",
                solubilityResult = solubility,
                litmusResult = litmus
            };
        }
    }
}
