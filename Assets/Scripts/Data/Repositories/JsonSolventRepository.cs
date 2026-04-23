using System.Collections.Generic;
using Newtonsoft.Json;
using Domain;

namespace Data
{
    public class JsonSolventRepository : ISolventRepository
    {
        private readonly Dictionary<int, Solvent> _byId;

        public JsonSolventRepository(IJsonProvider provider)
        {
            var json = provider.LoadText(DataIndex.SolventsFile);
            var dtos = JsonConvert.DeserializeObject<List<SolventDto>>(json);
            _byId = new Dictionary<int, Solvent>();

            if (dtos != null)
            {
                foreach (var dto in dtos)
                {
                    var solvent = dto.ToDomain();
                    _byId[solvent.Id] = solvent;
                }
            }
        }

        public IReadOnlyList<Solvent> ListAll() =>
            new List<Solvent>(_byId.Values);

        public Solvent GetById(int id) =>
            _byId.TryGetValue(id, out var s) ? s : null;
    }
}
