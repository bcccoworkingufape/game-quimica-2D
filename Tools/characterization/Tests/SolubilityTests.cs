using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// Caracteriza o SolubilityService. Cobre F06, F07 e F08.
    /// F07 e o teste-ancora da paridade: percorre os 80 pares do dataset e
    /// exige que o servico devolva exatamente o que o JSON autora.
    /// </summary>
    [TestFixture]
    public class SolubilityTests
    {
        private ISolubilityService _service;

        [SetUp]
        public void SetUp() => _service = Dataset.SolubilityService();

        // ---------------------------------------------------------------- F07
        [Test]
        public void F07_AllEightyPairs_MatchAuthoredJson()
        {
            var authored = JsonConvert.DeserializeObject<List<RawSolution>>(
                Dataset.RawJson("SolubilityData.json"));

            Assert.AreEqual(80, authored.Count, "o dataset deve ter 80 registros");

            var divergences = new List<string>();

            foreach (var row in authored)
            {
                var outcome = _service.GetOutcome(row.compoundId, row.solventId);

                var expectedSolubility = (SolubilityResultKind)Enum.Parse(
                    typeof(SolubilityResultKind), row.solubilityResult, true);
                var expectedLitmus = (LitmusResultKind)Enum.Parse(
                    typeof(LitmusResultKind), row.litmusResult, true);

                if (outcome.SolubilityResult != expectedSolubility)
                    divergences.Add($"c{row.compoundId}s{row.solventId}: solubility " +
                                    $"esperado {expectedSolubility}, obtido {outcome.SolubilityResult}");

                if (outcome.LitmusResult != expectedLitmus)
                    divergences.Add($"c{row.compoundId}s{row.solventId}: litmus " +
                                    $"esperado {expectedLitmus}, obtido {outcome.LitmusResult}");
            }

            Assert.IsEmpty(divergences,
                "os 80 pares devem reproduzir o JSON: " + string.Join(" | ", divergences));
        }

        [Test]
        public void F07_OutcomeCarriesCompoundAndSolventIdentity()
        {
            var outcome = _service.GetOutcome(1, 1);

            Assert.AreEqual(1, outcome.Compound.Id);
            Assert.AreEqual("Etanoato de etila", outcome.Compound.Name);
            Assert.AreEqual(1, outcome.Solvent.Id);
            Assert.AreEqual("Água", outcome.Solvent.Name);
        }

        [Test]
        public void F07_FlaskTypeIsDelegatedToSolvent()
        {
            // SolubilityOutcome.cs:14 — propriedade derivada, nao campo proprio.
            var outcome = _service.GetOutcome(1, 1);
            Assert.AreEqual(outcome.Solvent.FlaskType, outcome.FlaskType);
        }

        // ---------------------------------------------------------------- F06
        [Test]
        public void F06_MixtureType_IsSolidLiquidOnlyForSolidCompound()
        {
            // A UNICA regra de quimica computada do projeto, SolubilityService.cs:37-40.
            // Compostos SOLID sao os ids 2 e 6; todos os solventes sao LIQUID.
            foreach (var solventId in Enumerable.Range(1, 8))
            {
                Assert.AreEqual(MixtureType.SL, _service.GetOutcome(2, solventId).MixtureType,
                    "composto 2 (SOLID) x solvente " + solventId + " (LIQUID) = SL");
                Assert.AreEqual(MixtureType.SL, _service.GetOutcome(6, solventId).MixtureType,
                    "composto 6 (SOLID) x solvente " + solventId + " (LIQUID) = SL");
            }
        }

        [Test]
        public void F06_MixtureType_IsLiquidLiquidForEveryOtherCompound()
        {
            var liquidCompounds = new[] { 1, 3, 4, 5, 7, 8, 9, 10 };

            foreach (var compoundId in liquidCompounds)
                foreach (var solventId in Enumerable.Range(1, 8))
                    Assert.AreEqual(MixtureType.LL,
                        _service.GetOutcome(compoundId, solventId).MixtureType,
                        "composto " + compoundId + " (LIQUID) x solvente " + solventId + " = LL");
        }

        [Test]
        public void F06_DerivedMixtureType_AgreesWithIgnoredJsonField()
        {
            // SolubilityData.json TEM um campo "mixtureType" que o DTO nao declara
            // e o Newtonsoft descarta em silencio. O valor e recalculado por
            // heuristica, com nomenclatura diferente (LL/SL vs LIQUID_LIQUID).
            // Hoje os dois caminhos concordam. Este teste registra essa
            // concordancia — se ela quebrar, e sinal de dado autoral inconsistente.
            var authored = JsonConvert.DeserializeObject<List<RawSolution>>(
                Dataset.RawJson("SolubilityData.json"));

            foreach (var row in authored)
            {
                var derived = _service.GetOutcome(row.compoundId, row.solventId).MixtureType;
                var expected = row.mixtureType == "SOLID_LIQUID" ? MixtureType.SL : MixtureType.LL;

                Assert.AreEqual(expected, derived,
                    $"c{row.compoundId}s{row.solventId}: JSON diz {row.mixtureType}, servico deriva {derived}");
            }
        }

        // ---------------------------------------------------------------- F08
        [Test]
        public void F08_UnknownCompound_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _service.GetOutcome(99, 1));
        }

        [Test]
        public void F08_UnknownSolvent_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _service.GetOutcome(1, 99));
        }

        [Test]
        public void Constructor_RejectsNullRepositories()
        {
            Assert.Throws<ArgumentNullException>(
                () => new SolubilityService(null, Dataset.Solvents(), Dataset.Solutions()));
            Assert.Throws<ArgumentNullException>(
                () => new SolubilityService(Dataset.Compounds(), null, Dataset.Solutions()));
            Assert.Throws<ArgumentNullException>(
                () => new SolubilityService(Dataset.Compounds(), Dataset.Solvents(), null));
        }

        /// <summary>Espelho cru do JSON, incluindo o campo que o SolutionDto ignora.</summary>
        private class RawSolution
        {
            public int id = 0;
            public int compoundId = 0;
            public int solventId = 0;
            public string solutionName = null;
            public string solubilityResult = null;
            public string litmusResult = null;
            public string mixtureType = null;
        }
    }
}
