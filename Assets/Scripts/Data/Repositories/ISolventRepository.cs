using System.Collections.Generic;

namespace Domain
{
    public interface ICompoundRepository
    {
        IReadOnlyList<Compound> ListAll();
        Compound GetById(int id);
    }
}
