using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Domain;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// Provider em memoria: serve JSON forjado para um nome de arquivo,
    /// substituindo o acesso a disco sem tocar em nada de producao.
    /// </summary>
    internal sealed class StubJsonProvider : IJsonProvider
    {
        private readonly Dictionary<string, string> _porArquivo;

        public StubJsonProvider(string arquivo, string json)
        {
            _porArquivo = new Dictionary<string, string> { { arquivo, json } };
        }

        public string LoadText(string relativePath) =>
            _porArquivo.TryGetValue(relativePath, out var j) ? j : "[]";
    }

    /// <summary>
    /// Caracteriza a materializacao pelo CAMINHO REAL de producao:
    /// JSON -> IJsonProvider -> repositorio -> entidade de dominio.
    ///
    /// Escrito ANTES da Etapa B2, enquanto os mappers ainda existem como
    /// classes separadas, para provar que estes testes capturam as mesmas
    /// politicas que MappingTests capturava chamando ToDomain() direto.
    /// Depois de B2 o mapeamento fica privado dentro de cada repositorio e
    /// esta suite passa a ser a unica cobertura dessas politicas.
    ///
    /// Diferenca relevante em relacao a testar o mapper isolado: aqui a
    /// excecao sobe do CONSTRUTOR do repositorio, que e exatamente onde ela
    /// sobe em producao — dentro de Bootstrapper.Awake.
    /// </summary>
    [TestFixture]
    public class RepositoryMaterializationTests
    {
        private static ICompoundRepository Compounds(string json) =>
            new JsonCompoundRepository(new StubJsonProvider(DataIndex.CompoundsFile, json));

        private static ISolventRepository Solvents(string json) =>
            new JsonSolventRepository(new StubJsonProvider(DataIndex.SolventsFile, json));

        private static ISolutionRepository Solutions(string json) =>
            new JsonSolutionRepository(new StubJsonProvider(DataIndex.SolubilityFile, json));

        // ------------------------------------------------------- Compound
        [Test]
        public void Compound_SolidState_IsConverted()
        {
            var c = Compounds("[{\"id\":1,\"name\":\"X\",\"state\":\"SOLID\",\"group\":\"S1\"}]")
                .GetById(1);

            Assert.AreEqual(PhysicalState.SOLID, c.State);
        }

        [Test]
        public void Compound_UnknownState_SilentlyFallsBackToLiquid()
        {
            // Politica congelada de C: ternario sem validacao. Qualquer string
            // que nao seja exatamente "SOLID" vira LIQUID, sem aviso.
            foreach (var input in new[] { "\"GAS\"", "\"solid\"", "\"PLASMA\"", "\"\"", "null" })
            {
                var json = "[{\"id\":1,\"name\":\"X\",\"state\":" + input + ",\"group\":\"S1\"}]";
                Assert.AreEqual(PhysicalState.LIQUID, Compounds(json).GetById(1).State,
                    "state=" + input + " cai em LIQUID sem aviso");
            }
        }

        [Test]
        public void Compound_GroupStaysAsFreeString()
        {
            var c = Compounds(
                "[{\"id\":1,\"name\":\"X\",\"state\":\"LIQUID\",\"group\":\"GRUPO_INVENTADO\"}]")
                .GetById(1);

            Assert.AreEqual("GRUPO_INVENTADO", c.Group);
        }

        [Test]
        public void Compound_NumericFieldsAreCarried()
        {
            var c = Compounds(
                "[{\"id\":4,\"name\":\"X\",\"state\":\"LIQUID\",\"group\":\"SB\"," +
                "\"density\":0.7,\"meltingPoint\":-49.1,\"boilingPoint\":77.8}]")
                .GetById(4);

            Assert.AreEqual(0.7f, c.Density, 0.0001f);
            Assert.AreEqual(-49.1f, c.MeltingPoint, 0.0001f);
            Assert.AreEqual(77.8f, c.BoilingPoint, 0.0001f);
        }

        // -------------------------------------------------------- Solvent
        [Test]
        public void Solvent_FlaskTypeIsParsedIgnoringCase()
        {
            var s = Solvents(SolventJson("flask_02")).GetById(1);
            Assert.AreEqual(FlaskType.FLASK_02, s.FlaskType);
        }

        [Test]
        public void Solvent_UnknownFlaskType_ThrowsFromRepositoryConstructor()
        {
            // Contraste deliberado com o campo state do MESMO mapeamento:
            // state tolera, flaskType lanca. Duas politicas contraditorias.
            // Em producao isso derruba Bootstrapper.Awake.
            Assert.Throws<ArgumentException>(() => Solvents(SolventJson("FLASK_05")));
        }

        [Test]
        public void Solvent_UnknownState_FallsBackToLiquid_UnlikeFlaskType()
        {
            var json = SolventJson("FLASK_01").Replace("\"state\":\"LIQUID\"", "\"state\":\"GAS\"");
            Assert.AreEqual(PhysicalState.LIQUID, Solvents(json).GetById(1).State);
        }

        [Test]
        public void Solvent_ChemicalClassComesFromSnakeCaseField()
        {
            // chemical_class -> ChemicalClass. E a justificativa tecnica mais
            // forte para o SolventDto continuar existindo.
            var s = Solvents(SolventJson("FLASK_01")).GetById(1);
            Assert.AreEqual("solvent", s.ChemicalClass);
        }

        // ------------------------------------------------------- Solution
        [Test]
        public void Solution_BothEnumsAreParsedIgnoringCase()
        {
            var s = Solutions(SolutionJson("soluble", "acidic")).GetByIds(1, 1);

            Assert.AreEqual(SolubilityResultKind.Soluble, s.SolubilityResult);
            Assert.AreEqual(LitmusResultKind.Acidic, s.LitmusResult);
        }

        [Test]
        public void Solution_UnknownSolubilityResult_ThrowsFromRepositoryConstructor()
        {
            Assert.Throws<ArgumentException>(() => Solutions(SolutionJson("Dissolvido", "None")));
        }

        [Test]
        public void Solution_UnknownLitmusResult_ThrowsFromRepositoryConstructor()
        {
            Assert.Throws<ArgumentException>(() => Solutions(SolutionJson("Soluble", "Roxo")));
        }

        [Test]
        public void Solution_SolutionNameIsRenamedToName()
        {
            // Unico renomeio real de campo em toda a camada Data.
            var s = Solutions(SolutionJson("Soluble", "None")).GetByIds(1, 1);
            Assert.AreEqual("Solucao de teste", s.Name);
        }

        [Test]
        public void Solution_IsIndexedByCompositeKey()
        {
            const string json =
                "[{\"id\":1,\"compoundId\":2,\"solventId\":3,\"solutionName\":\"A\"," +
                "\"solubilityResult\":\"Soluble\",\"litmusResult\":\"None\"}," +
                "{\"id\":2,\"compoundId\":3,\"solventId\":2,\"solutionName\":\"B\"," +
                "\"solubilityResult\":\"InsolubleSink\",\"litmusResult\":\"None\"}]";

            var repo = Solutions(json);

            Assert.AreEqual("A", repo.GetByIds(2, 3).Name);
            Assert.AreEqual("B", repo.GetByIds(3, 2).Name, "a chave nao e comutativa");
            Assert.IsNull(repo.GetByIds(9, 9));
        }

        // --------------------------------------------------- caso vazio
        [Test]
        public void EmptyJson_ProducesEmptyRepositories()
        {
            Assert.AreEqual(0, Compounds("[]").ListAll().Count);
            Assert.AreEqual(0, Solvents("[]").ListAll().Count);
            Assert.AreEqual(0, Solutions("[]").ListAll().Count);
        }

        // -------------------------------------------------------- helpers
        private static string SolventJson(string flaskType) =>
            "[{\"id\":1,\"name\":\"X\",\"state\":\"LIQUID\",\"meltingPoint\":0.0," +
            "\"boilingPoint\":100.0,\"flaskType\":\"" + flaskType + "\"," +
            "\"chemical_class\":\"solvent\"}]";

        private static string SolutionJson(string solubility, string litmus) =>
            "[{\"id\":1,\"compoundId\":1,\"solventId\":1,\"solutionName\":\"Solucao de teste\"," +
            "\"solubilityResult\":\"" + solubility + "\"," +
            "\"litmusResult\":\"" + litmus + "\"}]";
    }
}
