using System.Collections.Generic;

namespace Domain
{
    public interface ISolutionRepository
    {
        IReadOnlyList<Solution> ListAll();
        Solution GetByIds(int compoundId, int solventId);
    }
}
