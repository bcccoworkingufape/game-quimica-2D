using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// Invariante arquitetural da Etapa C, verificada por varredura de fonte:
    /// nenhuma classe fora de Core/Audio conhece as implementacoes concretas
    /// SfxManager e MusicManager. Os consumidores falam com IAudioService.
    ///
    /// Nao e analise de findings de ferramenta — e uma garantia estrutural do
    /// experimento, checada sobre o texto dos arquivos de producao.
    /// </summary>
    [TestFixture]
    public class AudioBoundaryTests
    {
        /// <summary>
        /// Unicos arquivos autorizados a citar os managers concretos: o adapter
        /// que implementa a porta e os proprios managers.
        /// </summary>
        private static readonly string[] Autorizados =
        {
            Path.Combine("Core", "Audio", "UnityAudioService.cs"),
            Path.Combine("Core", "Audio", "SfxManager.cs"),
            Path.Combine("Core", "Audio", "MusicManager.cs"),
        };

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

        private static IEnumerable<string> Fontes() =>
            Directory.EnumerateFiles(ScriptsDir, "*.cs", SearchOption.AllDirectories);

        private static string Relativo(string full) =>
            full.Substring(ScriptsDir.Length).TrimStart(Path.DirectorySeparatorChar);

        private static bool Autorizado(string rel) =>
            Autorizados.Any(a => string.Equals(a, rel, StringComparison.OrdinalIgnoreCase));

        [Test]
        public void NoConsumerReferencesConcreteAudioSingletons()
        {
            var padrao = new Regex(@"\b(Sfx|Music)Manager\s*\.\s*Instance\b");
            var violacoes = new List<string>();

            foreach (var arquivo in Fontes())
            {
                var rel = Relativo(arquivo);
                if (Autorizado(rel)) continue;

                var linhas = File.ReadAllLines(arquivo);
                for (var i = 0; i < linhas.Length; i++)
                    if (padrao.IsMatch(linhas[i]))
                        violacoes.Add(rel + ":" + (i + 1) + "  " + linhas[i].Trim());
            }

            Assert.IsEmpty(violacoes,
                "acesso ao singleton concreto fora de Core/Audio:\n  " +
                string.Join("\n  ", violacoes));
        }

        [Test]
        public void NoConsumerReferencesConcreteAudioTypesAtAll()
        {
            // Mais estrito que o teste acima: pega tambem declaracao de campo,
            // parametro, cast ou `new` dos tipos concretos — nao so `.Instance`.
            // UnityAudioService e excluido do padrao porque e a implementacao da
            // porta e aparece legitimamente nos pontos de composicao como fallback.
            var padrao = new Regex(@"\b(SfxManager|MusicManager)\b");
            var violacoes = new List<string>();

            foreach (var arquivo in Fontes())
            {
                var rel = Relativo(arquivo);
                if (Autorizado(rel)) continue;

                var linhas = File.ReadAllLines(arquivo);
                for (var i = 0; i < linhas.Length; i++)
                {
                    var linha = linhas[i];
                    // comentarios e XML-doc nao sao dependencia de compilacao
                    var semComentario = Regex.Replace(linha, @"//.*$|///.*$", "");
                    if (padrao.IsMatch(semComentario))
                        violacoes.Add(rel + ":" + (i + 1) + "  " + linha.Trim());
                }
            }

            Assert.IsEmpty(violacoes,
                "referencia a tipo concreto de audio fora de Core/Audio:\n  " +
                string.Join("\n  ", violacoes));
        }

        [Test]
        public void AudioPortExposesAvailabilitySeparateFromEnabledState()
        {
            // A extensao aprovada na Audio Boundary Deviation precisa existir no
            // contrato, senao "manager ausente" volta a colapsar com "desligado".
            var porta = Path.Combine(ScriptsDir, "Core", "Audio", "IAudioService.cs");
            var texto = File.ReadAllText(porta);

            Assert.IsTrue(texto.Contains("IsSfxAvailable"), "IAudioService deve expor IsSfxAvailable");
            Assert.IsTrue(texto.Contains("IsMusicAvailable"), "IAudioService deve expor IsMusicAvailable");
            Assert.IsTrue(texto.Contains("IsSfxEnabled"), "IsSfxEnabled deve continuar existindo");
            Assert.IsTrue(texto.Contains("IsMusicEnabled"), "IsMusicEnabled deve continuar existindo");
        }

        [Test]
        public void AudioPortDoesNotLeakConcreteTypesInItsSignatures()
        {
            var porta = Path.Combine(ScriptsDir, "Core", "Audio", "IAudioService.cs");
            var corpo = File.ReadAllLines(porta)
                .Select(l => Regex.Replace(l, @"//.*$|///.*$", ""))
                .Where(l => !string.IsNullOrWhiteSpace(l));

            var vazamentos = corpo
                .Where(l => Regex.IsMatch(l, @"\b(SfxManager|MusicManager)\b"))
                .ToList();

            Assert.IsEmpty(vazamentos,
                "a assinatura de IAudioService nao pode citar a implementacao concreta:\n  " +
                string.Join("\n  ", vazamentos));
        }

        /// <summary>
        /// Etapa D: nenhum registro global novo pode ter sido introduzido para
        /// substituir o ServiceLocator. A constraint aprovada mantem o registry
        /// existente; ela nao autoriza um sucessor com outro nome.
        /// </summary>
        [Test]
        public void NoReplacementGlobalRegistryWasIntroduced()
        {
            var proibidos = new[]
            {
                "GameServices", "ServiceRegistry", "AppContext",
                "DependencyContainer", "ServiceProvider", "Services.Instance",
            };

            var violacoes = new List<string>();

            foreach (var arquivo in Fontes())
            {
                var rel = Relativo(arquivo);
                var linhas = File.ReadAllLines(arquivo);
                for (var i = 0; i < linhas.Length; i++)
                {
                    var linha = Regex.Replace(linhas[i], @"//.*$|///.*$", "");
                    foreach (var nome in proibidos)
                        if (linha.Contains(nome))
                            violacoes.Add(rel + ":" + (i + 1) + "  " + linhas[i].Trim());
                }
            }

            Assert.IsEmpty(violacoes,
                "registro global substituto introduzido:\n  " + string.Join("\n  ", violacoes));
        }

        /// <summary>
        /// Etapa D1: os tres repositorios de solubilidade chegam ao seu consumidor
        /// por injecao de construtor, nao por lookup global. Se alguem voltar a
        /// registra-los, a cerimonia removida retorna sem ninguem perceber.
        /// </summary>
        [Test]
        public void SolubilityRepositoriesAreNotGloballyRegistered()
        {
            var boot = Path.Combine(ScriptsDir, "Core", "Bootstrap", "Bootstrapper.cs");
            var linhas = File.ReadAllLines(boot)
                .Select(l => Regex.Replace(l, @"//.*$|///.*$", ""))
                .ToList();

            foreach (var tipo in new[] { "ICompoundRepository", "ISolventRepository", "ISolutionRepository" })
                Assert.IsFalse(
                    linhas.Any(l => l.Contains("Register<" + tipo + ">")),
                    tipo + " nao deve ser registrado: chega por construtor em SolubilityService");
        }

        /// <summary>
        /// Etapa D2: o MenuPresenter recebe audio por construtor. Se o lookup
        /// global voltar para dentro dele, a composicao explicita se perde.
        /// </summary>
        [Test]
        public void MenuPresenterReceivesAudioByConstructor()
        {
            var presenter = Path.Combine(ScriptsDir, "Presentation", "Menu", "MenuPresenter.cs");
            var texto = File.ReadAllText(presenter);

            Assert.IsTrue(texto.Contains("MenuPresenter(IMenuView view, IAudioService audio)"),
                "o construtor deve declarar a dependencia de audio");
            Assert.IsFalse(
                Regex.IsMatch(Regex.Replace(texto, @"//.*|///.*", ""), @"ServiceLocator\s*\."),
                "MenuPresenter nao deve mais resolver dependencia globalmente");
        }

        /// <summary>
        /// Etapa F1: a identidade do reagente indicador nao pode voltar a ser
        /// decidida por comparacao com o texto de exibicao. Se um literal
        /// "Tornassol" reaparecer em codigo executavel, a duplicacao volta.
        /// </summary>
        [Test]
        public void IndicatorIdentityIsNotDecidedByDisplayName()
        {
            var violacoes = new List<string>();

            foreach (var arquivo in Fontes())
            {
                var rel = Relativo(arquivo);
                var linhas = File.ReadAllLines(arquivo);
                for (var i = 0; i < linhas.Length; i++)
                {
                    // comentarios podem citar o nome para documentar o dataset
                    var codigo = Regex.Replace(linhas[i], @"//.*$|///.*$", "");
                    if (codigo.Contains("\"Tornassol\""))
                        violacoes.Add(rel + ":" + (i + 1) + "  " + linhas[i].Trim());
                }
            }

            Assert.IsEmpty(violacoes,
                "identidade quimica decidida por texto de exibicao:\n  " +
                string.Join("\n  ", violacoes));
        }

        [Test]
        public void MenuTogglesPreserveAvailabilityGuard()
        {
            // Paridade da secao 4: com o manager indisponivel, o Menu NAO pode
            // sobrescrever a UI de toggle. A guarda deve existir, agora expressa
            // pela porta em vez do singleton concreto.
            var menu = Path.Combine(ScriptsDir, "MenuScripts", "MenuUIController.cs");
            var texto = File.ReadAllText(menu);

            Assert.IsTrue(texto.Contains("!_audio.IsMusicAvailable"),
                "RefreshMusicToggleVisual deve manter a guarda de disponibilidade");
            Assert.IsTrue(texto.Contains("!_audio.IsSfxAvailable"),
                "RefreshSfxToggleVisual deve manter a guarda de disponibilidade");
        }
    }
}
