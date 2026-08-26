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
                    var solvent = ToDomain(dto);
                    _byId[solvent.Id] = solvent;
                }
            }
        }

        /// <summary>
        /// Transformacao da representacao externa para a entidade de dominio.
        /// Movida de Data/Mappers/SolventMapper.cs na Etapa B2, sem alteracao.
        /// Note que os dois campos de enum tem politicas de erro opostas:
        /// state tolera valor desconhecido e cai em LIQUID; flaskType lanca.
        /// A contradicao e comportamento do Estado C e foi preservada de proposito.
        /// </summary>
        private static Solvent ToDomain(SolventDto dto)
        {
            var state = dto.state == "SOLID"
                ? PhysicalState.SOLID
                : PhysicalState.LIQUID;

            // Enum.Parse com ignoreCase pra ter tolerância
            var flask = (FlaskType)System.Enum.Parse(
                typeof(FlaskType),
                dto.flaskType,
                ignoreCase: true);

            return new Solvent(
                dto.id,
                dto.name,
                state,
                dto.meltingPoint,
                dto.boilingPoint,
                flask,
                dto.chemical_class);
        }

        public IReadOnlyList<Solvent> ListAll() =>
            new List<Solvent>(_byId.Values);

        public Solvent GetById(int id) =>
            _byId.TryGetValue(id, out var s) ? s : null;
    }
}
