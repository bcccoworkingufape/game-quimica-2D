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
                    var compound = ToDomain(dto);
                    _byId[compound.Id] = compound;
                }
            }
        }

        /// <summary>
        /// Transformacao da representacao externa para a entidade de dominio.
        /// Movida de Data/Mappers/CompoundMapper.cs na Etapa B2, sem alteracao:
        /// state desconhecido continua caindo em LIQUID sem validacao.
        /// </summary>
        private static Compound ToDomain(CompoundDto dto)
        {
            var state = dto.state == "SOLID"
                ? PhysicalState.SOLID
                : PhysicalState.LIQUID;

            return new Compound(
                dto.id,
                dto.name,
                state,
                dto.group,
                dto.density,
                dto.meltingPoint,
                dto.boilingPoint);
        }

        public IReadOnlyList<Compound> ListAll() =>
            new List<Compound>(_byId.Values);   // snapshot

        public Compound GetById(int id) =>
            _byId.TryGetValue(id, out var c) ? c : null;
    }
}
