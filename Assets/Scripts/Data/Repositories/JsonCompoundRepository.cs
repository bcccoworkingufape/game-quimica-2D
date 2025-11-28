using System.Collections.Generic;
using Newtonsoft.Json;
using Domain;

namespace Data
{
    public class JsonCompoundRepository : ICompoundRepository
    {
        private readonly Dictionary<int, Compound> _byId;

        public JsonCompoundRepository(IJsonProvider provider)
        {
            var json = provider.LoadText(DataIndex.CompoundsFile);
            var dtos = JsonConvert.DeserializeObject<List<CompoundDto>>(json);
            _byId = new Dictionary<int, Compound>();

            if (dtos != null)
            {
                foreach (var dto in dtos)
                {
                    var compound = dto.ToDomain();
                    _byId[compound.Id] = compound;
                }
            }
        }

        public IReadOnlyList<Compound> ListAll() =>
            new List<Compound>(_byId.Values);

        public Compound GetById(int id) =>
            _byId.TryGetValue(id, out var c) ? c : null;
    }
}
