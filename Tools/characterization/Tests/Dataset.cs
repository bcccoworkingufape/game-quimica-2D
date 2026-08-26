using System;
using System.IO;
using Data;
using Domain;

namespace Molz.Characterization
{
    /// <summary>
    /// Localiza o dataset real do jogo e monta os repositorios de producao
    /// exatamente como o Bootstrapper faz, trocando apenas o IJsonProvider:
    /// ResourcesJsonProvider (UnityEngine) por FileJsonProvider (System.IO).
    ///
    /// Os dois leem o MESMO arquivo. A unica diferenca de contrato e que
    /// ResourcesJsonProvider descarta subdiretorios via
    /// Path.GetFileNameWithoutExtension (ResourcesJsonProvider.cs:29) enquanto
    /// FileJsonProvider honra o caminho (FileJsonProvider.cs:19). Como
    /// DataIndex so entrega nomes planos, os dois sao equivalentes aqui.
    /// </summary>
    public static class Dataset
    {
        private static string _dataDir;

        /// <summary>Assets/Resources/Data do repositorio, achado subindo a arvore.</summary>
        public static string DataDir
        {
            get
            {
                if (_dataDir != null) return _dataDir;

                var dir = new DirectoryInfo(AppContext.BaseDirectory);
                while (dir != null)
                {
                    var candidate = Path.Combine(dir.FullName, "Assets", "Resources", "Data");
                    if (Directory.Exists(candidate))
                    {
                        _dataDir = candidate;
                        return _dataDir;
                    }
                    dir = dir.Parent;
                }

                throw new DirectoryNotFoundException(
                    "Assets/Resources/Data nao encontrado a partir de " + AppContext.BaseDirectory);
            }
        }

        public static IJsonProvider Provider() => new FileJsonProvider(DataDir);

        public static string RawJson(string fileName) =>
            File.ReadAllText(Path.Combine(DataDir, fileName));

        public static ICompoundRepository Compounds() => new JsonCompoundRepository(Provider());
        public static ISolventRepository Solvents() => new JsonSolventRepository(Provider());
        public static ISolutionRepository Solutions() => new JsonSolutionRepository(Provider());
        public static IQuestionRepository Questions() => new JsonQuestionRepository(Provider());

        /// <summary>
        /// Reproduz a composicao de Bootstrapper.cs:44-45 — a unica injecao de
        /// construtor real do Estado C.
        /// </summary>
        public static ISolubilityService SolubilityService() =>
            new SolubilityService(Compounds(), Solvents(), Solutions());
    }
}
