using System.Collections.Generic;
using System.Linq;
using Domain;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// Caracteriza o pipeline JSON -> DTO -> Mapper -> Entidade -> Repositorio.
    /// Cobre F02, F03, F04 e F05 do Functional Parity Manifest.
    /// </summary>
    [TestFixture]
    public class DataLoadingTests
    {
        // ---------------------------------------------------------------- F02
        [Test]
        public void F02_Compounds_LoadAll()
        {
            var repo = Dataset.Compounds();
            var all = repo.ListAll();

            Assert.AreEqual(10, all.Count, "CompoundsData.json tem 10 compostos");
            CollectionAssert.AreEquivalent(
                Enumerable.Range(1, 10).ToArray(),
                all.Select(c => c.Id).ToArray(),
                "ids sao 1..10 sem lacuna");
        }

        [Test]
        public void F02_Compounds_PhysicalStateDistribution()
        {
            var all = Dataset.Compounds().ListAll();

            Assert.AreEqual(8, all.Count(c => c.State == PhysicalState.LIQUID));
            Assert.AreEqual(2, all.Count(c => c.State == PhysicalState.SOLID));

            var solidIds = all.Where(c => c.State == PhysicalState.SOLID)
                              .Select(c => c.Id).OrderBy(i => i).ToArray();
            CollectionAssert.AreEqual(new[] { 2, 6 }, solidIds,
                "os unicos SOLID sao Butanoato de sodio (2) e 4-aminobenzenossulfonamida (6)");
        }

        [Test]
        public void F02_Compounds_GroupsAreTheTenSolubilityClasses()
        {
            var groups = Dataset.Compounds().ListAll()
                                .OrderBy(c => c.Id).Select(c => c.Group).ToArray();

            CollectionAssert.AreEqual(
                new[] { "S1", "S2", "SA", "SB", "A1", "A2", "B", "N1", "N2", "I" },
                groups,
                "um composto por grupo, na ordem dos ids");
        }

        [Test]
        public void F02_Compounds_GetById_KnownValue()
        {
            var c = Dataset.Compounds().GetById(1);

            Assert.IsNotNull(c);
            Assert.AreEqual("Etanoato de etila", c.Name);
            Assert.AreEqual(PhysicalState.LIQUID, c.State);
            Assert.AreEqual("S1", c.Group);
        }

        [Test]
        public void F02_Compounds_GetById_Unknown_ReturnsNull()
        {
            Assert.IsNull(Dataset.Compounds().GetById(999));
        }

        // ---------------------------------------------------------------- F03
        [Test]
        public void F03_Solvents_LoadAll()
        {
            var all = Dataset.Solvents().ListAll();

            Assert.AreEqual(8, all.Count);
            Assert.IsTrue(all.All(s => s.State == PhysicalState.LIQUID),
                "todos os solventes sao LIQUID");
        }

        [Test]
        public void F03_Solvents_FlaskTypeDistribution()
        {
            var all = Dataset.Solvents().ListAll();

            Assert.AreEqual(3, all.Count(s => s.FlaskType == FlaskType.FLASK_01));
            Assert.AreEqual(2, all.Count(s => s.FlaskType == FlaskType.FLASK_02));
            Assert.AreEqual(1, all.Count(s => s.FlaskType == FlaskType.FLASK_03));
            Assert.AreEqual(2, all.Count(s => s.FlaskType == FlaskType.FLASK_04));
        }

        [Test]
        public void F03_Solvents_ChemicalClassIsMappedFromSnakeCase()
        {
            var all = Dataset.Solvents().ListAll();

            // chemical_class no JSON -> ChemicalClass no dominio.
            // E a unica diferenca de convencao de nome que justifica os DTOs.
            Assert.AreEqual(2, all.Count(s => s.ChemicalClass == "solvent"));
            Assert.AreEqual(6, all.Count(s => s.ChemicalClass == "solution"));
            Assert.IsTrue(all.All(s => !string.IsNullOrEmpty(s.ChemicalClass)));
        }

        [Test]
        public void F03_Solvents_LitmusReagentIsIdentifiedOnlyByName()
        {
            // Caracteriza o vazamento descrito no Decision Gate: nada no dominio
            // marca o Tornassol como indicador. chemical_class NAO discrimina —
            // 6 dos 8 solventes sao "solution", incluindo NaOH e HCl.
            var all = Dataset.Solvents().ListAll();
            var tornassol = all.Single(s => s.Name == "Tornassol");

            Assert.AreEqual(3, tornassol.Id);
            Assert.AreEqual("solution", tornassol.ChemicalClass);
            Assert.AreEqual(6, all.Count(s => s.ChemicalClass == "solution"),
                "chemical_class nao isola o indicador: 6 solventes compartilham o valor");
        }

        // ---------------------------------------------------------------- F04
        [Test]
        public void F04_Solutions_LoadFullCartesianProduct()
        {
            var all = Dataset.Solutions().ListAll();

            Assert.AreEqual(80, all.Count, "10 compostos x 8 solventes");

            var pairs = all.Select(s => (s.CompoundId, s.SolventId)).ToList();
            Assert.AreEqual(80, pairs.Distinct().Count(), "nenhum par duplicado");
        }

        [Test]
        public void F04_Solutions_ResultDistribution()
        {
            var all = Dataset.Solutions().ListAll();

            Assert.AreEqual(46, all.Count(s => s.SolubilityResult == SolubilityResultKind.Soluble));
            Assert.AreEqual(24, all.Count(s => s.SolubilityResult == SolubilityResultKind.InsolubleFloat));
            Assert.AreEqual(10, all.Count(s => s.SolubilityResult == SolubilityResultKind.InsolubleSink));

            Assert.AreEqual(76, all.Count(s => s.LitmusResult == LitmusResultKind.None));
            Assert.AreEqual(2, all.Count(s => s.LitmusResult == LitmusResultKind.Basic));
            Assert.AreEqual(1, all.Count(s => s.LitmusResult == LitmusResultKind.Acidic));
            Assert.AreEqual(1, all.Count(s => s.LitmusResult == LitmusResultKind.Neutral));
        }

        [Test]
        public void F04_Solutions_NonNeutralLitmusOnlyWithTornassol()
        {
            var all = Dataset.Solutions().ListAll();
            var reactive = all.Where(s => s.LitmusResult != LitmusResultKind.None).ToList();

            Assert.AreEqual(4, reactive.Count);
            Assert.IsTrue(reactive.All(s => s.SolventId == 3),
                "toda reacao de tornassol vem do solvente id=3");
        }

        [Test]
        public void F04_Solutions_GetByIds_UsesCompositeKey()
        {
            var repo = Dataset.Solutions();

            var s = repo.GetByIds(1, 1);
            Assert.IsNotNull(s);
            Assert.AreEqual(SolubilityResultKind.Soluble, s.SolubilityResult);
            Assert.AreEqual(LitmusResultKind.None, s.LitmusResult);

            Assert.IsNull(repo.GetByIds(99, 99), "par inexistente devolve null");
        }

        // ---------------------------------------------------------------- F05
        [Test]
        public void F05_Questions_LoadAll()
        {
            var all = Dataset.Questions().ListAll();

            Assert.AreEqual(10, all.Count);
            CollectionAssert.AreEqual(
                Enumerable.Range(1, 10).ToArray(),
                all.Select(q => q.CompoundId).ToArray(),
                "uma questao por composto, na ordem do arquivo");
        }

        [Test]
        public void F05_Questions_AlwaysFourAlternatives()
        {
            var all = Dataset.Questions().ListAll();

            Assert.IsTrue(all.All(q => q.Alternatives.Count == 4),
                "o painel da cena tem exatamente 4 slots; alternativas alem da 4a sao descartadas em silencio");
        }

        [Test]
        public void F05_Questions_CorrectAnswerIsAmongAlternatives()
        {
            var all = Dataset.Questions().ListAll();

            foreach (var q in all)
                Assert.Contains(q.CorrectAnswer, (System.Collections.ICollection)q.Alternatives.ToList(),
                    "questao " + q.Id + " tem a resposta certa entre as alternativas");
        }

        [Test]
        public void F05_Questions_NoNullTextFields()
        {
            // Question.cs:33-46 normaliza nulls para string.Empty no construtor.
            // A defesa esta na entidade, nao no QuestionMapper.
            var all = Dataset.Questions().ListAll();

            Assert.IsTrue(all.All(q => q.CorrectAnswer != null));
            Assert.IsTrue(all.All(q => q.Hint != null));
            Assert.IsTrue(all.All(q => q.Feedback != null));
            Assert.IsTrue(all.All(q => q.Description != null));
        }

        [Test]
        public void F05_Questions_GetById_Unknown_ReturnsNull()
        {
            Assert.IsNull(Dataset.Questions().GetById(999));
        }

        [Test]
        public void F05_Questions_EveryFieldIsMaterialized_PinnedContent()
        {
            // Fixa o conteudo real de uma questao campo a campo.
            //
            // Motivo: apos a Etapa B1 a materializacao de Question passa a
            // depender da correspondencia entre os nomes dos parametros do
            // construtor e as chaves do JSON. Renomear um parametro e uma
            // refatoracao que o compilador aceita, mas que quebraria o
            // carregamento em silencio — o campo viraria string.Empty ou 0.
            //
            // Os testes F05 anteriores ja cobrem compoundId, correctAnswer e
            // alternatives. Este fecha a lacuna de id, description, hint e
            // feedback, que passariam despercebidos.
            var q = Dataset.Questions().ListAll().Single(x => x.Id == 1);

            Assert.AreEqual(1, q.Id);
            Assert.AreEqual(1, q.CompoundId);
            Assert.AreEqual("Q1", q.Description);
            Assert.AreEqual("Etanoato de etila", q.CorrectAnswer);
            Assert.AreEqual("Dica: é um éster.", q.Hint);
            Assert.AreEqual(
                "Etanoato de etila é um composto orgânico que forma uma solução incolor quando misturado com água.",
                q.Feedback);
            CollectionAssert.AreEqual(
                new[] { "Etanoato de etila", "Butanoato de sódio", "Cicloexanona", "Decano-1-amina" },
                q.Alternatives.ToList());
        }

        [Test]
        public void F05_Questions_NoFieldIsSilentlyEmpty()
        {
            // Complementa o teste acima em todo o dataset: nenhuma questao pode
            // ter texto vazio. Se a materializacao perder um campo, as 10 caem
            // juntas e a falha e inequivoca.
            var all = Dataset.Questions().ListAll();

            Assert.IsTrue(all.All(q => q.Id > 0), "todo Id foi materializado");
            Assert.IsTrue(all.All(q => q.Description.Length > 0), "toda Description foi materializada");
            Assert.IsTrue(all.All(q => q.CorrectAnswer.Length > 0), "todo CorrectAnswer foi materializado");
            Assert.IsTrue(all.All(q => q.Hint.Length > 0), "todo Hint foi materializado");
            Assert.IsTrue(all.All(q => q.Feedback.Length > 0), "todo Feedback foi materializado");
        }
    }
}
