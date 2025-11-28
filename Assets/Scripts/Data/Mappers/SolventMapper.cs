using Domain;

namespace Data
{
    public static class SolventMapper
    {
        public static Solvent ToDomain(this SolventDto dto)
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
                flask);
        }
    }
}
