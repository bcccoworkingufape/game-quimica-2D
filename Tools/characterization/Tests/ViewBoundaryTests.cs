using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// Invariantes estruturais da Etapa G, verificadas por varredura de fonte.
    ///
    /// Os dois orquestradores do laboratorio nao podem voltar a conhecer o
    /// MonoBehaviour concreto da View nem a manipular componentes de UI dela.
    /// Os checks descrevem relacoes, nunca numeros de linha.
    /// </summary>
    [TestFixture]
    public class ViewBoundaryTests
    {
        private static string ScriptsDir
        {
            get
            {
                var dir = new DirectoryInfo(AppContext.BaseDirectory);
                while (dir != null)
                {
                    var candidate = Path.Combine(dir.FullName, "Assets", "Scripts");
                    if (Directory.Exists(candidate)) return candidate;
                    dir = dir.Parent;
                }
                throw new DirectoryNotFoundException("Assets/Scripts nao encontrado");
            }
        }

        private static string Ler(params string[] partes) =>
            File.ReadAllText(Path.Combine(new[] { ScriptsDir }.Concat(partes).ToArray()));

        /// <summary>Codigo executavel, sem comentarios de linha nem XML-doc.</summary>
        private static IEnumerable<string> Codigo(string texto) =>
            texto.Split('\n')
                 .Select(l => Regex.Replace(l, @"//.*$|///.*$", ""))
                 .Where(l => !string.IsNullOrWhiteSpace(l));

        private static readonly string[] Orquestradores =
        {
            Path.Combine("Presentation", "Lab", "MixingRoundController.cs"),
            Path.Combine("Presentation", "Lab", "QuestionFlowPresenter.cs"),
        };

        [Test]
        public void OrchestratorsDoNotReferenceTheConcreteView()
        {
            var violacoes = new List<string>();

            foreach (var rel in Orquestradores)
            {
                var linhas = Codigo(File.ReadAllText(Path.Combine(ScriptsDir, rel))).ToList();
                foreach (var l in linhas.Where(l => Regex.IsMatch(l, @"\bLabUIController\b")))
                    violacoes.Add(rel + "  " + l.Trim());
            }

            Assert.IsEmpty(violacoes,
                "orquestrador voltou a conhecer o tipo concreto da View:\n  " +
                string.Join("\n  ", violacoes));
        }

        [Test]
        public void OrchestratorsDoNotScanTheSceneForTheView()
        {
            var violacoes = new List<string>();

            foreach (var rel in Orquestradores)
            {
                var linhas = Codigo(File.ReadAllText(Path.Combine(ScriptsDir, rel))).ToList();
                foreach (var l in linhas.Where(l =>
                             Regex.IsMatch(l, @"Find\w*ObjectsByType\s*<\s*LabUIController") ||
                             Regex.IsMatch(l, @"FindAnyObjectByType\s*<\s*LabUIController")))
                    violacoes.Add(rel + "  " + l.Trim());
            }

            Assert.IsEmpty(violacoes,
                "varredura de cena para obter a View:\n  " + string.Join("\n  ", violacoes));
        }

        [Test]
        public void OrchestratorsDoNotTouchUnityUiComponents()
        {
            // O vazamento removido em G eram acessos do tipo
            // uiController.solutionAnimationText.text e uiController.questionPanelTitle.text.
            var proibidos = new[]
            {
                "TextMeshProUGUI", "TMP_Text", "solutionAnimationText", "questionPanelTitle",
            };
            var violacoes = new List<string>();

            foreach (var rel in Orquestradores)
            {
                var linhas = Codigo(File.ReadAllText(Path.Combine(ScriptsDir, rel))).ToList();
                foreach (var l in linhas)
                    foreach (var p in proibidos)
                        if (l.Contains(p))
                            violacoes.Add(rel + "  " + l.Trim());
            }

            Assert.IsEmpty(violacoes,
                "orquestrador manipulando componente de UI da View:\n  " +
                string.Join("\n  ", violacoes));
        }

        [Test]
        public void ViewImplementsTheThreeClientContracts()
        {
            var texto = Ler("LabScripts", "LabUIController.cs");
            var declaracao = Codigo(texto).First(l => l.Contains("class LabUIController"));

            foreach (var contrato in new[] { "ILabView", "IMixingView", "IQuestionView" })
                StringAssert.Contains(contrato, declaracao,
                    "LabUIController deve implementar " + contrato);
        }

        [Test]
        public void LabViewContractWasNotExpandedIntoAnAggregator()
        {
            // Resultado deliberado de G: as operacoes dos orquestradores NAO foram
            // somadas a ILabView. Se aparecerem la, os papeis voltaram a se misturar.
            var texto = Ler("Presentation", "Lab", "LabView.cs");
            var corpo = string.Join("\n", Codigo(texto));

            foreach (var operacao in new[]
                     {
                         "ShowConfirmationPanel", "SetSolutionResultText", "ShowSolutionAnimationPanel",
                         "SetQuestionTitle", "SetupQuestionPanel", "ShowQuestionPanel",
                         "ShowQuestionVictoryPanel", "ShowQuestionErrorPanel",
                     })
                Assert.IsFalse(corpo.Contains(operacao),
                    "ILabView nao deve absorver a operacao " + operacao +
                    ": ela pertence ao contrato do orquestrador");
        }

        [Test]
        public void OrchestratorContractsStayMinimal()
        {
            // Cada contrato deve refletir apenas acessos reais observados no mapa.
            // Se crescerem sem consumidor, viram a mesma interface agregadora que
            // G rejeitou — so que com outro nome.
            var mixing = Codigo(Ler("Presentation", "Lab", "MixingView.cs"))
                .Count(l => l.TrimEnd().EndsWith(");"));
            var question = Codigo(Ler("Presentation", "Lab", "QuestionView.cs"))
                .Count(l => l.TrimEnd().EndsWith(");"));

            Assert.AreEqual(3, mixing, "IMixingView tem 3 operacoes");
            Assert.AreEqual(5, question, "IQuestionView tem 5 operacoes");
        }
    }
}
