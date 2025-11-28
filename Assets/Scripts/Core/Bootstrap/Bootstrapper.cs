using UnityEngine;
using Data;
using Domain;

namespace Core
{
    /// <summary>
    /// Inicializa repositórios, serviços de domínio e registra no ServiceLocator.
    /// Script está em um GameObject na primeira cena (ex: 0_BootstrapScene).
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Pasta de dados (dentro de StreamingAssets)")]
        private string dataFolder = "Data";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            var rootPath = System.IO.Path.Combine(
                Application.streamingAssetsPath,
                dataFolder);

            // Provider que lê os JSONs
            IJsonProvider provider = new FileJsonProvider(rootPath);

            // Repositórios (já fazem cache em memória no construtor)
            ICompoundRepository compoundRepo = new JsonCompoundRepository(provider);
            ISolventRepository solventRepo   = new JsonSolventRepository(provider);
            ISolutionRepository solutionRepo = new JsonSolutionRepository(provider);

            // Serviços de domínio
            ISolubilityService solubilityService = new SolubilityService(
                compoundRepo, solventRepo, solutionRepo);

            IHistoryService historyService = new InMemoryHistoryService();

            // Registro no ServiceLocator
            ServiceLocator.Register<ICompoundRepository>(compoundRepo);
            ServiceLocator.Register<ISolventRepository>(solventRepo);
            ServiceLocator.Register<ISolutionRepository>(solutionRepo);

            ServiceLocator.Register<ISolubilityService>(solubilityService);
            ServiceLocator.Register<IHistoryService>(historyService);
        }
    }
}
