using Domain;

namespace Data
{
    public static class CompoundMapper
    {
        public static Compound ToDomain(this CompoundDto dto)
        {
            var state = dto.state == "SOLID"
                ? AggregateState.SOLID
                : AggregateState.LIQUID;

            return new Compound(
                dto.id,
                dto.name,
                state,
                dto.group,
                dto.density,
                dto.meltingPoint,
                dto.boilingPoint);
        }
    }
}
