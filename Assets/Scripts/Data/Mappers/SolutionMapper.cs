using Domain;

namespace Data
{
    public static class SolutionMapper
    {
        public static Solution ToDomain(this SolutionDto dto)
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
    }
}
