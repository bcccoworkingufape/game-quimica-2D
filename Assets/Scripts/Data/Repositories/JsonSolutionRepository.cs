using System.Collections.Generic;
using Newtonsoft.Json;
using Domain;

namespace Data
{
    public class JsonSolutionRepository : ISolutionRepository
    {
        private readonly Dictionary<SolutionKey, Solution> _byKey;

        public JsonSolutionRepository(IJsonProvider provider)
        {
            var json = provider.LoadText(DataIndex.SolubilityFile);
            var dtos = JsonConvert.DeserializeObject<List<SolutionDto>>(json);

            _byKey = new Dictionary<SolutionKey, Solution>();

            if (dtos != null)
            {
                foreach (var dto in dtos)
                {
                    var solution = ToDomain(dto);
                    var key = new SolutionKey(solution.CompoundId, solution.SolventId);
                    _byKey[key] = solution;
                }
            }
        }

        /// <summary>
        /// Transformacao da representacao externa para a entidade de dominio.
        /// Movida de Data/Mappers/SolutionMapper.cs na Etapa B2, sem alteracao.
        /// Os dois Enum.Parse seguem sem TryParse e sem try/catch: um valor
        /// desconhecido em qualquer uma das 80 linhas de SolubilityData.json
        /// derruba a inicializacao do jogo. E comportamento do Estado C.
        /// Note tambem que o campo mixtureType do JSON continua nao sendo lido:
        /// o valor e derivado em runtime por SolubilityService.cs:37-40.
        /// </summary>
        private static Solution ToDomain(SolutionDto dto)
        {
            var solRes = (SolubilityResultKind)System.Enum.Parse(
                typeof(SolubilityResultKind),
                dto.solubilityResult,
                ignoreCase: true);

            var litmus = (LitmusResultKind)System.Enum.Parse(
                typeof(LitmusResultKind),
                dto.litmusResult,
                ignoreCase: true);

            return new Solution(
                dto.id,
                dto.compoundId,
                dto.solventId,
                dto.solutionName,
                solRes,
                litmus);
        }

        public IReadOnlyList<Solution> ListAll() =>
            new List<Solution>(_byKey.Values);

        public Solution GetByIds(int compoundId, int solventId)
        {
            var key = new SolutionKey(compoundId, solventId);
            return _byKey.TryGetValue(key, out var s) ? s : null;
        }
    }
}
