using UnityEngine;
using Data;
using Domain;

namespace Core
{
    /// <summary>
    /// Inicializa repositórios, serviços de domínio e registra no ServiceLocator.
    /// Coloque este script em qualquer cena que possa ser usada como ponto de entrada
    /// (ex: 1_MenuScene e 2_LabScene). Ele se garante para não inicializar duas vezes.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        private static bool _initialized = false;

        [Header("Pasta de dados (dentro de StreamingAssets)")]
        private string dataFolder = "Data";

        private void Awake()
        {
            if (_initialized)
            {
                // Já inicializado em outra cena -> descarta este Bootstrapper
                Destroy(gameObject);
                return;
            }

            _initialized = true;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[Bootstrapper] Inicializando serviços...");

            var rootPath = System.IO.Path.Combine(
                Application.streamingAssetsPath,
                dataFolder);

            IJsonProvider provider = new FileJsonProvider(rootPath);

            ICompoundRepository compoundRepo = new JsonCompoundRepository(provider);
            ISolventRepository solventRepo = new JsonSolventRepository(provider);
            ISolutionRepository solutionRepo = new JsonSolutionRepository(provider);

            ISolubilityService solubilityService =
                new SolubilityService(compoundRepo, solventRepo, solutionRepo);

            IHistoryService historyService = new InMemoryHistoryService();
            IScoringService scoringService = new ScoringService();
            //IQuestionService questionService = new QuestionService();

            //ServiceLocator.Register<IQuestionService>(questionService);
            ServiceLocator.Register<ICompoundRepository>(compoundRepo);
            ServiceLocator.Register<ISolventRepository>(solventRepo);
            ServiceLocator.Register<ISolutionRepository>(solutionRepo);

            ServiceLocator.Register<ISolubilityService>(solubilityService);
            ServiceLocator.Register<IHistoryService>(historyService);
            ServiceLocator.Register<IScoringService>(scoringService);

            Debug.Log("[Bootstrapper] Serviços registrados no ServiceLocator.");
        }
    }
}