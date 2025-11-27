using System.Collections.Generic;
using System.Text.Json;
using Domain;

namespace Data
{
    public class JsonSolutionRepository : ISolutionRepository
    {
        private readonly Dictionary<SolutionKey, Solution> _byKey;

        public JsonSolutionRepository(IJsonProvider provider)
        {
            var json = provider.LoadText(DataIndex.SolubilityFile);
            var dtos = JsonSerializer.Deserialize<List<SolutionDto>>(json);
            _byKey = new Dictionary<SolutionKey, Solution>();

            if (dtos != null)
            {
                foreach (var dto in dtos)
                {
                    var solution = dto.ToDomain();
                    var key = new SolutionKey(solution.CompoundId, solution.SolventId);
                    _byKey[key] = solution;
                }
            }
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
